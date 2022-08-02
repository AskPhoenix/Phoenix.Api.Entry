namespace Phoenix.Verification.Api.Entry.Tests
{
    public class SchoolEntryTests : EntryTestsBase
    {
        private const int DEMO_ID = 2;
        private const string FB_KEY = "1234";

        [Fact]
        public async void PostSchoolAsync()
        {
            var schoolApi = new SchoolApi(new School
            {
                Name = "Test School",
                Slug = "TS",
                City = "Thessaloniki",
                AddressLine = "Kapou",
                Description = "A nice school",
                SchoolSetting = new()
                {
                    Country = "Greece",
                    PrimaryLocale = "en-US",
                    SecondaryLocale = "el-GR",
                    TimeZone = "GTB Standard Time",
                    PhoneCountryCode = "+30"
                }
            });

            var schoolApi2 = await this.PostAsync($"{API_BASE}/school", schoolApi);

            Assert.NotNull(schoolApi2);
        }

        [Fact]
        public async void GetSchoolsAsync()
        {
            var schoolsApi = await this.GetAsync<IEnumerable<SchoolApi>>($"{API_BASE}/school");
            
            Assert.NotNull(schoolsApi);
            Assert.NotEmpty(schoolsApi);
        }

        [Fact]
        public async void GetSchoolAsync()
        {
            var schoolApi = await this.GetAsync<SchoolApi>($"{API_BASE}/school/{DEMO_ID}");

            Assert.NotNull(schoolApi);
        }

        [Fact]
        public async void PutSchoolAsync()
        {
            var schoolApi = await this.GetAsync<SchoolApi>($"{API_BASE}/school/{DEMO_ID}");

            Assert.NotNull(schoolApi);

            var school = schoolApi.ToSchool();
            school.Name = "Demo School Test";
            school.Description = "The best school in town";
            school.SchoolSetting.Country = "Cyprus";

            var schoolApi2 = await this.PutAsync($"{API_BASE}/school/{DEMO_ID}", new SchoolApi(school));

            Assert.NotNull(schoolApi2);
        }

        [Fact]
        public async void DeleteSchoolAsync()
        {
            var code = await this.DeleteAsync($"{API_BASE}/school/{DEMO_ID}");

            Assert.Equal(System.Net.HttpStatusCode.OK, code);
        }

        [Fact]
        public async void GetSchoolConnectionsAsync()
        {
            var connApi = await this.GetAsync<IEnumerable<SchoolConnectionApi>>(
                $"{API_BASE}/school/{DEMO_ID}/connections");

            Assert.NotNull(connApi);
        }

        [Fact]
        public async void GetSchoolCoursesAsync()
        {
            var coursesApi = await this.GetAsync<IEnumerable<CourseApi>>(
                $"{API_BASE}/school/{DEMO_ID}/courses");

            Assert.NotNull(coursesApi);
        }

        [Fact]
        public async void GetSchoolClassroomsAsync()
        {
            var classroomsApi = await this.GetAsync<IEnumerable<ClassroomApi>>(
                $"{API_BASE}/school/{DEMO_ID}/classrooms");

            Assert.NotNull(classroomsApi);
        }

        [Fact]
        public async void GetSchoolUsersAsync()
        {
            var appUsersApi = await this.GetAsync<IEnumerable<ApplicationUserApi>>(
                $"{API_BASE}/school/{DEMO_ID}/users");

            Assert.NotNull(appUsersApi);
        }

        [Fact]
        public async void PostSchoolFacebookConnectionAsync()
        {
            var schoolConnApi = await this.PostAsync<SchoolConnectionApi>(
                $"{API_BASE}/schoolconnection/facebook/{FB_KEY}?school_id={DEMO_ID}&activate=false");

            Assert.NotNull(schoolConnApi);
        }

        [Fact]
        public async void GetSchoolFacebookConnectionAsync()
        {
            var schoolConnApi = await this.GetAsync<SchoolConnectionApi>(
                $"{API_BASE}/schoolconnection/facebook/{FB_KEY}");

            Assert.NotNull(schoolConnApi);
        }

        [Fact]
        public async void ConnectSchoolFacebookConnectionAsync()
        {
            var schoolConnApi = await this.PutAsync<SchoolConnectionApi>(
                $"{API_BASE}/schoolconnection/facebook/{FB_KEY}/connect");

            Assert.NotNull(schoolConnApi);
        }

        [Fact]
        public async void DisconnectSchoolFacebookConnectionAsync()
        {
            var schoolConnApi = await this.PutAsync<SchoolConnectionApi>(
                $"{API_BASE}/schoolconnection/facebook/{FB_KEY}/disconnect");

            Assert.NotNull(schoolConnApi);
        }

        [Fact]
        public async void DeleteSchoolFacebookConnectionAsync()
        {
            var code = await this.DeleteAsync(
                $"{API_BASE}/schoolconnection/facebook/{FB_KEY}");

            Assert.Equal(System.Net.HttpStatusCode.OK, code);
        }
    }
}