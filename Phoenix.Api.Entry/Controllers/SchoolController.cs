﻿using Microsoft.AspNetCore.Authorization;
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
    public class SchoolController : ApplicationController
    {
        private readonly SchoolRepository _schoolRepository;
        private readonly SchoolConnectionRepository _schoolConnectionRepository;

        public SchoolController(
            PhoenixContext phoenixContext,
            ApplicationUserManager userManager,
            ILogger<SchoolController> logger)
            : base(phoenixContext, userManager, logger)
        {
            _schoolRepository = new(phoenixContext);
            _schoolConnectionRepository = new(phoenixContext);
        }

        [HttpPost]
        public async Task<SchoolApi?> PostAsync([FromBody] SchoolApi schoolApi)
        {
            _logger.LogInformation("Entry -> School -> Post");

            if (!this.CheckUserAuth())
                return null;

            var school = schoolApi.ToSchool();
            school.Id = 0;
            school.Code = 0;
            school.SchoolSetting.SchoolId = 0;
            school.Users.Add(this.PhoenixUser!);

            school = await _schoolRepository.CreateAsync(school);

            school.Code = -school.Id;
            school = await _schoolRepository.UpdateAsync(school);

            return new SchoolApi(school);
        }

        [HttpGet]
        public IEnumerable<SchoolApi>? Get()
        {
            _logger.LogInformation("Entry -> School -> Get");

            return this.PhoenixUser?
                .Schools
                .Select(s => new SchoolApi(s));
        }

        [HttpGet("{id}")]
        public SchoolApi? Get(int id)
        {
            _logger.LogInformation("Entry -> School -> Get -> {id}", id);

            var school = this.PhoenixUser?
                .Schools
                .SingleOrDefault(s => s.Id == id);

            if (school is null)
                return null;

            return new SchoolApi(school);
        }

        [HttpPut("{id}")]
        public async Task<SchoolApi?> PutAsync(int id, [FromBody] SchoolApi schoolApi)
        {
            _logger.LogInformation("Entry -> School -> Put -> {id}", id);

            var school = this.PhoenixUser?
                .Schools
                .SingleOrDefault(s => s.Id == id);

            if (school is null)
                return null;

            school = await _schoolRepository.UpdateAsync(schoolApi.ToSchool(school));

            return new SchoolApi(school);
        }

        [HttpDelete("{id}")]
        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Entry -> School -> Delete -> {id}", id);

            if (!this.CheckUserAuth())
                return;

            var isEnrolled = this.PhoenixUser!
                .Schools
                .Any(s => s.Id == id);

            if (!isEnrolled)
                return;

            await _schoolRepository.DeleteAsync(id);
        }

        [HttpPost("facebook/register/{id}")]
        public async Task<IActionResult> FacebookRegisterAsync(int id, [Required] string facebookKey)
        {
            // TODO: Allow registration to other channels as well
            // TODO: Connect to Azure Bot

            _logger.LogInformation("Entry -> School -> Register -> {id}", id);

            if (string.IsNullOrWhiteSpace(facebookKey))
                return BadRequest();

            var school = this.PhoenixUser?
                .Schools
                .SingleOrDefault(s => s.Id == id);

            if (school is null)
                return BadRequest();

            SchoolConnection connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .RegisterAsync(ChannelProvider.Facebook, facebookKey, id);
            }
            catch(InvalidOperationException e) 
            {
                return BadRequest(e.Message);
            }

            return Ok(new SchoolConnectionApi(connection));
        }

        [HttpPost("facebook/connect")]
        public async Task<IActionResult> FacebookConnectAsync([Required] string facebookKey)
        {
            _logger.LogInformation("Entry -> School -> Connect");

            if (string.IsNullOrWhiteSpace(facebookKey))
                return BadRequest();

            School? school;
            SchoolConnection? connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .FindUniqueAsync(ChannelProvider.Facebook, facebookKey);

                if (connection is null)
                    return BadRequest();

                school = this.PhoenixUser?
                .Schools
                .SingleOrDefault(s => s.Id == connection.TenantId);

                if (school is null)
                    return BadRequest();

                connection = await _schoolConnectionRepository
                    .ConnectAsync(ChannelProvider.Facebook, facebookKey);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }

            return Ok(new SchoolConnectionApi(connection));
        }

        [HttpPost("facebook/disconnect")]
        public async Task<IActionResult> FacebookDisconnectAsync([Required] string facebookKey)
        {
            _logger.LogInformation("Entry -> School -> Disconnect");

            if (string.IsNullOrWhiteSpace(facebookKey))
                return BadRequest();

            School? school;
            SchoolConnection? connection;
            try
            {
                connection = await _schoolConnectionRepository
                    .FindUniqueAsync(ChannelProvider.Facebook, facebookKey);

                if (connection is null)
                    return BadRequest();

                school = this.PhoenixUser?
                .Schools
                .SingleOrDefault(s => s.Id == connection.TenantId);

                if (school is null)
                    return BadRequest();

                connection = await _schoolConnectionRepository
                    .DisconnectAsync(ChannelProvider.Facebook, facebookKey);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }

            return Ok(new SchoolConnectionApi(connection));
        }

        [HttpDelete("facebook")]
        public async Task<IActionResult> FacebookDeleteConnectionAsync([Required] string facebookKey)
        {
            _logger.LogInformation("Entry -> School -> Connection -> Delete");

            if (string.IsNullOrWhiteSpace(facebookKey))
                return BadRequest();

            var connection = await _schoolConnectionRepository
                .FindUniqueAsync(ChannelProvider.Facebook, facebookKey);

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
