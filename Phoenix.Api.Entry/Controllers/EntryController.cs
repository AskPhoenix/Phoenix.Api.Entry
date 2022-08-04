using Microsoft.AspNetCore.Authorization;
using Phoenix.DataHandle.Api;

namespace Phoenix.Api.Entry.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class EntryController : ApplicationController
    {
        public EntryController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<EntryController> logger)
            : base(phoenixContext, userManager, logger)
        {
        }

        protected async Task<IEnumerable<ApplicationUserApi>> GetApplicationUsersApiAsync(IEnumerable<User> users)
        {
            if (users is null)
                throw new ArgumentNullException(nameof(users));

            var tore = new List<ApplicationUserApi>(users.Count());
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.AspNetUserId.ToString());
                var roleRanks = await _userManager.GetRoleRanksAsync(appUser);

                tore.Add(new(user, appUser, roleRanks.ToList()));
            }

            return tore;
        }

        protected IEnumerable<Course>? FindCourses(bool nonObviatedOnly = true)
        {
            return this.FindSchools(nonObviatedOnly)?
                .SelectMany(s => s.Courses)
                .Where(c => !nonObviatedOnly || (!c.ObviatedAt.HasValue && nonObviatedOnly));
        }

        protected Course? FindCourse(int courseId, bool nonObviatedOnly = true)
        {
            return this.FindCourses(nonObviatedOnly)?
                .SingleOrDefault(c => c.Id == courseId);
        }

        protected IEnumerable<User>? FindUsers(bool nonObviatedOnly = true)
        {
            return this.FindSchools(nonObviatedOnly)?
                .SelectMany(s => s.Users)
                .Where(u => !nonObviatedOnly || (!u.ObviatedAt.HasValue && nonObviatedOnly));
        }

        protected User? FindUser(int userId, bool nonObviatedOnly = true)
        {
            return this.FindUsers(nonObviatedOnly)?
                .SingleOrDefault(u => u.AspNetUserId == userId);
        }
    }
}
