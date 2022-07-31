using Phoenix.DataHandle.Main.Types;

namespace Phoenix.Api.Entry.Controllers
{
    public class LectureController : EntryController<Lecture, LectureApi>
    {
        private readonly LectureRepository _lectureRepository;

        public LectureController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<LectureController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _lectureRepository = new(phoenixContext);
        }

        protected override bool Check(Lecture lecture)
        {
            if (lecture is null)
                return false;

            if (this.FindCourse(lecture.CourseId) is null)
                return false;

            if (lecture.ClassroomId.HasValue && this.FindClassroom(lecture.ClassroomId.Value) is null)
                return false;

            if (lecture.ScheduleId.HasValue && this.FindSchedule(lecture.ScheduleId.Value) is null)
                return false;

            if (lecture.EndDateTime.TimeOfDay <= lecture.StartDateTime.TimeOfDay)
                return false;

            return true;
        }

        [HttpPost("exceptional")]
        public override async Task<LectureApi?> PostAsync([FromBody] LectureApi lectureApi)
        {
            _logger.LogInformation("Entry -> Lecture -> Post");

            var lecture = lectureApi.ToLecture();
            lecture.Id = 0;
            lecture.Occasion = LectureOccasion.Exceptional;

            if (!Check(lecture))
                return null;

            lecture = await _lectureRepository.CreateAsync(lecture);

            return new LectureApi(lecture);
        }

        [HttpPost("{id}/replacement")]
        public async Task<LectureApi?> PostReplacementAsync(int id, [FromBody] LectureApi lectureApi)
        {
            _logger.LogInformation("Entry -> Lecture -> Post");

            var lectureToBeReplaced = this.FindLecture(id);
            if (lectureToBeReplaced is null)
                return null;

            var lectureReplacement = lectureApi.ToLecture();
            lectureReplacement.Id = 0;
            lectureReplacement.Occasion = LectureOccasion.Replacement;

            if (!Check(lectureReplacement))
                return null;

            lectureReplacement = await _lectureRepository.CreateAsync(lectureReplacement);

            lectureToBeReplaced.ReplacementLectureId = lectureReplacement.Id;
            lectureToBeReplaced = await _lectureRepository.UpdateAsync(lectureToBeReplaced);

            return new LectureApi(lectureReplacement);
        }

        [HttpGet]
        public override IEnumerable<LectureApi>? Get()
        {
            _logger.LogInformation("Entry -> Lecture -> Get");

            return this.PhoenixUser?
                .Schools
                .SelectMany(s => s.Courses)
                .SelectMany(c => c.Lectures)
                .Select(l => new LectureApi(l));
        }

        [HttpGet("{id}")]
        public override LectureApi? Get(int id)
        {
            _logger.LogInformation("Entry -> Lecture -> Get -> {id}", id);

            var lecture = this.FindLecture(id);
            if (lecture is null)
                return null;

            return new LectureApi(lecture);
        }

        [HttpPut("{id}")]
        public override async Task<LectureApi?> PutAsync(int id, [FromBody] LectureApi lectureApi)
        {
            _logger.LogInformation("Entry -> Lecture -> Put -> {id}", id);

            var lecture = this.FindLecture(id);
            if (lecture is null)
                return null;

            lecture = lectureApi.ToLecture(lecture);

            if (!Check(lecture))
                return null;

            lecture = await _lectureRepository.UpdateAsync(lecture);

            return new LectureApi(lecture);
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> Lecture -> Delete -> {id}", id);

            if (!this.CheckUserAuth())
                return Unauthorized();

            var lecture = this.FindLecture(id);
            if (lecture is null)
                return BadRequest();

            await _lectureRepository.DeleteAsync(lecture);

            return Ok();
        }
    }
}
