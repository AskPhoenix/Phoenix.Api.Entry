namespace Phoenix.Api.Entry.Controllers
{
    public class ClassroomController : EntryController<Classroom, ClassroomApi>
    {
        private readonly ClassroomRepository _classroomRepository;

        public ClassroomController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<ClassroomController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _classroomRepository = new(phoenixContext, nonObviatedOnly: true);
        }

        protected override bool Check(Classroom classroom)
        {
            if (classroom is null)
                return false;

            if (this.FindSchool(classroom.SchoolId) is null)
                return false;

            return true;
        }

        [HttpPost]
        public override async Task<ClassroomApi?> PostAsync([FromBody] ClassroomApi classroomApi)
        {
            _logger.LogInformation("Entry -> Classroom -> Post");

            if (!this.CheckUserAuth())
                return null;

            var classroom = classroomApi.ToClassroom();
            classroom.Id = 0;

            if (!Check(classroom))
                return null;

            classroom = await _classroomRepository.CreateAsync(classroom);

            return new ClassroomApi(classroom);
        }

        [HttpGet]
        public override IEnumerable<ClassroomApi>? Get()
        {
            _logger.LogInformation("Entry -> Classroom -> Get");

            return this.PhoenixUser?
                .Schools
                .SelectMany(s => s.Classrooms)
                .Select(c => new ClassroomApi(c));
        }

        [HttpGet("{id}")]
        public override ClassroomApi? Get(int id)
        {
            _logger.LogInformation("Entry -> Classroom -> Get -> {id}", id);

            var classroom = this.FindClassroom(id);
            if (classroom is null)
                return null;

            return new ClassroomApi(classroom);
        }

        [HttpGet("{id}/lectures")]
        public IEnumerable<LectureApi>? GetLectures(int id)
        {
            _logger.LogInformation("Entry -> Classroom -> Get -> Lectures -> {id}", id);

            var classroom = this.FindClassroom(id);
            if (classroom is null)
                return null;

            return classroom.Lectures
                .Select(l => new LectureApi(l));
        }

        [HttpGet("{id}/schedules")]
        public IEnumerable<ScheduleApi>? GetSchedules(int id)
        {
            _logger.LogInformation("Entry -> Classroom -> Get -> Schedules -> {id}", id);

            var classroom = this.FindClassroom(id);
            if (classroom is null)
                return null;

            return classroom.Schedules
                .Select(s => new ScheduleApi(s));
        }

        [HttpPut("{id}")]
        public override async Task<ClassroomApi?> PutAsync(int id, [FromBody] ClassroomApi classroomApi)
        {
            _logger.LogInformation("Entry -> Classroom -> Put -> {id}", id);

            var classroom = this.FindClassroom(id);
            if (classroom is null)
                return null;

            classroom = classroomApi.ToClassroom(classroom);

            if (!Check(classroom))
                return null;

            classroom = await _classroomRepository.UpdateAsync(classroom);

            return new ClassroomApi(classroom);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> Classroom -> Delete -> {id}", id);

            if (!this.CheckUserAuth())
                return Unauthorized();

            var classroom = this.FindClassroom(id);
            if (classroom is null)
                return BadRequest();

            await _classroomRepository.DeleteAsync(classroom);

            return Ok();
        }
    }
}
