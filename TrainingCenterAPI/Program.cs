using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
using System.Threading.RateLimiting;
using TrainingCenterAPI.Configurations;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.Repositories.Implementations;
using TrainingCenterAPI.Repositories.Interfaces;
using TrainingCenterAPI.Services.Implementations;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.Services.Security;


/// <summary>
/// Serilog is a structured logging library used to replace the default
/// ASP.NET Core logging provider with more powerful and searchable logs.
///
/// Benefits:
/// 
/// 1. Structured Logging:
///    Logs are stored as structured data instead of plain text, making them
///    easier to search, filter, and analyze in production environments.
///
/// 2. Better Debugging:
///    Captures detailed information such as request paths, exceptions,
///    database queries, timestamps, and log levels.
///
/// 3. Production Monitoring:
///    Integrates with logging platforms such as Seq, Elasticsearch,
///    Application Insights, and Grafana Loki for centralized monitoring.
///
/// 4. Correlation Tracking:
///    Works with Correlation IDs to connect all logs belonging to the same
///    HTTP request, making it easier to trace errors across the application.
///
/// 5. Exception Logging:
///    Captures complete exception details including stack traces, making
///    troubleshooting easier.
///
/// Example flow:
///
/// Request
///    ↓
/// Correlation ID Middleware
///    ↓
/// Controller / Service / Repository
///    ↓
/// Serilog captures logs with request information
///    ↓
/// Developer can trace the complete request lifecycle
///
/// Configuration:
/// Serilog is registered in Program.cs and replaces the default logging
/// system using builder.Host.UseSerilog().
///
/// .Enrich.FromLogContext() allows Serilog to include contextual data
/// such as Correlation IDs added through logging scopes.
/// </summary>

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog();


// ===============================
// Controllers
// ===============================

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


// ===============================
// Swagger + JWT
// ===============================

builder.Services.AddSwaggerGen(options =>
{
    options.CustomOperationIds(apiDescription =>
        apiDescription.TryGetMethodInfo(out var methodInfo)
            ? methodInfo.Name
            : null);


    options.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Example = new OpenApiString("2005-12-03")
    });


    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter: Bearer {your JWT token}"
        });


    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});


// ===============================
// Database
// ===============================

builder.Services.AddDbContext<TrainingCenterDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
            .GetConnectionString("DefaultConnection")));


// ===============================
// Health Checks
// ===============================

///<summery>
/// A health check is an endpoint that tells you if your API is alive and its important dependencies are working.
/// Why do we need it?
/// Imagine your API is deployed on a server.
/// The server is running, but:
/// SQL Server database crashed ❌
/// Connection string is wrong ❌
/// Database is unreachable ❌
/// The server itself says:
/// "I am running"
/// but your application is actually broken.
/// Health checks allow monitoring systems to ask:
/// "Can this API actually do its job?"
/// </summery>

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<TrainingCenterDbContext>();

// ===============================
// JWT Configuration
// ===============================

var jwtSettings =
    builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");


// Environment variable has priority
// over appsettings.json

var secretKey =
    Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? jwtSettings.Key;


if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException(
        "JWT secret key is not configured.");
}


builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateDataAnnotations()
    .ValidateOnStart();


// ===============================
// Authentication
// ===============================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,

                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });


// ===============================
// Authorization
// ===============================

builder.Services.AddAuthorization(options =>
{

    options.AddPolicy(
        "OwnStudentOrAdmin",
        policy =>
        {
            policy.Requirements
                .Add(new StudentOwnershipRequirement());
        });


    options.AddPolicy(
        "OwnInstructorOrAdmin",
        policy =>
        {
            policy.Requirements
                .Add(new InstructorOwnershipRequirement());
        });


    options.AddPolicy(
        "OwnEnrollmentOrAdmin",
        policy =>
        {
            policy.Requirements
                .Add(new EnrollmentOwnershipRequirement());
        });


    options.AddPolicy(
        "AdminOnly",
        policy =>
        {
            policy.RequireRole("Admin");
        });

});


// ===============================
// Security Services
// ===============================

builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<IAuthorizationHandler,
    StudentOwnershipHandler>();

builder.Services.AddScoped<IAuthorizationHandler,
    InstructorOwnershipHandler>();

builder.Services.AddScoped<IAuthorizationHandler,
    EnrollmentOwnershipHandler>();


builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// ===============================
// Output Cache
// ===============================
//
// Caches responses for read-only endpoints.
// Reduces repeated database queries and improves API performance.

builder.Services.AddOutputCache();
builder.Services.AddMemoryCache();


// ===============================
// Repositories
// ===============================

builder.Services.AddScoped<ICourseRepository,
    CourseRepository>();

builder.Services.AddScoped<IStudentRepository,
    StudentRepository>();

builder.Services.AddScoped<IStudentProfileRepository,
    StudentProfileRepository>();

builder.Services.AddScoped<IInstructorRepository,
    InstructorRepository>();

builder.Services.AddScoped<IEnrollmentRepository,
    EnrollmentRepository>();

builder.Services.AddScoped<IPersonRepository,
    PersonRepository>();

builder.Services.AddScoped<IAdminRepository,
    AdminRepository>();


// ===============================
// Services
// ===============================

builder.Services.AddScoped<ICourseService,
    CourseService>();

builder.Services.AddScoped<IStudentService,
    StudentService>();

builder.Services.AddScoped<IStudentProfileService,
    StudentProfileService>();

builder.Services.AddScoped<IInstructorService,
    InstructorService>();

builder.Services.AddScoped<IEnrollmentService,
    EnrollmentService>();

builder.Services.AddScoped<IAdminService,
    AdminService>();

builder.Services.AddScoped<IRefreshTokenService,
    RefreshTokenService>();

builder.Services.AddScoped<IAuditService,
    AuditService>();

// ===============================
// CORS
// ===============================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


// ===============================
// Rate Limiting
// ===============================

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(
        policyName: "AuthPolicy",
        limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;

            limiterOptions.Window =
                TimeSpan.FromMinutes(1);

            limiterOptions.QueueLimit = 0;

            limiterOptions.QueueProcessingOrder =
                QueueProcessingOrder.OldestFirst;
        });


    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;
});


// ===============================
// Build Application
// ===============================

var app = builder.Build();


// ===============================
// Database Initialization
// ===============================

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
        .GetRequiredService<TrainingCenterDbContext>();


    var passwordHasher =
        scope.ServiceProvider
        .GetRequiredService<IPasswordHasher>();


    await DbInitializer.Initialize(
        context,
        passwordHasher);
}


// ===============================
// Swagger
// ===============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


// ===============================
// Middleware Pipeline
// ===============================

app.UseHttpsRedirection();


app.UseRateLimiter();


app.UseCors("AllowFrontend");


// Global Exception Handler
app.UseMiddleware<
    TrainingCenterAPI.Utilities.Middleware.ExceptionHandlingMiddleware>();


// Adds x-correlation-id header
app.UseMiddleware<
    TrainingCenterAPI.Utilities.Middleware.CorrelationIdMiddleware>();


// Adds security headers
app.UseMiddleware<
    TrainingCenterAPI.Utilities.Middleware.SecurityHeadersMiddleware>();


app.UseAuthentication();

app.UseOutputCache();

app.UseAuthorization();



app.MapControllers();


// Health endpoint
app.MapHealthChecks("/health");


app.Run();