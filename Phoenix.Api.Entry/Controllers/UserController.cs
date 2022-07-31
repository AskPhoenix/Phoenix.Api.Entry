namespace Phoenix.Api.Entry.Controllers
{
    public abstract class UserController : EntryController<ApplicationUser, ApplicationUserApi>
    {
        public UserController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<UserController> logger)
            : base(phoenixContext, userManager, logger)
        {
        }
    }
}
