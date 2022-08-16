using Microsoft.AspNetCore.Identity;
using Phoenix.DataHandle.Base;
using Phoenix.DataHandle.Main.Types;

namespace Phoenix.Api.Entry.Controllers
{
    public abstract class UserEntryController<TRoleRankApi> : EntryController
        where TRoleRankApi : Enum
    {
        private readonly ApplicationStore _appStore;

        public UserEntryController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            IUserStore<ApplicationUser> appStore,
            ILogger<UserEntryController<TRoleRankApi>> logger)
            : base(phoenixContext, userManager, logger)
        {
            _appStore = (ApplicationStore)appStore;
        }

        #region GET

        [HttpGet]
        public async Task<IEnumerable<ApplicationUserApi>?> GetAsync()
        {
            _logger.LogInformation("Entry -> User -> Get");

            var users = FindUsers();
            if (users is null)
                return null;

            return await this.GetApplicationUsersApiAsync(users);
        }

        [HttpGet("{role}")]
        public abstract Task<IEnumerable<ApplicationUserApi>?> GetAsync(TRoleRankApi role);

        [HttpGet("{id}")]
        public async Task<ApplicationUserApi?> GetAsync(int id)
        {
            _logger.LogInformation("Entry -> User -> Get -> {id}", id);

            var user = FindUser(id);
            if (user is null)
                return null;

            var appUser = await _userManager.FindByIdAsync(user.AspNetUserId.ToString());
            var roleRanks = await _userManager.GetRoleRanksAsync(appUser);

            return new ApplicationUserApi(user, appUser, roleRanks.ToList());
        }

        [HttpGet("{id}/schools")]
        public IEnumerable<SchoolApi>? GetSchools(int id)
        {
            _logger.LogInformation("Entry -> User -> Get -> Schools -> {id}", id);

            var user = FindUser(id);
            if (user is null)
                return null;

            return user.Schools
                .Where(s => !s.ObviatedAt.HasValue)
                .Select(s => new SchoolApi(s));
        }

        [HttpGet("{id}/courses")]
        public IEnumerable<CourseApi>? GetCourses(int id)
        {
            _logger.LogInformation("Entry -> User -> Get -> Courses -> {id}", id);

            var user = FindUser(id);
            if (user is null)
                return null;

            return user.Courses
                .Where(c => !c.ObviatedAt.HasValue)
                .Select(c => new CourseApi(c));
        }

        #endregion

        #region PUT

        [HttpPut("{id}")]
        public abstract Task<ApplicationUserApi?> PutAsync(int id, [FromBody] ApplicationUserApi appUserApi);

        [HttpPut("{id}/courses")]
        public async Task<IEnumerable<CourseApi>?> PutCoursesAsync(int id, [FromBody] List<int> courseIds)
        {
            _logger.LogInformation("Entry -> User -> Put -> Courses -> {id}", id);

            var user = FindUser(id);
            if (user is null)
                return null;

            user.Courses.Clear();

            Course? course;
            foreach (var courseId in courseIds)
            {
                course = this.FindCourse(courseId);
                if (course is null)
                    continue;

                user.Courses.Add(course);
            }

            user = await _userRepository.UpdateAsync(user);

            return user.Courses.Select(c => new CourseApi(c));
        }

        #endregion

        #region DELETE

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> User -> Delete -> {id}", id);

            if (!CheckUserAuth())
                return Unauthorized();

            var user = FindUser(id);
            if (user is null)
                return BadRequest();

            await _userRepository.ObviateAsync(user);

            return Ok();
        }

        #endregion

        protected List<School>? FindSchools(int[] schoolIds)
        {
            List<School> schools = new(schoolIds.Count());
            foreach (var schoolId in schoolIds)
            {
                var school = this.FindSchool(schoolId);
                if (school is not null)
                    schools.Add(school);
            }

            if (!schools.Any())
                return null;

            return schools;
        }

        protected virtual async Task<ApplicationUserApi?> CreateUserAsync(ApplicationUserApi appUserApi,
            RoleRank roleRank, int depOrder, string linkedPhone, int[] school_ids)
        {
            if (roleRank == RoleRank.None)
                return null;

            if (!string.IsNullOrEmpty(appUserApi.PhoneNumber))
                if ((await _userManager.FindByPhoneNumberAsync(appUserApi.PhoneNumber)) is not null)
                    return null;

            var schools = this.FindSchools(school_ids);
            if (schools is null)
                return null;

            var appUser = Activator.CreateInstance<ApplicationUser>();
            var username = UserExtensions.GenerateUserName(schools.Select(s => s.Code), linkedPhone, depOrder);

            await _appStore.SetUserNameAsync(appUser, username);
            await _appStore.SetNormalizedUserNameAsync(appUser, ApplicationUser.NormFunc(username));

            await _appStore.SetPhoneNumberAsync(appUser, appUserApi.PhoneNumber);
            await _appStore.SetPhoneNumberConfirmedAsync(appUser, false);

            await _userManager.CreateAsync(appUser);
            await _userManager.AddToRoleAsync(appUser, roleRank.ToNormalizedString());

            var user = appUserApi.User.ToUser();
            user.AspNetUserId = appUser.Id;
            user.IsSelfDetermined = depOrder == 0;
            user.DependenceOrder = depOrder;

            foreach (var school in schools)
                user.Schools.Add(school);

            user = await _userRepository.CreateAsync(user);

            return new ApplicationUserApi(user, appUser, new(1) { roleRank });
        }

        protected virtual async Task<ApplicationUserApi?> UpdateUserAsync(
            int id, ApplicationUserApi appUserApi, int depOrder, string linkedPhone)
        {
            var user = FindUser(id);
            if (user is null)
                return null;

            var appUser = await _userManager.FindByIdAsync(user.AspNetUserId.ToString());

            user = appUserApi.User.ToUser(user);

            user.IsSelfDetermined = depOrder == 0;
            user.DependenceOrder = depOrder;

            var username = UserExtensions.GenerateUserName(
                user.Schools.Select(s => s.Code), linkedPhone, depOrder);

            await _appStore.SetUserNameAsync(appUser, username);
            await _appStore.SetNormalizedUserNameAsync(appUser, ApplicationUser.NormFunc(username));

            await _appStore.SetPhoneNumberAsync(appUser, appUserApi.PhoneNumber);

            user = await _userRepository.UpdateAsync(user);
            await _userManager.UpdateAsync(appUser);

            var roleRanks = await _userManager.GetRoleRanksAsync(appUser);

            return new ApplicationUserApi(user, appUser, roleRanks.ToList());
        }
    }
}
