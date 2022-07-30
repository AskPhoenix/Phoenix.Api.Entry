﻿using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api.Models;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Repositories;

namespace Phoenix.Api.Entry.Controllers
{
    public class CourseController : EntryController
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

        [HttpPost]
        public async Task<CourseApi?> PostAsync([FromBody] CourseApi courseApi)
        {
            _logger.LogInformation("Entry -> Course -> Post");

            if (!this.CheckUserAuth())
                return null;

            if (courseApi.LastDate < courseApi.FirstDate)
                return null;

            var school = this.FindSchool(courseApi.SchoolId);
            if (school is null)
                return null;

            var course = courseApi.ToCourse();
            course.Id = 0;
            course.Code = 0;
            //course.Users.Add(this.PhoenixUser!);

            course = await _courseRepository.CreateAsync(course);

            course.Code = (short)school.Courses.Count;
            course = await _courseRepository.UpdateAsync(course);

            return new CourseApi(course);
        }

        [HttpGet]
        public IEnumerable<CourseApi>? Get()
        {
            _logger.LogInformation("Entry -> Course -> Get");

            return this.PhoenixUser?
                .Schools
                .SelectMany(s => s.Courses)
                .Select(c => new CourseApi(c));
        }

        [HttpGet("{id}")]
        public CourseApi? Get(int id)
        {
            _logger.LogInformation("Entry -> Course -> Get -> {id}", id);

            var course = this.FindCourse(id);
            if (course is null)
                return null;

            return new CourseApi(course);
        }

        [HttpGet("books/{id}")]
        public IEnumerable<BookApi>? GetBooks(int id)
        {
            _logger.LogInformation("Entry -> Course -> Get -> Books -> {id}", id);

            var course = this.FindCourse(id);
            if (course is null)
                return null;

            return course.Books
                .Select(b => new BookApi(b));
        }

        [HttpGet("lectures/{id}")]
        public IEnumerable<LectureApi>? GetLectures(int id)
        {
            _logger.LogInformation("Entry -> Course -> Get -> Lectures -> {id}", id);

            var course = this.FindCourse(id);
            if (course is null)
                return null;

            return course.Lectures
                .Select(l => new LectureApi(l));
        }

        [HttpGet("schedules/{id}")]
        public IEnumerable<ScheduleApi>? GetSchedules(int id)
        {
            _logger.LogInformation("Entry -> Course -> Get -> Schedules -> {id}", id);

            var course = this.FindCourse(id);
            if (course is null)
                return null;

            return course.Schedules
                .Select(s => new ScheduleApi(s));
        }

        [HttpGet("users/{id}")]
        public async Task<IEnumerable<AspNetUserApi>?> GetUsersAsync(int id)
        {
            _logger.LogInformation("Entry -> Course -> Get -> Users -> {id}", id);

            var course = this.FindCourse(id);
            if (course is null)
                return null;

            var users = course.Users.Where(u => u.ObviatedAt == null);
            var tore = new List<AspNetUserApi>(users.Count());

            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.AspNetUserId.ToString());
                var roles = await _userManager.GetRolesAsync(appUser);

                tore.Add(new(user, appUser, roles.ToList()));
            }

            return tore;
        }

        [HttpPut("{id}")]
        public async Task<CourseApi?> PutAsync(int id, [FromBody] CourseApi courseApi)
        {
            _logger.LogInformation("Entry -> Course -> Put -> {id}", id);

            var course = this.FindCourse(id);
            if (course is null)
                return null;

            course = await _courseRepository.UpdateAsync(courseApi.ToCourse(course));

            return new CourseApi(course);
        }

        [HttpPut("books/{id}")]
        public async Task<IEnumerable<BookApi>?> PutBooksAsync(int id, [FromBody] List<int> bookIds)
        {
            _logger.LogInformation("Entry -> Course -> Put -> Books -> {id}", id);

            var course = this.FindCourse(id);
            if (course is null)
                return null;

            course.Books.Clear();

            Book? book;
            foreach (var bookId in bookIds)
            {
                book = this.FindBook(bookId);
                if (book is null)
                    continue;

                course.Books.Add(book);
            }

            course = await _courseRepository.UpdateAsync(course);

            return course.Books.Select(b => new BookApi(b));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> Course -> Delete -> {id}", id);

            if (!this.CheckUserAuth())
                return Unauthorized();

            var course = this.FindCourse(id);
            if (course is null)
                return BadRequest();

            await _courseRepository.DeleteAsync(course);

            return Ok();
        }
    }
}