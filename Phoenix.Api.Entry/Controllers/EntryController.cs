using Phoenix.DataHandle.Api;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;

namespace Phoenix.Api.Entry.Controllers
{
    public abstract class EntryController : ApplicationController
    {
        protected EntryController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<EntryController> logger)
            : base(phoenixContext, userManager, logger)
        {
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
    }
}
