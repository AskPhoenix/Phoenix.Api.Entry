using Phoenix.DataHandle.Api.Types;
using Phoenix.DataHandle.Main.Types;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Entry.Controllers
{
    [ApiExplorerSettings(GroupName = "5")]
    public class ClientController : UserEntryController<ClientRoleRankApi>
    {
        public ClientController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<ClientController> logger)
            : base(phoenixContext, userManager, logger)
        {
        }

        #region POST

        [HttpPost("parent")]
        public async Task<ApplicationUserApi?> PostParentAsync([FromBody] ApplicationUserApi appUserApi,
            [FromQuery, Required] int[] school_ids)
        {
            _logger.LogInformation("Entry -> Client -> Post -> Parent");

            if (string.IsNullOrWhiteSpace(appUserApi.PhoneNumber))
                return null;

            return await this.CreateUserAsync(appUserApi,
                RoleRank.Parent, depOrder: 0, appUserApi.PhoneNumber, school_ids);
        }

        [HttpPost("student")]
        public async Task<ApplicationUserApi?> PostStudentAsync([FromBody] ApplicationUserApi appUserApi,
            int? parent1_id, int? parent2_id, [FromQuery, Required] int[] school_ids)
        {
            _logger.LogInformation("Entry -> Client -> Post -> Student");

            var parents = new List<User>(2);
            var appParents = new List<ApplicationUser>(2);

            foreach (var parentId in new int?[2] { parent1_id, parent2_id })
            {
                if (!parentId.HasValue)
                    continue;

                var parent = this.FindUser(parentId.Value);
                if (parent is null)
                    continue;
                parents.Add(parent);

                var appParent = await _userManager.FindByIdAsync(parent.AspNetUserId.ToString());
                appParents.Add(appParent);
            }

            var studentDependence = this.CalculateStudentDependence(appUserApi, parents, appParents);
            if (studentDependence is null)
                return null;

            return await this.CreateUserAsync(appUserApi, RoleRank.Student,
                depOrder: studentDependence.Item1, linkedPhone: studentDependence.Item2, school_ids);
        }

        #endregion

        #region GET

        public override async Task<IEnumerable<ApplicationUserApi>?> GetAsync(ClientRoleRankApi role)
        {
            _logger.LogInformation("Entry -> Client -> Get -> {role}", role.ToString());

            var roleRank = ClientRoleRankApiExtensions.ConvertToRoleRank(role);

            return (await this.GetAsync())?.Where(au => au.Roles.Any(r => r.ToRoleRank() == roleRank));
        }

        // TODO: Get Parents/Children

        #endregion

        #region PUT

        public override async Task<ApplicationUserApi?> PutAsync(int id,
            [FromBody] ApplicationUserApi appUserApi)
        {
            _logger.LogInformation("Entry -> Client -> Put -> {id}", id);

            if (string.IsNullOrWhiteSpace(appUserApi.PhoneNumber))
                return null;

            var user = this.FindUser(id);
            if (user is null)
                return null;

            var parents = user.Parents
                .Where(p => !p.ObviatedAt.HasValue);

            var appParents = parents
                .Select(p => _userManager.FindByIdAsync(p.AspNetUserId.ToString()).Result);

            var studentDependence = this.CalculateStudentDependence(appUserApi, parents, appParents);
            if (studentDependence is null)
                return null;

            return await this.UpdateUserAsync(id, appUserApi,
                depOrder: studentDependence.Item1, linkedPhone: studentDependence.Item2);
        }

        #endregion

        private Tuple<int, string>? CalculateStudentDependence(ApplicationUserApi appUserApi,
            IEnumerable<User> parents, IEnumerable<ApplicationUser> appParents)
        {
            int depOrder;
            string linkedPhone;

            if (appUserApi.User.IsSelfDetermined)
            {
                if (string.IsNullOrWhiteSpace(appUserApi.PhoneNumber))
                    return null;

                depOrder = 0;
                linkedPhone = appUserApi.PhoneNumber;
            }
            else
            {
                if (!parents.Any())
                    return null;

                depOrder = parents.First()
                    .Children
                    .Select(c => c.DependenceOrder)
                    .DefaultIfEmpty(0)
                    .Max() + 1;

                linkedPhone = appParents.First().PhoneNumber;
            }

            return Tuple.Create(depOrder, linkedPhone);
        }
    }
}
