using Phoenix.DataHandle.Api.Models.Extensions;

namespace Phoenix.Api.Entry.Controllers
{
    public abstract class DataEntryController<TModel, TModelApi> : EntryController
        where TModel : class
        where TModelApi : class, IModelApi
    {
        protected DataEntryController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<DataEntryController<TModel, TModelApi>> logger)
            : base(phoenixContext, userManager, logger)
        {
        }

        // TODO: Check if unique model already exists in POST
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
            return this.CheckUserAuth() && model is not null;
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

        protected Lecture? FindLecture(int lectureId)
        {
            return this.PhoenixUser?.Schools
                .SelectMany(s => s.Courses)
                .SelectMany(c => c.Lectures)
                .SingleOrDefault(l => l.Id == lectureId);
        }
    }
}
