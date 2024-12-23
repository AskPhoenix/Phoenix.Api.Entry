﻿namespace Phoenix.Api.Entry.Controllers
{
    [ApiExplorerSettings(GroupName = "3a")]
    public class ClassroomController : DataEntryController<Classroom, ClassroomApi>
    {
        private readonly ClassroomRepository _classroomRepository;

        public ClassroomController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<ClassroomController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _classroomRepository = new(phoenixContext, nonObviatedOnly: false);
        }

        protected override bool Check(Classroom classroom)
        {
            if (classroom is null)
                return false;

            if (FindSchool(classroom.SchoolId) is null)
                return false;

            return true;
        }

        #region POST

        public override async Task<ClassroomApi?> PostAsync([FromBody] ClassroomApi classroomApi)
        {
            _logger.LogInformation("Entry -> Classroom -> Post");

            var classroom = classroomApi.ToClassroom();

            if (!Check(classroom))
                return null;

            if ((await _classroomRepository.FindUniqueAsync(classroomApi.SchoolId, classroomApi)) is not null)
                return null;

            classroom = await _classroomRepository.CreateAsync(classroom);

            return new ClassroomApi(classroom);
        }

        #endregion

        #region GET

        public override IEnumerable<ClassroomApi>? Get()
        {
            _logger.LogInformation("Entry -> Classroom -> Get");

            return FindClassrooms()?
                .Select(c => new ClassroomApi(c));
        }

        public override ClassroomApi? Get(int id)
        {
            _logger.LogInformation("Entry -> Classroom -> Get -> {id}", id);

            var classroom = FindClassroom(id);
            if (classroom is null)
                return null;

            return new ClassroomApi(classroom);
        }

        [HttpGet("{id}/lectures")]
        public IEnumerable<LectureApi>? GetLectures(int id)
        {
            _logger.LogInformation("Entry -> Classroom -> Get -> Lectures -> {id}", id);

            var classroom = FindClassroom(id);
            if (classroom is null)
                return null;

            return classroom.Lectures
                .Where(l => !l.ObviatedAt.HasValue)
                .Select(l => new LectureApi(l));
        }

        [HttpGet("{id}/schedules")]
        public IEnumerable<ScheduleApi>? GetSchedules(int id)
        {
            _logger.LogInformation("Entry -> Classroom -> Get -> Schedules -> {id}", id);

            var classroom = FindClassroom(id);
            if (classroom is null)
                return null;

            return classroom.Schedules
                .Where(l => !l.ObviatedAt.HasValue)
                .Select(s => new ScheduleApi(s));
        }

        #endregion

        #region PUT

        public override async Task<ClassroomApi?> PutAsync(int id, [FromBody] ClassroomApi classroomApi)
        {
            _logger.LogInformation("Entry -> Classroom -> Put -> {id}", id);

            var classroom = FindClassroom(id);
            if (classroom is null)
                return null;

            classroom = classroomApi.ToClassroom(classroom);

            if (!Check(classroom))
                return null;

            classroom = await _classroomRepository.UpdateAsync(classroom);

            return new ClassroomApi(classroom);
        }

        #endregion

        #region DELETE

        public override async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> Classroom -> Delete -> {id}", id);

            if (!CheckUserAuth())
                return Unauthorized();

            var classroom = FindClassroom(id);
            if (classroom is null)
                return BadRequest();

            await _classroomRepository.ObviateAsync(classroom);

            return Ok();
        }

        #endregion
    }
}
