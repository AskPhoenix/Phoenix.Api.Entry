﻿using Microsoft.AspNetCore.Authorization;
using Phoenix.DataHandle.Api;
using Phoenix.DataHandle.Main.Models.Extensions;

namespace Phoenix.Api.Entry.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class EntryController : ApplicationController
    {
        protected EntryController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<EntryController> logger)
            : base(phoenixContext, userManager, logger)
        {
        }

        protected virtual bool Check(IModelEntity model)
        {
            return model is not null;
        }

        protected Course? FindCourse(int courseId)
        {
            return this.PhoenixUser?.Schools
                .SelectMany(s => s.Courses)
                .SingleOrDefault(c => c.Id == courseId);
        }

        protected Book? FindBook(int bookId)
        {
            return this.PhoenixUser?.Schools
                .SelectMany(s => s.Courses)
                .SelectMany(c => c.Books)
                .SingleOrDefault(b => b.Id == bookId);
        }

        protected Schedule? FindSchedule(int scheduleId)
        {
            return this.PhoenixUser?.Schools
                .SelectMany(s => s.Courses)
                .SelectMany(c => c.Schedules)
                .SingleOrDefault(s => s.Id == scheduleId);
        }

        protected Classroom? FindClassroom(int classroomId)
        {
            return this.PhoenixUser?.Schools
                .SelectMany(s => s.Classrooms)
                .SingleOrDefault(c => c.Id == classroomId);
        }
    }
}
