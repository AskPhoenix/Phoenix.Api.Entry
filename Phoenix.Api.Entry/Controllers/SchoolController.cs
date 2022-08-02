namespace Phoenix.Api.Entry.Controllers
{
    [ApiExplorerSettings(GroupName = "1a")]
    public class SchoolController : DataEntryController<School, SchoolApi>
    {
        private readonly SchoolRepository _schoolRepository;

        public SchoolController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<SchoolController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _schoolRepository = new(phoenixContext, nonObviatedOnly: true);
        }

        #region POST

        public override async Task<SchoolApi?> PostAsync([FromBody] SchoolApi schoolApi)
        {
            _logger.LogInformation("Entry -> School -> Post");

            if (!CheckUserAuth())
                return null;

            var school = schoolApi.ToSchool();
            school.Users.Add(PhoenixUser!);
            school = await _schoolRepository.CreateAsync(school);

            school.Code = -school.Id;
            school = await _schoolRepository.UpdateAsync(school);

            return new SchoolApi(school);
        }

        #endregion

        #region GET

        public override IEnumerable<SchoolApi>? Get()
        {
            _logger.LogInformation("Entry -> School -> Get");

            return PhoenixUser?
                .Schools
                .Select(s => new SchoolApi(s));
        }

        public override SchoolApi? Get(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> {id}", id);

            var school = FindSchool(id);
            if (school is null)
                return null;

            return new SchoolApi(school);
        }

        [HttpGet("{id}/connections")]
        public IEnumerable<SchoolConnectionApi>? GetConnections(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> Connections -> {id}", id);

            var school = FindSchool(id);
            if (school is null)
                return null;

            return school.SchoolConnections
                .Select(c => new SchoolConnectionApi(c));
        }

        [HttpGet("{id}/courses")]
        public IEnumerable<CourseApi>? GetCourses(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> Courses -> {id}", id);

            var school = FindSchool(id);
            if (school is null)
                return null;

            return school.Courses
                .Where(c => c.ObviatedAt == null)
                .Select(c => new CourseApi(c));
        }

        [HttpGet("{id}/classrooms")]
        public IEnumerable<ClassroomApi>? GetClassrooms(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> Classrooms -> {id}", id);

            var school = FindSchool(id);
            if (school is null)
                return null;

            return school.Classrooms
                .Where(c => c.ObviatedAt == null)
                .Select(c => new ClassroomApi(c));
        }

        [HttpGet("{id}/users")]
        public async Task<IEnumerable<ApplicationUserApi>?> GetUsersAsync(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> Users -> {id}", id);

            var school = FindSchool(id);
            if (school is null)
                return null;

            return await this.GetApplicationUsersAsync(school.Users);
        }

        #endregion

        #region PUT

        public override async Task<SchoolApi?> PutAsync(int id, [FromBody] SchoolApi schoolApi)
        {
            _logger.LogInformation("Entry -> School -> Put -> {id}", id);

            var school = FindSchool(id);
            if (school is null)
                return null;

            // TODO: Ensure that this update does not empty the collections
            school = await _schoolRepository.UpdateAsync(schoolApi.ToSchool(school));

            return new SchoolApi(school);
        }

        #endregion

        #region DELETE

        public override async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> School -> Delete -> {id}", id);

            if (!CheckUserAuth())
                return Unauthorized();

            var school = FindSchool(id);
            if (school is null)
                return BadRequest();

            await _schoolRepository.DeleteAsync(school);

            return Ok();
        }

        #endregion
    }
}
