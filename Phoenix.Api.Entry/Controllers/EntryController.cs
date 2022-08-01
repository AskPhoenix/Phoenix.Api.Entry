using Phoenix.DataHandle.Api;

namespace Phoenix.Api.Entry.Controllers
{
    public class EntryController : ApplicationController
    {
        public EntryController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<EntryController> logger)
            : base(phoenixContext, userManager, logger)
        {
        }

        protected async Task<IEnumerable<ApplicationUserApi>> GetApplicationUsersAsync(IEnumerable<User> users)
        {
            if (users is null)
                throw new ArgumentNullException(nameof(users));

            var nonObviatedUsers = users.Where(u => u.ObviatedAt == null);

            var tore = new List<ApplicationUserApi>(users.Count());
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.AspNetUserId.ToString());
                var roleRanks = await _userManager.GetRoleRanksAsync(appUser);

                tore.Add(new(user, appUser, roleRanks.ToList()));
            }

            return tore;
        }

        protected Course? FindCourse(int courseId)
        {
            return this.PhoenixUser?.Schools
                .SelectMany(s => s.Courses)
                .SingleOrDefault(c => c.Id == courseId);
        }

        protected User? FindUser(int userId)
        {
            return this.PhoenixUser?.Schools
                .SelectMany(s => s.Users)
                .SingleOrDefault(u => u.AspNetUserId == userId);
        }
    }
}
