using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Entry.Controllers
{
    public class BookController : EntryController
    {
        private readonly BookRepository _bookRepository;
        public BookController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<BookController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _bookRepository = new(phoenixContext);
        }

        [HttpPost]
        public async Task<BookApi?> PostAsync([FromBody] BookApi bookApi)
        {
            _logger.LogInformation("Entry -> Book -> Post");

            if (!this.CheckUserAuth())
                return null;

            var book = bookApi.ToBook();
            book.Id = 0;

            book = await _bookRepository.CreateAsync(book);

            return new BookApi(book);
        }

        [HttpGet]
        public IEnumerable<BookApi>? Get()
        {
            _logger.LogInformation("Entry -> Book -> Get");

            return this.PhoenixUser?
                .Schools
                .SelectMany(s => s.Courses)
                .SelectMany(c => c.Books)
                .Select(b => new BookApi(b));
        }

        [HttpGet("{id}")]
        public BookApi? Get(int id)
        {
            _logger.LogInformation("Entry -> Book -> Get -> {id}", id);

            var book = this.FindBook(id);
            if (book is null)
                return null;

            return new BookApi(book);
        }

        [HttpGet("courses/{id}")]
        public IEnumerable<CourseApi>? GetCourses(int id)
        {
            _logger.LogInformation("Entry -> Book -> Get -> Courses -> {id}", id);

            var book = this.FindBook(id);
            if (book is null)
                return null;

            return book.Courses
                .Select(c => new CourseApi(c));
        }

        [HttpPut("{id}")]
        public async Task<BookApi?> PutAsync(int id, [FromBody] BookApi bookApi)
        {
            _logger.LogInformation("Entry -> Book -> Put -> {id}", id);

            var book = this.FindBook(id);
            if (book is null)
                return null;

            book = await _bookRepository.UpdateAsync(bookApi.ToBook(book));

            return new BookApi(book);
        }

        [HttpPut("courses/{id}")]
        public async Task<IEnumerable<CourseApi>?> PutCoursesAsync(int id, [FromBody] List<int> courseIds)
        {
            _logger.LogInformation("Entry -> Book -> Put -> Courses -> {id}", id);

            var book = this.FindBook(id);
            if (book is null)
                return null;

            book.Courses.Clear();

            Course? course;
            foreach (var courseId in courseIds)
            {
                course = this.FindCourse(courseId);
                if (course is null)
                    continue;

                book.Courses.Add(course);
            }

            book = await _bookRepository.UpdateAsync(book);

            return book.Courses.Select(c => new CourseApi(c));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> Book -> Delete -> {id}", id);

            if (!this.CheckUserAuth())
                return Unauthorized();

            var book = this.FindBook(id);
            if (book is null)
                return BadRequest();

            await _bookRepository.DeleteAsync(book);

            return Ok();
        }
    }
}
