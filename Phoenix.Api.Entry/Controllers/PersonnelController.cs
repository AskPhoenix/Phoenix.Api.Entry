using Phoenix.DataHandle.Api.Types;
using Phoenix.DataHandle.Main.Types;

namespace Phoenix.Api.Entry.Controllers
{
    [ApiExplorerSettings(GroupName = "4")]
    public class PersonnelController : UserController
    {
        public PersonnelController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<PersonnelController> logger)
            : base(phoenixContext, userManager, logger)
        {
        }

        #region POST

        [HttpPost]
        public async Task<ApplicationUserApi?> PostAsync([FromBody] ApplicationUserApi appUserApi,
            PersonnelRoleRankApi role)
        {
            // TODO: School, Courses

            _logger.LogInformation("Entry -> Client -> Post");

            if (!CheckUserAuth())
                return null;

            var roleRank = (RoleRank)(role + RoleHierarchy.StaffRolesBase);

            var appUser = appUserApi.ToAppUser();
            appUser.Id = 0;

            await _userManager.CreateAsync(appUser);
            await _userManager.AddToRoleAsync(appUser, roleRank.ToNormalizedString());

            var user = appUserApi.User.ToUser();
            user.AspNetUserId = appUser.Id;
            user.IsSelfDetermined = true;
            user.DependenceOrder = 0;

            user = await _userRepository.CreateAsync(user);

            return new ApplicationUserApi(user, appUser);
        }

        #endregion

        #region GET

        [HttpGet]
        public override IEnumerable<ApplicationUserApi>? Get()
        {
            throw new NotImplementedException();
        }

        [HttpGet("{id}")]
        public override ApplicationUserApi? Get(int id)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region PUT

        [HttpPut]
        public override Task<ApplicationUserApi?> PutAsync(int id, [FromBody] ApplicationUserApi appUserApi)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region DELETE

        [HttpDelete]
        public override Task<IActionResult> DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
