using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Entry.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class UserEntryController<TRoleRank> : EntryController
        where TRoleRank : Enum
    {
        public UserEntryController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<UserEntryController<TRoleRank>> logger)
            : base(phoenixContext, userManager, logger)
        {
        }


        [HttpPost("{role}")]
        public abstract Task<ApplicationUserApi?> PostAsync([FromBody] ApplicationUserApi appUserApi,
            TRoleRank role, [FromQuery, Required] int[] school_ids);

        [HttpGet]
        public abstract Task<IEnumerable<ApplicationUserApi>?> GetAsync();

        [HttpGet("{role}")]
        public abstract IEnumerable<ApplicationUserApi>? Get(TRoleRank role);

        [HttpGet("{id}")]
        public abstract ApplicationUserApi? Get(int id);

        [HttpPut("{id}")]
        public abstract Task<ApplicationUserApi?> PutAsync(int id, [FromBody] ApplicationUserApi appUserApi);

        [HttpPut("{id}/courses")]
        public abstract Task<IEnumerable<CourseApi>?> PutCoursesAsync(int id, [FromBody] List<int> courseIds);

        [HttpDelete("{id}")]
        public abstract Task<IActionResult> DeleteAsync(int id);

        protected User? FindUser(int userId)
        {
            return this.PhoenixUser?.Schools
                .SelectMany(s => s.Users)
                .SingleOrDefault(u => u.AspNetUserId == userId);
        }
    }
}
