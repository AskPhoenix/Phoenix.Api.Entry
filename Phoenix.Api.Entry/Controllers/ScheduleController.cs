using Phoenix.DataHandle.DataEntry;

namespace Phoenix.Api.Entry.Controllers
{
    [ApiExplorerSettings(GroupName = "3b")]
    public class ScheduleController : DataEntryController<Schedule, ScheduleApi>
    {
        private readonly ScheduleRepository _scheduleRepository;
        private readonly LectureRepository _lectureRepository;

        public ScheduleController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<ScheduleController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _scheduleRepository = new(phoenixContext, nonObviatedOnly: true);
            _lectureRepository = new(phoenixContext, nonObviatedOnly: true);
        }

        protected override bool Check(Schedule schedule)
        {
            if (schedule is null)
                return false;

            if (schedule.EndTime < schedule.StartTime)
                return false;

            if (FindCourse(schedule.CourseId) is null)
                return false;

            if (schedule.ClassroomId.HasValue)
                if (FindClassroom(schedule.ClassroomId.Value) is null)
                    schedule.ClassroomId = null;

            return true;
        }

        #region POST

        public override async Task<ScheduleApi?> PostAsync([FromBody] ScheduleApi scheduleApi)
        {
            _logger.LogInformation("Entry -> Schedule -> Post");

            var schedule = scheduleApi.ToSchedule();

            if (!Check(schedule))
                return null;

            if ((await _scheduleRepository.FindUniqueAsync(scheduleApi.CourseId, scheduleApi)) is not null)
                return null;

            schedule = await _scheduleRepository.CreateAsync(schedule);

            return new ScheduleApi(schedule);
        }

        #endregion

        #region GET

        public override IEnumerable<ScheduleApi>? Get()
        {
            _logger.LogInformation("Entry -> Schedule -> Get");

            return PhoenixUser?
                .Schools
                .SelectMany(s => s.Courses)
                .SelectMany(c => c.Schedules)
                .Select(s => new ScheduleApi(s));
        }

        public override ScheduleApi? Get(int id)
        {
            _logger.LogInformation("Entry -> Schedule -> Get -> {id}", id);

            var schedule = FindSchedule(id);
            if (schedule is null)
                return null;

            return new ScheduleApi(schedule);
        }

        [HttpGet("{id}/lectures")]
        public IEnumerable<LectureApi>? GetLectures(int id)
        {
            _logger.LogInformation("Entry -> Schedule -> Get -> Lectures -> {id}", id);

            var schedule = FindSchedule(id);
            if (schedule is null)
                return null;

            return schedule.Lectures
                .Select(l => new LectureApi(l));
        }

        #endregion

        #region PUT

        public override async Task<ScheduleApi?> PutAsync(int id, [FromBody] ScheduleApi scheduleApi)
        {
            _logger.LogInformation("Entry -> Schedule -> Put -> {id}", id);

            var schedule = FindSchedule(id);
            if (schedule is null)
                return null;

            schedule = scheduleApi.ToSchedule(schedule);

            if (!Check(schedule))
                return null;

            schedule = await _scheduleRepository.UpdateAsync(schedule);

            return new ScheduleApi(schedule);
        }

        [HttpPut("{id}/lectures")]
        public async Task<IEnumerable<LectureApi>?> PutLecturesAsync(int id)
        {
            _logger.LogInformation("Entry -> Schedule -> Put -> Lectures -> {id}", id);

            var schedule = FindSchedule(id);
            if (schedule is null)
                return null;

            var lecturesTuple = await EntryHelper.GenerateLecturesAsync(schedule, _lectureRepository);

            var lecturesCreated = await _lectureRepository.CreateRangeAsync(lecturesTuple.Item1);
            var lecturesUpdated = await _lectureRepository.UpdateRangeAsync(lecturesTuple.Item2);

            var lecturesFinal = lecturesCreated.Concat(lecturesUpdated);

            // TODO: Check if this translates to SQL
            var lecturesToDelete = schedule.Lectures.Where(l => !lecturesFinal.Contains(l));
            await _lectureRepository.DeleteRangeAsync(lecturesToDelete);

            return lecturesFinal.Select(l => new LectureApi(l));
        }

        #endregion

        #region DELETE

        public override async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> Schedule -> Delete -> {id}", id);

            if (!CheckUserAuth())
                return Unauthorized();

            var schedule = FindSchedule(id);
            if (schedule is null)
                return BadRequest();

            await _scheduleRepository.DeleteAsync(schedule);

            return Ok();
        }

        #endregion
    }
}
