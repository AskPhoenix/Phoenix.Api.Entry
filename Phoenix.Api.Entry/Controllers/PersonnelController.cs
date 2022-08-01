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

        public override async Task<ApplicationUserApi?> PostAsync([FromBody] ApplicationUserApi appUserApi,
            PersonnelRoleRankApi role, [FromQuery, Required] int[] school_ids)
        {
            _logger.LogInformation("Entry -> Personnel -> Post");

            if ((int)role < 0 || (int)role >= RoleExtensions.StaffRoleRanks.Length)
                return null;

            List<School> schools = new(school_ids.Count());
            foreach (var schoolId in school_ids)
            {
                var school = this.FindSchool(schoolId);
                if (school is not null)
                    schools.Add(school);
            }

            if (!schools.Any())
                return null;

            var roleRank = PersonnelRoleRankApiExtensions.ConvertToRoleRank(role);

            var appUser = appUserApi.ToAppUser();
            appUser.Id = 0;

            await _userManager.CreateAsync(appUser);
            await _userManager.AddToRoleAsync(appUser, roleRank.ToNormalizedString());

            var user = appUserApi.User.ToUser();
            user.AspNetUserId = appUser.Id;
            user.IsSelfDetermined = true;
            user.DependenceOrder = 0;

            foreach (var school in schools)
                user.Schools.Add(school);

            user = await _userRepository.CreateAsync(user);

            return new ApplicationUserApi(user, appUser, new(1) { roleRank });
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

            var user = FindUser(id);
            if (user is null)
                return null;

            var appUser = await _userManager.FindByIdAsync(user.AspNetUserId.ToString());

            user = appUserApi.User.ToUser(user);
            appUser = appUserApi.ToAppUser(appUser);

            user.IsSelfDetermined = true;
            user.DependenceOrder = 0;

            user = await _userRepository.UpdateAsync(user);
            await _userManager.UpdateAsync(appUser);

            var roleRanks = await _userManager.GetRoleRanksAsync(appUser);

            return new ApplicationUserApi(user, appUser, roleRanks.ToList());
        }

        #endregion
    }
}
