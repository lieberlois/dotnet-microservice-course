using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlatformService.AsyncDataServices.Http;
using PlatformService.Data;
using PlatformService.Dtos;
using PlatformService.Models;
using PlatformService.SyncDataServices.Http;

namespace PlatformService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformRepository _repository;
        private readonly IMapper _mapper;
        private readonly ICommandDataClient _commandClient;
        private readonly IMessageBusClient _messageBusClient;

        public PlatformsController(
            IPlatformRepository repository,
            IMapper mapper,
            ICommandDataClient commandClient,
            IMessageBusClient messageBusClient
        )
        {
            this._repository = repository;
            this._mapper = mapper;
            this._commandClient = commandClient;
            this._messageBusClient = messageBusClient;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            var platforms = this._repository.GetAllPlatforms();
            return Ok(this._mapper.Map<IEnumerable<PlatformReadDto>>(platforms));
        }

        [HttpGet("{id}", Name = "GetPlatformById")]
        public ActionResult<PlatformReadDto> GetPlatformById(int id)
        {
            var platform = this._repository.GetPlatformById(id);

            if (platform == null)
            {
                return NotFound();
            }

            return Ok(this._mapper.Map<PlatformReadDto>(platform));
        }

        [HttpPost]
        public async Task<ActionResult<PlatformReadDto>> CreatePlatform(PlatformCreateDto dto)
        {
            var platformModel = this._mapper.Map<Platform>(dto);
            this._repository.CreatePlatform(platformModel);
            this._repository.SaveChanges();

            var platformReadDto = this._mapper.Map<PlatformReadDto>(platformModel);

            // Synchronous Message 
            try
            {
                await this._commandClient.SendPlatformToCommand(platformReadDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"---> Could not send to Commands Service: {ex.Message}");
            }

            // Async Message
            try
            {
                this._messageBusClient.PublishNewPlatform(
                    this._mapper.Map<PlatformPublishDto>(platformReadDto)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"---> Could not publish to Message Bus: {ex.Message}");
            }

            return CreatedAtRoute(
                nameof(GetPlatformById),
                new { Id = platformReadDto.Id },
                platformReadDto
            );
        }
    }
}