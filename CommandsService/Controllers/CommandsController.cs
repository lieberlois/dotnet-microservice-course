using System.Collections.Generic;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/platforms/{platformId}/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommandRepository _commandRepository;
        private readonly IMapper _mapper;

        public CommandsController(ICommandRepository commandRepository, IMapper mapper)
        {
            this._commandRepository = commandRepository;
            this._mapper = mapper;
        }

        [HttpGet("{commandId}", Name = "GetCommandForPlatform")]
        public ActionResult<CommandReadDto> GetCommandForPlatform(int platformId, int commandId)
        {
            if (!this._commandRepository.PlatformExists(platformId))
            {
                return NotFound();
            }

            var command = this._commandRepository.GetCommand(platformId, commandId);

            if (command == null)
            {
                return NotFound();
            }

            return Ok(
                this._mapper.Map<CommandReadDto>(command)
            );
        }

        [HttpPost]
        public ActionResult<CommandReadDto> CreateCommandForPlatform(int platformId, CommandCreateDto commandCreateDto)
        {
            if (!this._commandRepository.PlatformExists(platformId))
            {
                return NotFound();
            }

            var commandModel = this._mapper.Map<Command>(commandCreateDto);

            this._commandRepository.CreateCommand(platformId, commandModel);
            this._commandRepository.SaveChanges();

            var commandReadDto = this._mapper.Map<CommandReadDto>(commandModel);

            return CreatedAtRoute(
                nameof(GetCommandForPlatform),
                new { platformId = platformId, commandId = commandReadDto.Id },
                commandReadDto
            );
        }
    }
}