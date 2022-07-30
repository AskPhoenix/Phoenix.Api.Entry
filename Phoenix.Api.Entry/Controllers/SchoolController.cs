using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Entry.Controllers
{
    public class SchoolController : EntryController
    {
        private readonly SchoolRepository _schoolRepository;
        private readonly ApplicationStore _appStore;

        public SchoolController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<SchoolController> logger,
            ApplicationStore appStore)
            : base(phoenixContext, userManager, logger)
        {
            _schoolRepository = new(phoenixContext, nonObviatedOnly: true);
            _appStore = appStore;
        }

        [HttpPost]
        public async Task<SchoolApi?> PostAsync([FromBody] SchoolApi schoolApi)
        {
            _logger.LogInformation("Entry -> School -> Post");

            if (!this.CheckUserAuth())
                return null;

            var school = schoolApi.ToSchool();
            school.Id = 0;
            school.Code = 0;
            school.SchoolSetting.SchoolId = 0;
            school.Users.Add(this.PhoenixUser!);

            school = await _schoolRepository.CreateAsync(school);

            school.Code = -school.Id;
            school = await _schoolRepository.UpdateAsync(school);

            return new SchoolApi(school);
        }

        [HttpGet]
        public IEnumerable<SchoolApi>? Get()
        {
            _logger.LogInformation("Entry -> School -> Get");

            return this.PhoenixUser?
                .Schools
                .Select(s => new SchoolApi(s));
        }

        [HttpGet("{id}")]
        public SchoolApi? Get(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> {id}", id);

            var school = this.FindSchool(id);
            if (school is null)
                return null;

            return new SchoolApi(school);
        }

        [HttpGet("connections/{id}")]
        public IEnumerable<SchoolConnectionApi>? GetConnections(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> Connections -> {id}", id);

            var school = this.FindSchool(id);
            if (school is null)
                return null;

            return school.SchoolConnections
                .Select(c => new SchoolConnectionApi(c));
        }

        [HttpGet("courses/{id}")]
        public IEnumerable<CourseApi>? GetCourses(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> Courses -> {id}", id);

            var school = this.FindSchool(id);
            if (school is null)
                return null;

            return school.Courses
                .Where(c => c.ObviatedAt == null)
                .Select(c => new CourseApi(c));
        }

        [HttpGet("classrooms/{id}")]
        public IEnumerable<ClassroomApi>? GetClassrooms(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> Classrooms -> {id}", id);

            var school = this.FindSchool(id);
            if (school is null)
                return null;

            return school.Classrooms
                .Where(c => c.ObviatedAt == null)
                .Select(c => new ClassroomApi(c));
        }

        [HttpGet("users/{id}")]
        public async Task<IEnumerable<AspNetUserApi>?> GetUsersAsync(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> Users -> {id}", id);

            var school = this.FindSchool(id);
            if (school is null)
                return null;

            var users = school.Users.Where(u => u.ObviatedAt == null);
            
            // TODO: Generalize with a method that takes users as argument
            var tore = new List<AspNetUserApi>(users.Count());
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.AspNetUserId.ToString());
                var roles = await _userManager.GetRolesAsync(appUser);

                tore.Add(new(user, appUser, roles.ToList()));
            }

            return tore;
        }

        [HttpPut("{id}")]
        public async Task<SchoolApi?> PutAsync(int id, [FromBody] SchoolApi schoolApi)
        {
            _logger.LogInformation("Entry -> School -> Put -> {id}", id);

            var school = this.FindSchool(id);
            if (school is null)
                return null;

            school = await _schoolRepository.UpdateAsync(schoolApi.ToSchool(school));

            return new SchoolApi(school);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> School -> Delete -> {id}", id);

            if (!this.CheckUserAuth())
                return Unauthorized();

            var school = this.FindSchool(id);
            if (school is null)
                return BadRequest();

            await _schoolRepository.DeleteAsync(school);

            return Ok();
        }
    }
}
