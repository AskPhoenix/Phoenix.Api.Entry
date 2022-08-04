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

        protected IEnumerable<Book>? FindBooks()
        {
            return this.FindCourses()?
                .SelectMany(c => c.Books);
        }

        protected Book? FindBook(int bookId)
        {
            return this.FindBooks()?
                .SingleOrDefault(b => b.Id == bookId);
        }

        protected IEnumerable<Schedule>? FindSchedules(bool nonObviatedOnly = true)
        {
            return this.FindCourses(nonObviatedOnly)?
                .SelectMany(c => c.Schedules)
                .Where(s => !nonObviatedOnly || (!s.ObviatedAt.HasValue && nonObviatedOnly));
        }

        protected Schedule? FindSchedule(int scheduleId, bool nonObviatedOnly = true)
        {
            return this.FindSchedules(nonObviatedOnly)?
                .SingleOrDefault(s => s.Id == scheduleId);
        }

        protected IEnumerable<Classroom>? FindClassrooms(bool nonObviatedOnly = true)
        {
            return this.FindSchools(nonObviatedOnly)?
                .SelectMany(s => s.Classrooms)
                .Where(c => !nonObviatedOnly || (!c.ObviatedAt.HasValue && nonObviatedOnly));
        }

        protected Classroom? FindClassroom(int classroomId, bool nonObviatedOnly = true)
        {
            return this.FindClassrooms(nonObviatedOnly)?
                .SingleOrDefault(c => c.Id == classroomId);
        }

        protected IEnumerable<Lecture>? FindLectures(bool nonObviatedOnly = true)
        {
            return this.FindCourses(nonObviatedOnly)?
                .SelectMany(c => c.Lectures)
                .Where(c => !nonObviatedOnly || (!c.ObviatedAt.HasValue && nonObviatedOnly));
        }

        protected Lecture? FindLecture(int lectureId, bool nonObviatedOnly = true)
        {
            return this.FindLectures(nonObviatedOnly)?
                .SingleOrDefault(l => l.Id == lectureId);
        }
    }
}
