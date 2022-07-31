using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models;
using Phoenix.DataHandle.DataEntry;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

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
            _lectureRepository = new(phoenixContext);
        }

        [HttpPost]
        public async Task<ScheduleApi?> PostAsync([FromBody] ScheduleApi scheduleApi)
        {
            _logger.LogInformation("Entry -> Schedule -> Post");

            if (!this.CheckUserAuth())
                return null;

            if (scheduleApi.EndTime < scheduleApi.StartTime)
                return null;

            var course = this.FindCourse(scheduleApi.CourseId);
            if (course is null)
                return null;

            var schedule = scheduleApi.ToSchedule();
            schedule.Id = 0;

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

            schedule = await _scheduleRepository.UpdateAsync(scheduleApi.ToSchedule(schedule));

            return new ScheduleApi(schedule);
        }

        [HttpPut("lectures/{id}")]
        public async Task<IEnumerable<LectureApi>?> PutLecturesAsync(int id)
        {
            _logger.LogInformation("Entry -> Schedule -> Put -> Lectures -> {id}", id);

            var schedule = this.FindSchedule(id);
            if (schedule is null)
                return null;

            // TODO: Check if this deletes the old Lectures, or just sets their ScheduleId property to null
            schedule.Lectures.Clear();

            var lecturesTuple = await EntryHelper.GenerateLecturesAsync(schedule, _lectureRepository);

            var lecturesCreated = await _lectureRepository.CreateRangeAsync(lecturesTuple.Item1);
            var lecturesUpdated = await _lectureRepository.UpdateRangeAsync(lecturesTuple.Item2);

            return lecturesCreated.Concat(lecturesUpdated).Select(l => new LectureApi(l));
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
