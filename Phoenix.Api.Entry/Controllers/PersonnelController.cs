using Phoenix.DataHandle.Api.Types;
using Phoenix.DataHandle.Main.Types;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Entry.Controllers
{
    [ApiExplorerSettings(GroupName = "4")]
    public class PersonnelController : UserEntryController<PersonnelRoleRankApi>
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
            PersonnelRoleRankApi role, [FromQuery, Required] int[] school_ids)
        {
            _logger.LogInformation("Entry -> Personnel -> Post");

            var roleRank = PersonnelRoleRankApiExtensions.ConvertToRoleRank(role);

            if (string.IsNullOrWhiteSpace(appUserApi.PhoneNumber))
                return null;
            
            return await this.CreateUserAsync(appUserApi,
                roleRank, depOrder: 0, linkedPhone: appUserApi.PhoneNumber, school_ids);
        }

        #endregion

        #region GET

        public override async Task<IEnumerable<ApplicationUserApi>?> GetAsync(PersonnelRoleRankApi role)
        {
            _logger.LogInformation("Entry -> Personnel -> Get -> {role}", role.ToString());

            var roleRank = PersonnelRoleRankApiExtensions.ConvertToRoleRank(role);

            return (await this.GetAsync())?.Where(au => au.Roles.Any(r => r.ToRoleRank() == roleRank));
        }

        #endregion

        #region PUT

        public override async Task<ApplicationUserApi?> PutAsync(int id,
            [FromBody] ApplicationUserApi appUserApi)
        {
            _logger.LogInformation("Entry -> Personnel -> Put -> {id}", id);

            if (string.IsNullOrWhiteSpace(appUserApi.PhoneNumber))
                return null;

            return await this.UpdateUserAsync(id, appUserApi, 
                depOrder: 0, linkedPhone: appUserApi.PhoneNumber);
        }

        #endregion
    }
}
