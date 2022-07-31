using Phoenix.DataHandle.Api;
using Phoenix.DataHandle.Main.Types;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Entry.Controllers
{
    public class SchoolConnectionController : ApplicationController
    {
        private readonly SchoolConnectionRepository _schoolConnectionRepository;

        public SchoolConnectionController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<SchoolConnectionController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _schoolConnectionRepository = new(phoenixContext);
        }

        [HttpPost("facebook/{key}")]
        public async Task<SchoolConnectionApi?> FacebookRegisterAsync(
            [Required] int school_id, string key, bool activate = true)
        {
            // TODO: Allow registration to other channels as well
            // TODO: Connect to Azure Bot

            _logger.LogInformation("Entry -> School Connection -> Facebook -> Register -> {id}", school_id);

            if (string.IsNullOrWhiteSpace(key))
                return null;

            var school = this.FindSchool(school_id);
            if (school is null)
                return null;

            SchoolConnection connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .RegisterAsync(ChannelProvider.Facebook, key, school_id, activate);
            }
            catch(InvalidOperationException) 
            {
                return null;
            }

            return new SchoolConnectionApi(connection);
        }

        [HttpGet("facebook/{key}")]
        public async Task<SchoolConnectionApi?> FacebookGetAsync(string key)
        {
            _logger.LogInformation("Entry -> School Connection -> Facebook -> Get");

            if (string.IsNullOrWhiteSpace(key))
                return null;

            var connection = await _schoolConnectionRepository
                .FindUniqueAsync(ChannelProvider.Facebook, key);

            if (connection is null)
                return null;

            var school = this.FindSchool(connection.TenantId);
            if (school is null)
                return null;

            return new SchoolConnectionApi(connection);
        }

        [HttpPut("facebook/{key}/connect")]
        public async Task<SchoolConnectionApi?> FacebookConnectAsync(string key)
        {
            _logger.LogInformation("Entry -> School Connection -> Facebook -> Connect");

            if (string.IsNullOrWhiteSpace(key))
                return null;

            School? school;
            SchoolConnection? connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .FindUniqueAsync(ChannelProvider.Facebook, key);

                if (connection is null)
                    return null;

                school = this.FindSchool(connection.TenantId);
                if (school is null)
                    return null;

                connection = await _schoolConnectionRepository
                    .ConnectAsync(ChannelProvider.Facebook, key);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return new SchoolConnectionApi(connection);
        }

        [HttpPut("facebook/{key}/disconnect")]
        public async Task<SchoolConnectionApi?> FacebookDisconnectAsync(string key)
        {
            _logger.LogInformation("Entry -> School Connection -> Facebook -> Disconnect");

            if (string.IsNullOrWhiteSpace(key))
                return null;

            School? school;
            SchoolConnection? connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .FindUniqueAsync(ChannelProvider.Facebook, key);

                if (connection is null)
                    return null;

                school = this.FindSchool(connection.TenantId);
                if (school is null)
                    return null;

                connection = await _schoolConnectionRepository
                    .DisconnectAsync(ChannelProvider.Facebook, key);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return new SchoolConnectionApi(connection);
        }

        [HttpDelete("facebook/{key}")]
        public async Task<IActionResult> FacebookDeleteAsync(string key)
        {
            _logger.LogInformation("Entry -> School Connection -> Facebook -> Delete");

            if (string.IsNullOrWhiteSpace(key))
                return BadRequest();

            var connection = await _schoolConnectionRepository
                .FindUniqueAsync(ChannelProvider.Facebook, key);

            if (connection is null)
                return BadRequest();

            var school = this.FindSchool(connection.TenantId);
            if (school is null)
                return BadRequest();

            await _schoolConnectionRepository.DeleteAsync(connection);

            return Ok();
        }
    }
}
