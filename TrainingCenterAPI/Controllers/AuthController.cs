using Microsoft.AspNetCore.Mvc;
using TrainingCenterAPI.DTOs.Auth;
using TrainingCenterAPI.Services.Security;
using TrainingCenterAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TrainingCenterAPI.Enums;
using TrainingCenterAPI.Repositories.Interfaces;
using TrainingCenterAPI.Utilities.Helpers;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using TrainingCenterAPI.Utilities.Exceptions;
using Microsoft.Extensions.Options;
using TrainingCenterAPI.Configurations;



namespace TrainingCenterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IInstructorRepository _instructorRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly JwtSettings _jwtSettings;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAuditService _auditService;


        public AuthController(
            IStudentRepository studentRepository,
            IInstructorRepository instructorRepository,
            IPersonRepository personRepository,
            IPasswordHasher passwordHasher,
            IOptions<JwtSettings> jwtSettings,
            IRefreshTokenService refreshTokenService,
            IAuditService auditService)
        {
            _studentRepository = studentRepository;
            _instructorRepository = instructorRepository;
            _personRepository = personRepository;
            _passwordHasher = passwordHasher;
            _jwtSettings = jwtSettings.Value;
            _refreshTokenService = refreshTokenService;
            _auditService = auditService;
        }

        // =========================
        // Register Student
        // =========================

        [HttpPost("register/student")]
        public async Task<ActionResult<LoginResponseDto>> RegisterStudent(
            [FromBody] RegisterRequestDto dto)
        {
            var existingPerson = await _personRepository.GetByEmailAsync(dto.Email);

            BusinessRuleHelper.ThrowIfExists(
                existingPerson != null,
                "Email already registered.");

            // 1. Create Person
            var person = new Person
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                PhoneNumber = dto.PhoneNumber,
                Role = UserRole.Student
            };

            await _personRepository.AddAsync(person);
            await _personRepository.SaveChangesAsync();

            // 2. Create Student
            var student = new Student
            {
                PersonId = person.PersonId,
                RegisteredAt = DateTime.UtcNow,
                Status = StudentStatus.Active
            };

            await _studentRepository.AddAsync(student);
            await _studentRepository.SaveChangesAsync();

            // 3. Generate JWT
            var token = GenerateJwtToken(
                student.StudentId,
                person.Email,
                person.Role);

            var refreshToken =
                await _refreshTokenService.CreateAsync(person.PersonId);


            return Ok(new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        // =========================
        // Register Instructor
        // =========================

        [HttpPost("register/instructor")]
        public async Task<ActionResult<LoginResponseDto>> RegisterInstructor(
            [FromBody] RegisterInstructorRequestDto dto)
        {
            var existingPerson = await _personRepository.GetByEmailAsync(dto.Email);

            BusinessRuleHelper.ThrowIfExists(
                existingPerson != null,
                "Email already registered.");

            // 1. Create Person
            var person = new Person
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = UserRole.Instructor
            };

            await _personRepository.AddAsync(person);
            await _personRepository.SaveChangesAsync();

            // 2. Create Instructor
            var instructor = new Instructor
            {
                PersonId = person.PersonId,
                HireDate = dto.HireDate,
                Salary = 0,
                ManagerId = null,
                IsActive = true
            };

            await _instructorRepository.AddAsync(instructor);
            await _instructorRepository.SaveChangesAsync();

            // 3. Generate JWT
            var token = GenerateJwtToken(
                instructor.InstructorId,
                person.Email,
                person.Role);

            var refreshToken =
                await _refreshTokenService.CreateAsync(person.PersonId);


            return Ok(new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        // =========================
        // Login
        // =========================

        [HttpPost("login")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<LoginResponseDto>> Login(
            [FromBody] LoginRequestDto dto)
        {
            var person = await _personRepository.GetByEmailAsync(dto.Email);

            // Check if account is temporarily locked
            if (person != null &&
                person.LockedUntil != null &&
                person.LockedUntil > DateTime.UtcNow)
            {
                return Unauthorized(new
                {
                    error = "Account temporarily locked. Try again later."
                });
            }

            if (person == null)
            {
                await _auditService.LogAsync(
                    null,
                    "LoginFailed",
                    $"Failed login attempt for email {dto.Email}");

                return Unauthorized(new { error = "Invalid email or password." });
            }

            /// <summary>
            /// Account Lockout and Brute Force Protection
            ///
            /// This feature protects user accounts from repeated failed login attempts by
            /// tracking unsuccessful authentication attempts and temporarily locking an
            /// account after exceeding the allowed failure limit.
            ///
            /// Implementation:
            ///
            /// - FailedLoginAttempts:
            ///   Stores the number of consecutive failed login attempts for a user.
            ///
            /// - LockedUntil:
            ///   Stores the time until which the account remains temporarily locked.
            ///
            /// Flow:
            ///
            /// User login attempt
            ///        ↓
            /// Check if account is locked
            ///        ↓
            /// Validate password
            ///        ↓
            /// Wrong password:
            ///        ↓
            /// Increase failed attempts counter
            ///        ↓
            /// After 5 failed attempts:
            ///        ↓
            /// Lock account for 15 minutes
            ///
            /// Successful login:
            ///        ↓
            /// Reset failed attempts
            ///        ↓
            /// Remove account lock
            ///
            /// Benefits:
            ///
            /// 1. Protection Against Brute Force Attacks:
            ///    Prevents attackers from continuously guessing passwords for the same
            ///    account.
            ///
            /// 2. Account-Level Security:
            ///    Protection is applied to the user account itself, regardless of the
            ///    attacker's IP address or location.
            ///
            /// 3. Improved Authentication Security:
            ///    Adds another security layer on top of password hashing and JWT
            ///    authentication.
            ///
            /// Difference Between Account Lockout and Rate Limiting:
            ///
            /// Rate Limiting:
            /// - Protects the API endpoint.
            /// - Limits the number of requests sent to the server within a specific time.
            /// - Example:
            ///
            ///     100 login requests from the same IP in one minute
            ///          ↓
            ///     API returns HTTP 429 Too Many Requests
            ///
            /// - Main goal:
            ///     Protect server resources and prevent request flooding.
            ///
            /// Account Lockout:
            /// - Protects the user account.
            /// - Tracks failed authentication attempts for a specific user.
            /// - Example:
            ///
            ///     Wrong password 5 times for admin account
            ///          ↓
            ///     Admin account locked for 15 minutes
            ///
            /// - Main goal:
            ///     Prevent password guessing attacks against user accounts.
            ///
            /// Production authentication systems commonly use both:
            ///
            /// Rate Limiting protects the API,
            /// Account Lockout protects user identities.
            ///
            /// Together they provide stronger defense against brute force attacks.
            /// </summary>zc

            if (!_passwordHasher.VerifyPassword(
                                    dto.Password,
                                    person.PasswordHash))
            {
                person.FailedLoginAttempts++;

                if (person.FailedLoginAttempts >= 5)
                {
                    person.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                    person.FailedLoginAttempts = 0;
                }

                await _personRepository.SaveChangesAsync();

                await _auditService.LogAsync(
                    person.PersonId,
                    "LoginFailed",
                    "Invalid password.");

                return Unauthorized(new
                {
                    error = "Invalid email or password."
                });
            }

            int userId;

            if (person.Role == UserRole.Student)
            {
                var student = await _studentRepository
                    .GetByPersonIdAsync(person.PersonId);

                if (student == null)
                    return Unauthorized(new { error = "Student account not found." });

                userId = student.StudentId;
            }
            else if (person.Role == UserRole.Instructor)
            {
                var instructor = await _instructorRepository
                    .GetByPersonIdAsync(person.PersonId);

                if (instructor == null)
                    return Unauthorized(new { error = "Instructor account not found." });

                userId = instructor.InstructorId;
            }
            else if (person.Role == UserRole.Admin)
            {
                userId = person.PersonId;
            }
            else
            {
                return Unauthorized(new { error = "Invalid user role." });
            }

            person.FailedLoginAttempts = 0;
            person.LockedUntil = null;

            await _personRepository.SaveChangesAsync();

            var token = GenerateJwtToken(
                        userId,
                        person.Email,
                        person.Role);


            var refreshToken =
                await _refreshTokenService.CreateAsync(person.PersonId);

            await _auditService.LogAsync(
                            person.PersonId,
                            "LoginSuccess",
                            "User logged in successfully.");

            return Ok(new LoginResponseDto
            {
                Token = token,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        // 
        //
        // 
        [HttpPost("refresh-token")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> RefreshToken(
            RefreshTokenRequestDto dto)
        {
            var storedToken =
                await _refreshTokenService
                    .GetActiveTokenAsync(dto.RefreshToken);


            if (storedToken == null)
            {
                await _auditService.LogAsync(
                    null,
                    "RefreshTokenFailed",
                    "Invalid refresh token.");

                throw new UnauthorizedException(
                    "Invalid refresh token.");
            }


            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                await _auditService.LogAsync(
                    storedToken.PersonId,
                    "RefreshTokenFailed",
                    "Expired refresh token.");

                throw new UnauthorizedException(
                    "Refresh token expired.");
            }


            if (storedToken.RevokedAt != null)
            {
                await _auditService.LogAsync(
                    storedToken.PersonId,
                    "RefreshTokenFailed",
                    "Revoked refresh token.");

                throw new UnauthorizedException(
                    "Refresh token revoked.");
            }


            // Revoke old refresh token (rotation)
            await _refreshTokenService
                .RevokeAsync(storedToken);


            // Generate new JWT
            var newJwt =
                GenerateJwtToken(
                    storedToken.Person.PersonId,
                    storedToken.Person.Email,
                    storedToken.Person.Role);



            // Generate new refresh token
            var newRefreshToken =
                await _refreshTokenService
                    .CreateAsync(
                        storedToken.PersonId);

            await _auditService.LogAsync(
                            storedToken.PersonId,
                            "RefreshToken",
                            "JWT refreshed.");

            return Ok(new
            {
                token = newJwt,

                refreshToken =
                    newRefreshToken.Token,

                expiresAt =
                    newRefreshToken.ExpiresAt
            });
        }

        //
        //
        //

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
            RefreshTokenRequestDto dto)
        {
            var token =
                await _refreshTokenService
                    .GetActiveTokenAsync(dto.RefreshToken);


            if (token == null)
            {
                return BadRequest(
                    "Token already invalid");
            }


            await _refreshTokenService
                .RevokeAsync(token);

            await _auditService.LogAsync(
                            token.PersonId,
                            "Logout",
                            "User logged out.");

            return NoContent();
        }

        // =========================
        // JWT
        // =========================
        private string GenerateJwtToken(
            int id,
            string email,
            UserRole role)
        {
            var secretKey = _jwtSettings.Key
                ?? throw new InvalidOperationException(
                    "JWT secret key is not configured.");


            var claims = new[]
            {
        new Claim(
            JwtRegisteredClaimNames.Sub,
            id.ToString()),

        new Claim(
            JwtRegisteredClaimNames.Email,
            email),

        new Claim(
            ClaimTypes.Role,
            role.ToString()),

        new Claim(
            JwtRegisteredClaimNames.Jti,
            Guid.NewGuid().ToString())
    };


            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey));


            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);



            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                             _jwtSettings.ExpiryMinutes),
                signingCredentials: credentials);



            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}