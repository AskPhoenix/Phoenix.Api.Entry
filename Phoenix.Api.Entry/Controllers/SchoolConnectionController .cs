using Microsoft.AspNetCore.Authorization;
using Phoenix.DataHandle.Api;
using Phoenix.DataHandle.Main.Types;
using Phoenix.DataHandle.Senders;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Entry.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "1b")]
    public class SchoolConnectionController : ApplicationController
    {
        private readonly EmailSender _emailSender;
        private readonly SchoolConnectionRepository _schoolConnectionRepository;

        public SchoolConnectionController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<SchoolConnectionController> logger,
            EmailSender emailSender)
            : base(phoenixContext, userManager, logger)
        {
            _emailSender = emailSender;
            _schoolConnectionRepository = new(phoenixContext);
        }

        #region Facebook POST

        [HttpPost("facebook/{key}")]
        public async Task<SchoolConnectionApi?> FacebookRegisterAsync(
            [Required] int school_id, string key, bool activate = true)
        {
            // TODO: Connect to Azure Bot

            _logger.LogInformation("Entry -> School Connection -> Facebook -> Register -> {id}", school_id);

            if (string.IsNullOrWhiteSpace(key))
                return null;

            var school = FindSchool(school_id);
            if (school is null)
                return null;

            SchoolConnection connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .RegisterAsync(ChannelProvider.Facebook, key, school_id, activate);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            await _emailSender.SendAsync(
                to: "it@askphoenix.gr",
                subject: "[Pavo API] New School Connection",
                plainTextContent: $"There is a new Facebook connection awating for school '{school.Name}' " +
                    $"with id {school.Id}.\n\nFacebook key: {key}\n");

            return new SchoolConnectionApi(connection);
        }

        #endregion

        #region Facebook GET

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

            var school = FindSchool(connection.TenantId);
            if (school is null)
                return null;

            return new SchoolConnectionApi(connection);
        }

        #endregion

        #region Facebook PUT

        [HttpPut("facebook/{key}/connect")]
        public async Task<SchoolConnectionApi?> FacebookConnectAsync(string key)
        {
            _logger.LogInformation("Entry -> School Connection -> Facebook -> Connect");

            if (string.IsNullOrWhiteSpace(key))
                return null;

            SchoolConnection? connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .FindUniqueAsync(ChannelProvider.Facebook, key);

                if (connection is null)
                    return null;

                var school = FindSchool(connection.TenantId);
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

            SchoolConnection? connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .FindUniqueAsync(ChannelProvider.Facebook, key);

                if (connection is null)
                    return null;

                var school = FindSchool(connection.TenantId);
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

        [HttpPut("facebook/{key}/update-token")]
        public async Task<SchoolConnectionApi?> FacebookDisconnectAsync(string key, [Required] string token)
        {
            _logger.LogInformation("Entry -> School Connection -> Facebook -> Update Token");

            if (string.IsNullOrWhiteSpace(key))
                return null;
            if (string.IsNullOrWhiteSpace(token))
                return null;

            SchoolConnection? connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .FindUniqueAsync(ChannelProvider.Facebook, key);

                if (connection is null)
                    return null;

                var school = FindSchool(connection.TenantId);
                if (school is null)
                    return null;

                connection.ChannelToken = token;

                connection = await _schoolConnectionRepository.UpdateAsync(connection);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return new SchoolConnectionApi(connection);
        }

        #endregion

        #region Facebook DELETE

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

            var school = FindSchool(connection.TenantId);
            if (school is null)
                return BadRequest();

            await _schoolConnectionRepository.DeleteAsync(connection);

            return Ok();
        }

        #endregion
    }
}
