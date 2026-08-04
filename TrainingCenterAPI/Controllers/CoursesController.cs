using Microsoft.AspNetCore.Mvc;
using TrainingCenterAPI.Services.Interfaces;
using TrainingCenterAPI.DTOs;
using TrainingCenterAPI.Utilities.Filters;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using TrainingCenterAPI.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;


namespace TrainingCenterAPI.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly IMemoryCache _cache;

        public CoursesController(
            ICourseService courseService,
            IMemoryCache cache)
        {
            _courseService = courseService;
            _cache = cache;
        }

        /// <summary>
        /// GET /api/courses?level={level}&status={status}
        /// Returns all courses, optionally filtered by Level and/or Status.
        /// Both query parameters are optional (nullable) — omit either or
        /// both to get unfiltered/partially filtered results.
        ///
        /// Principle: DRY (Don't Repeat Yourself).
        /// One flexible method handles every filter combination via
        /// AsQueryable() and conditional .Where() clauses in the repository,
        /// instead of writing a separate method for each possible combination
        /// of filters (GetByLevel, GetByStatus, GetByLevelAndStatus, etc.),
        /// which wouldn't scale as more filters are added later.
        ///
        /// Example calls:
        ///   GET /api/courses               → all courses
        ///   GET /api/courses?level=0       → Beginner only
        ///   GET /api/courses?status=1      → Published only
        ///   GET /api/courses?level=0&status=1 → Beginner AND Published
        /// </summary>

        /// <summary>
        /// GET /api/courses?page={page}&pageSize={pageSize}&level=&status=
        /// Returns a page of courses instead of the full result set, plus
        /// TotalCount so the client can calculate total pages.
        ///
        /// Principle: Single Responsibility. Pagination logic (Skip/Take) lives
        /// in the repository, next to the query it paginates — the controller
        /// only passes parameters through and shapes the response.
        /// </summary>
        [HttpGet(Name = "GetAllCourses")]
        [EndpointSummary("Get all courses")]
        [EndpointDescription("Returns all courses, optionally filtered by level and status.")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] CourseLevel? level,
            [FromQuery] CourseStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var cacheKey =
                $"courses_{level}_{status}_{page}_{pageSize}";


            if (_cache.TryGetValue(cacheKey, out object? cachedResult))
            {
                return Ok(cachedResult);
            }


            var (courses, totalCount) =
                await _courseService.GetAllCoursesAsync(
                    level,
                    status,
                    page,
                    pageSize);


            var result = new
            {
                totalCount,
                page,
                pageSize,
                items = courses
            };


            var cacheOptions =
                new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(
                    TimeSpan.FromMinutes(5));


            _cache.Set(
                cacheKey,
                result,
                cacheOptions);


            return Ok(result);
        }
        //
        //
        //

        /// <summary>
        /// Before (what you originally had — Controller checks manually):
        /// csharp
        /// [HttpGet("{id}", Name = "GetCourseById")]
        ///        [ValidateId]
        /// public async Task<ActionResult<CourseDto>> GetById(int id)
        /// {
        ///     var course = await _courseService.GetCourseByIdAsync(id);
        ///     if (course == null)
        ///     {
        ///         return ApiResponseHelper.NotFoundResponse("Course", id);
        ///     }
        ///     return Ok(course);
        /// }
        ///
        /// Here, CourseService.GetCourseByIdAsync just returns null if nothing's found — it doesn't throw anything.
        /// The Controller is the one deciding "null means 404" and building that response itself.
        ///
        /// After(the #15 style — Service throws, Middleware catches):
        /// </summary>

        [HttpGet("{id}", Name = "GetCourseById")]
        [ValidateId]
        [AllowAnonymous]
        public async Task<ActionResult<CourseDto>> GetById(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            return Ok(course);
        }

        //
        //
        //

        /// <summary>
        /// Here, if the course doesn't exist, GetCourseByIdAsync throws NotFoundException immediately 
        /// — execution never even reaches return Ok(course). The exception travels up, ExceptionHandlingMiddleware catches it,
        /// and sends the client a 404 automatically. The Controller has no if check at all 
        /// — it just trusts that if it got a result back, it's valid. 
        /// </summary>

        [HttpPost(Name = "AddCourse")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseDto dto)
        {
            var course = await _courseService.CreateCourseAsync(dto);
            return CreatedAtRoute("GetCourseById", new { id = course.CourseId }, course);
        }

        /// <summary>
        /// PUT = replace everything.
        /// To change just the status,
        /// you'd have to send the entire course object back — title, price, description, all of it 
        /// — even though you only wanted to change one thing.
        /// </summary>

        /// <summary>
        /// If the course exists → UpdateCourseAsync updates it, returns true, controller ignores the return value, returns 204 No Content
        /// If the course doesn't exist → UpdateCourseAsync throws NotFoundException before it even gets to return anything →
        /// the exception bubbles up through the controller (never reaches return NoContent()) → 
        /// ExceptionHandlingMiddleware catches it → client gets 404 automatically
        /// So once CourseService.UpdateCourseAsync is fully migrated to throw instead of return false, 
        /// the bool return type becomes pointless — nothing ever needs to check it, because failure is signaled via exception,
        /// not return value.At that point, you could even simplify the interface to return Task instead of Task<bool>.
        /// </summary>

        //
        //
        //

        [HttpPut("{id}", Name = "UpdateCourse")]
        [ValidateId]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDto dto)
        {

            await _courseService.UpdateCourseAsync(id, dto);
            return NoContent();
        }

        // 
        //
        //

        [HttpDelete("{id}", Name = "DeleteCourse")]
        [ValidateId]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _courseService.DeleteCourseAsync(id);
            return NoContent();
        }

        //
        //
        //

        // Additional methods

        /// <summary>
        /// PATCH .../publish = do one specific action.
        /// You send nothing — just hit the URL — and the server knows exactly what "publish" means:
        /// set status to Published, set the published date to now.
        /// 
        /// ----------------------------------------
        /// 
        /// Client sends: PATCH /api/courses/5/publish
        ///
        /// 1. Controller receives the id(5)
        /// 2. Controller calls _courseService.PublishCourseAsync(5)
        /// 3. Service calls _courseRepository.GetByIdAsync(5) → loads the full Course entity
        /// 4. Service sets course.Status = Published, course.PublishedAt = now
        /// 5. Service calls _courseRepository.Update(course) → marks it for saving
        /// 6. Service calls _courseRepository.SaveChangesAsync() → commits to the database
        /// 7. Controller returns 204 No Content
        /// 
        /// </summary>

        [HttpPatch("{id}/publish", Name = "PublishCourse")]
        [ValidateId]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Publish(int id)
        {
            await _courseService.PublishCourseAsync(id);
            return NoContent();
        }

        //
        //
        //

        /// <summary>
        /// GET /api/courses/instructor/{instructorId}
        /// Returns all courses taught by a specific instructor.
        /// Principle: Single Responsibility — a focused, entity-specific query
        /// that doesn't belong in the generic IRepository&lt;T&gt;, only in
        /// ICourseRepository.
        /// </summary>
        [HttpGet("instructor/{instructorId}", Name = "GetCoursesByInstructor")]
        [ValidateId("instructorId")]
        public async Task<IActionResult> GetByInstructor(int instructorId)
        {
            var courses = await _courseService.GetCoursesByInstructorAsync(instructorId);
            return Ok(courses);
        }
    }
}