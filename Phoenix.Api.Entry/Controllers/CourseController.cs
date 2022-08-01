namespace Phoenix.Api.Entry.Controllers
{
    [ApiExplorerSettings(GroupName = "2b")]
    public class CourseController : DataEntryController<Course, CourseApi>
    {
        private readonly CourseRepository _courseRepository;

        public CourseController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<CourseController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _courseRepository = new(phoenixContext, nonObviatedOnly: true);
        }

        protected override bool Check(Course course)
        {
            if (course is null)
                return false;

            if (course.LastDate < course.FirstDate)
                return false;

            if (FindSchool(course.SchoolId) is null)
                return false;

            return true;
        }

        #region POST

        public override async Task<CourseApi?> PostAsync([FromBody] CourseApi courseApi)
        {
            _logger.LogInformation("Entry -> Course -> Post");

            var school = FindSchool(courseApi.SchoolId);

            var course = courseApi.ToCourse();
            course.Id = 0;
            course.Code = 0;
            //course.Users.Add(this.PhoenixUser!);

            if (!Check(course))
                return null;

            course = await _courseRepository.CreateAsync(course);

            course.Code = (short)school!.Courses.Count;
            course = await _courseRepository.UpdateAsync(course);

            return new CourseApi(course);
        }

        #endregion

        #region GET

        public override IEnumerable<CourseApi>? Get()
        {
            _logger.LogInformation("Entry -> Course -> Get");

            return PhoenixUser?
                .Schools
                .SelectMany(s => s.Courses)
                .Select(c => new CourseApi(c));
        }

        public override CourseApi? Get(int id)
        {
            _logger.LogInformation("Entry -> Course -> Get -> {id}", id);

            var course = FindCourse(id);
            if (course is null)
                return null;

            return new CourseApi(course);
        }

        [HttpGet("{id}/books")]
        public IEnumerable<BookApi>? GetBooks(int id)
        {
            _logger.LogInformation("Entry -> Course -> Get -> Books -> {id}", id);

            var course = FindCourse(id);
            if (course is null)
                return null;

            return course.Books
                .Select(b => new BookApi(b));
        }

        [HttpGet("{id}/lectures")]
        public IEnumerable<LectureApi>? GetLectures(int id)
        {
            _logger.LogInformation("Entry -> Course -> Get -> Lectures -> {id}", id);

            var course = FindCourse(id);
            if (course is null)
                return null;

            return course.Lectures
                .Select(l => new LectureApi(l));
        }

        [HttpGet("{id}/schedules")]
        public IEnumerable<ScheduleApi>? GetSchedules(int id)
        {
            _logger.LogInformation("Entry -> Course -> Get -> Schedules -> {id}", id);

            var course = FindCourse(id);
            if (course is null)
                return null;

            return course.Schedules
                .Select(s => new ScheduleApi(s));
        }

        [HttpGet("{id}/users")]
        public async Task<IEnumerable<ApplicationUserApi>?> GetUsersAsync(int id)
        {
            _logger.LogInformation("Entry -> Course -> Get -> Users -> {id}", id);

            var course = FindCourse(id);
            if (course is null)
                return null;

            return await this.GetApplicationUsersAsync(course.Users);
        }

        #endregion

        #region PUT

        public override async Task<CourseApi?> PutAsync(int id, [FromBody] CourseApi courseApi)
        {
            _logger.LogInformation("Entry -> Course -> Put -> {id}", id);

            var course = FindCourse(id);
            if (course is null)
                return null;

            course = courseApi.ToCourse(course);

            if (!Check(course))
                return null;

            course = await _courseRepository.UpdateAsync(course);

            return new CourseApi(course);
        }

        [HttpPut("{id}/books")]
        public async Task<IEnumerable<BookApi>?> PutBooksAsync(int id, [FromBody] List<int> bookIds)
        {
            _logger.LogInformation("Entry -> Course -> Put -> Books -> {id}", id);

            var course = FindCourse(id);
            if (course is null)
                return null;

            course.Books.Clear();

            Book? book;
            foreach (var bookId in bookIds)
            {
                book = FindBook(bookId);
                if (book is null)
                    continue;

                course.Books.Add(book);
            }

            course = await _courseRepository.UpdateAsync(course);

            return course.Books.Select(b => new BookApi(b));
        }

        [HttpPut("{id}/users")]
        public async Task<IEnumerable<ApplicationUserApi>?> PutUsersAsync(int id,
            [FromBody] List<int> userIds)
        {
            _logger.LogInformation("Entry -> Course -> Put -> Users -> {id}", id);

            var course = FindCourse(id);
            if (course is null)
                return null;

            course.Users.Clear();

            User? user;
            foreach (var userId in userIds)
            {
                user = this.FindUser(userId);
                if (user is null)
                    continue;

                course.Users.Add(user);
            }

            course = await _courseRepository.UpdateAsync(course);

            return await this.GetApplicationUsersAsync(course.Users);
        }

        #endregion

        #region DELETE

        public override async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> Course -> Delete -> {id}", id);

            if (!CheckUserAuth())
                return Unauthorized();

            var course = FindCourse(id);
            if (course is null)
                return BadRequest();

            await _courseRepository.DeleteAsync(course);

            return Ok();
        }

        #endregion
    }
}
