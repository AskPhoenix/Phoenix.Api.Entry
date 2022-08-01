using Phoenix.DataHandle.Main.Types;

namespace Phoenix.Api.Entry.Controllers
{
    // TODO: Remove from Pavo and use through Egretta
    [ApiExplorerSettings(IgnoreApi = true)]
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

            if (FindCourse(lecture.CourseId) is null)
                return false;

            if (lecture.ClassroomId.HasValue && FindClassroom(lecture.ClassroomId.Value) is null)
                return false;

            if (lecture.ScheduleId.HasValue && FindSchedule(lecture.ScheduleId.Value) is null)
                return false;

            if (lecture.EndDateTime.TimeOfDay <= lecture.StartDateTime.TimeOfDay)
                return false;

            return true;
        }

        #region POST

        public override async Task<LectureApi?> PostAsync([FromBody] LectureApi lectureApi)
        {
            _logger.LogInformation("Entry -> Lecture -> Post");

            var lecture = lectureApi.ToLecture();
            lecture.Id = 0;
            lecture.Occasion = LectureOccasion.Exceptional;
            lecture.ScheduleId = null;

            if (!Check(lecture))
                return null;

            lecture = await _lectureRepository.CreateAsync(lecture);

            return new LectureApi(lecture);
        }

        [HttpPost("{id}/replacement")]
        public async Task<LectureApi?> PostReplacementAsync(int id, [FromBody] LectureApi lectureApi)
        {
            _logger.LogInformation("Entry -> Lecture -> Post");

            var lectureToBeReplaced = FindLecture(id);
            if (lectureToBeReplaced is null)
                return null;

            var lectureReplacement = lectureApi.ToLecture();
            lectureReplacement.Id = 0;
            lectureReplacement.Occasion = LectureOccasion.Replacement;
            lectureReplacement.ScheduleId = null;

            if (!Check(lectureReplacement))
                return null;

            lectureReplacement = await _lectureRepository.CreateAsync(lectureReplacement);

            lectureToBeReplaced.ReplacementLectureId = lectureReplacement.Id;
            lectureToBeReplaced = await _lectureRepository.UpdateAsync(lectureToBeReplaced);

            return new LectureApi(lectureReplacement);
        }

        #endregion

        #region GET

        public override IEnumerable<LectureApi>? Get()
        {
            _logger.LogInformation("Entry -> Lecture -> Get");

            return PhoenixUser?
                .Schools
                .SelectMany(s => s.Courses)
                .SelectMany(c => c.Lectures)
                .Select(l => new LectureApi(l));
        }

        public override LectureApi? Get(int id)
        {
            _logger.LogInformation("Entry -> Lecture -> Get -> {id}", id);

            var lecture = FindLecture(id);
            if (lecture is null)
                return null;

            return new LectureApi(lecture);
        }

        #endregion

        #region PUT

        public override async Task<LectureApi?> PutAsync(int id, [FromBody] LectureApi lectureApi)
        {
            _logger.LogInformation("Entry -> Lecture -> Put -> {id}", id);

            var lecture = FindLecture(id);
            if (lecture is null)
                return null;

            lecture = lectureApi.ToLecture(lecture);

            if (!Check(lecture))
                return null;

            lecture = await _lectureRepository.UpdateAsync(lecture);

            return new LectureApi(lecture);
        }

        #endregion

        #region DELETE

        public override async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> Lecture -> Delete -> {id}", id);

            if (!CheckUserAuth())
                return Unauthorized();

            var lecture = FindLecture(id);
            if (lecture is null)
                return BadRequest();

            await _lectureRepository.DeleteAsync(lecture);

            return Ok();
        }

        #endregion
    }
}
