using Phoenix.DataHandle.DataEntry;
using Phoenix.DataHandle.Main.Models.Extensions;

namespace Phoenix.Api.Entry.Controllers
{
    public class ScheduleController : EntryController
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

        protected override bool Check(IModelEntity model)
        {
            var schedule = model as Schedule;

            if (schedule is null)
                return false;

            if (schedule.EndTime < schedule.StartTime)
                return false;

            if (this.FindCourse(schedule.CourseId) is null)
                return false;

            if (schedule.ClassroomId.HasValue)
                if (this.FindClassroom(schedule.ClassroomId.Value) is null)
                    schedule.ClassroomId = null;

            return true;
        }

        [HttpPost]
        public async Task<ScheduleApi?> PostAsync([FromBody] ScheduleApi scheduleApi)
        {
            _logger.LogInformation("Entry -> Schedule -> Post");

            if (!this.CheckUserAuth())
                return null;

            var schedule = scheduleApi.ToSchedule();
            schedule.Id = 0;

            if (!Check(schedule))
                return null;

            schedule = await _scheduleRepository.CreateAsync(schedule);

            return new ScheduleApi(schedule);
        }

        [HttpGet]
        public IEnumerable<ScheduleApi>? Get()
        {
            _logger.LogInformation("Entry -> Schedule -> Get");

            return this.PhoenixUser?
                .Schools
                .SelectMany(s => s.Courses)
                .SelectMany(c => c.Schedules)
                .Select(s => new ScheduleApi(s));
        }

        [HttpGet("{id}")]
        public ScheduleApi? Get(int id)
        {
            _logger.LogInformation("Entry -> Schedule -> Get -> {id}", id);

            var schedule = this.FindSchedule(id);
            if (schedule is null)
                return null;

            return new ScheduleApi(schedule);
        }

        [HttpGet("lectures/{id}")]
        public IEnumerable<LectureApi>? GetLectures(int id)
        {
            _logger.LogInformation("Entry -> Schedule -> Get -> Lectures -> {id}", id);

            var schedule = this.FindSchedule(id);
            if (schedule is null)
                return null;

            return schedule.Lectures
                .Select(l => new LectureApi(l));
        }

        [HttpPut("{id}")]
        public async Task<ScheduleApi?> PutAsync(int id, [FromBody] ScheduleApi scheduleApi)
        {
            _logger.LogInformation("Entry -> Schedule -> Put -> {id}", id);

            var schedule = this.FindSchedule(id);
            if (schedule is null)
                return null;

            schedule = scheduleApi.ToSchedule(schedule);
           
            if (!Check(schedule))
                return null;

            schedule = await _scheduleRepository.UpdateAsync(schedule);

            return new ScheduleApi(schedule);
        }

        [HttpPut("lectures/{id}")]
        public async Task<IEnumerable<LectureApi>?> PutLecturesAsync(int id)
        {
            _logger.LogInformation("Entry -> Schedule -> Put -> Lectures -> {id}", id);

            var schedule = this.FindSchedule(id);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> Schedule -> Delete -> {id}", id);

            if (!this.CheckUserAuth())
                return Unauthorized();

            var schedule = this.FindSchedule(id);
            if (schedule is null)
                return BadRequest();

            await _scheduleRepository.DeleteAsync(schedule);

            return Ok();
        }
    }
}
