using Microsoft.AspNetCore.Authorization;
using Phoenix.DataHandle.Api;
using Phoenix.DataHandle.Api.Models.Extensions;
using Phoenix.DataHandle.Main.Models.Extensions;

namespace Phoenix.Api.Entry.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class EntryController<TModel, TModelApi> : ApplicationController
        where TModel : class, IModelEntity
        where TModelApi : class, IModelApi
    {
        protected EntryController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<EntryController<TModel, TModelApi>> logger)
            : base(phoenixContext, userManager, logger)
        {
        }

        [HttpPost]
        public abstract Task<TModelApi?> PostAsync([FromBody] TModelApi modelApi);

        [HttpGet]
        public abstract IEnumerable<TModelApi>? Get();

        [HttpGet("{id}")]
        public abstract TModelApi? Get(int id);

        [HttpPut("{id}")]
        public abstract Task<TModelApi?> PutAsync(int id, [FromBody] TModelApi modelApi);

        [HttpDelete("{id}")]
        public abstract Task<IActionResult> DeleteAsync(int id);


        protected virtual bool Check(TModel model)
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
