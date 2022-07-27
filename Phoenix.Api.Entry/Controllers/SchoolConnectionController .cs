using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Phoenix.DataHandle.Api;
using Phoenix.DataHandle.Api.Models;
using Phoenix.DataHandle.Identity;
using Phoenix.DataHandle.Main.Models;
using Phoenix.DataHandle.Main.Types;
using Phoenix.DataHandle.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Phoenix.Api.Entry.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    [Route("api/[controller]")]
    public class SchoolConnectionController : ApplicationController
    {
        private readonly SchoolConnectionRepository _schoolConnectionRepository;

        public SchoolConnectionController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<SchoolController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _schoolConnectionRepository = new(phoenixContext);
        }

        [HttpPost("facebook/{school_id}")]
        public async Task<SchoolConnectionApi?> FacebookRegisterAsync(
            int school_id, [Required] string facebook_key, bool activate = true)
        {
            // TODO: Allow registration to other channels as well
            // TODO: Connect to Azure Bot

            _logger.LogInformation("Entry -> School Connection -> Facebook -> Register -> {id}", school_id);

            if (string.IsNullOrWhiteSpace(facebook_key))
                return null;

            var school = this.PhoenixUser?
                .Schools
                .SingleOrDefault(s => s.Id == school_id);

            if (school is null)
                return null;

            SchoolConnection connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .RegisterAsync(ChannelProvider.Facebook, facebook_key, school_id, activate);
            }
            catch(InvalidOperationException) 
            {
                return null;
            }

            return new SchoolConnectionApi(connection);
        }

        [HttpGet("facebook/{school_id}")]
        public IEnumerable<SchoolConnectionApi>? FacebookGet(int school_id)
        {
            _logger.LogInformation("Entry -> School Connection -> Facebook -> Get");

            var school = this.PhoenixUser?
                .Schools
                .SingleOrDefault(s => s.Id == school_id);

            if (school is null)
                return null;

            return school.SchoolConnections
                .Select(c => new SchoolConnectionApi(c));
        }

        [HttpPut("facebook/connect")]
        public async Task<SchoolConnectionApi?> FacebookConnectAsync([Required] string facebook_key)
        {
            _logger.LogInformation("Entry -> School Connection -> Facebook -> Connect");

            if (string.IsNullOrWhiteSpace(facebook_key))
                return null;

            School? school;
            SchoolConnection? connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .FindUniqueAsync(ChannelProvider.Facebook, facebook_key);

                if (connection is null)
                    return null;

                school = this.PhoenixUser?
                .Schools
                .SingleOrDefault(s => s.Id == connection.TenantId);

                if (school is null)
                    return null;

                connection = await _schoolConnectionRepository
                    .ConnectAsync(ChannelProvider.Facebook, facebook_key);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return new SchoolConnectionApi(connection);
        }

        [HttpPut("facebook/disconnect")]
        public async Task<SchoolConnectionApi?> FacebookDisconnectAsync([Required] string facebook_key)
        {
            _logger.LogInformation("Entry -> School Connection -> Facebook -> Disconnect");

            if (string.IsNullOrWhiteSpace(facebook_key))
                return null;

            School? school;
            SchoolConnection? connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .FindUniqueAsync(ChannelProvider.Facebook, facebook_key);

                if (connection is null)
                    return null;

                school = this.PhoenixUser?
                .Schools
                .SingleOrDefault(s => s.Id == connection.TenantId);

                if (school is null)
                    return null;

                connection = await _schoolConnectionRepository
                    .DisconnectAsync(ChannelProvider.Facebook, facebook_key);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return new SchoolConnectionApi(connection);
        }

        [HttpDelete("facebook")]
        public async Task<IActionResult> FacebookDeleteAsync([Required] string facebook_key)
        {
            _logger.LogInformation("Entry -> School Connection -> Facebook -> Delete");

            if (string.IsNullOrWhiteSpace(facebook_key))
                return BadRequest();

            var connection = await _schoolConnectionRepository
                .FindUniqueAsync(ChannelProvider.Facebook, facebook_key);

            if (connection is null)
                return BadRequest();

            var school = this.PhoenixUser?
                .Schools
                .SingleOrDefault(s => s.Id == connection.TenantId);

            if (school is null)
                return BadRequest();

            await _schoolConnectionRepository.DeleteAsync(connection);

            return Ok();
        }
    }
}
