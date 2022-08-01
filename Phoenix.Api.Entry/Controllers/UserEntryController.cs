using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Entry.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class UserEntryController<TRoleRank> : EntryController
        where TRoleRank : Enum
    {
        public UserEntryController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<UserEntryController<TRoleRank>> logger)
            : base(phoenixContext, userManager, logger)
        {
        }

        #region POST

        [HttpPost("{role}")]
        public abstract Task<ApplicationUserApi?> PostAsync([FromBody] ApplicationUserApi appUserApi,
            TRoleRank role, [FromQuery, Required] int[] school_ids);

        #endregion

        #region GET

        [HttpGet]
        public async Task<IEnumerable<ApplicationUserApi>?> GetAsync()
        {
            _logger.LogInformation("Entry -> User -> Get");

            var users = this.PhoenixUser?.Schools.SelectMany(s => s.Users);
            if (users is null)
                return null;

            return await this.GetApplicationUsersAsync(users);
        }

        [HttpGet("{role}")]
        public abstract Task<IEnumerable<ApplicationUserApi>?> GetAsync(TRoleRank role);

        [HttpGet("{id}")]
        public async Task<ApplicationUserApi?> GetAsync(int id)
        {
            _logger.LogInformation("Entry -> User -> Get -> {id}", id);

            var user = FindUser(id);
            if (user is null)
                return null;

            var appUser = await _userManager.FindByIdAsync(user.AspNetUserId.ToString());
            var roleRanks = await _userManager.GetRoleRanksAsync(appUser);

            return new ApplicationUserApi(user, appUser, roleRanks.ToList());
        }

        [HttpGet("{id}/schools")]
        public IEnumerable<SchoolApi>? GetSchools(int id)
        {
            _logger.LogInformation("Entry -> User -> Get -> Schools -> {id}", id);

            var user = FindUser(id);
            if (user is null)
                return null;

            return user.Schools
                .Select(s => new SchoolApi(s));
        }

        [HttpGet("{id}/courses")]
        public IEnumerable<CourseApi>? GetCourses(int id)
        {
            _logger.LogInformation("Entry -> User -> Get -> Courses -> {id}", id);

            var user = FindUser(id);
            if (user is null)
                return null;

            return user.Courses
                .Select(c => new CourseApi(c));
        }

        #endregion

        #region PUT

        [HttpPut("{id}")]
        public abstract Task<ApplicationUserApi?> PutAsync(int id, [FromBody] ApplicationUserApi appUserApi);

        [HttpPut("{id}/courses")]
        public async Task<IEnumerable<CourseApi>?> PutCoursesAsync(int id, [FromBody] List<int> courseIds)
        {
            _logger.LogInformation("Entry -> User -> Put -> Courses -> {id}", id);

            var user = FindUser(id);
            if (user is null)
                return null;

            user.Courses.Clear();

            Course? course;
            foreach (var courseId in courseIds)
            {
                course = this.FindCourse(courseId);
                if (course is null)
                    continue;

                user.Courses.Add(course);
            }

            user = await _userRepository.UpdateAsync(user);

            return user.Courses.Select(c => new CourseApi(c));
        }

        #endregion

        #region DELETE

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> User -> Delete -> {id}", id);

            if (!CheckUserAuth())
                return Unauthorized();

            var user = FindUser(id);
            if (user is null)
                return BadRequest();

            await _userRepository.DeleteAsync(user);

            return Ok();
        }

        #endregion

        protected int CalculateDependanceOrder(User parent)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            return parent.Children
                .Select(c => c.DependenceOrder)
                .DefaultIfEmpty(0)
                .Max() + 1;
        }
    }
}
