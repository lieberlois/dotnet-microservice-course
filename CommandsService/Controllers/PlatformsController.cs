using System.Collections.Generic;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CommandsService.Controllers
{
    [Route("api/c/[controller]")]
    [ApiController]
    public class PlatformsController : ControllerBase
    {
        private readonly ICommandRepository _commandRepository;
        private readonly IMapper _mapper;

        public PlatformsController(ICommandRepository commandRepository, IMapper mapper)
        {
            this._commandRepository = commandRepository;
            this._mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlatformReadDto>> GetPlatforms()
        {
            var platforms = this._commandRepository.GetAllPlatforms();

            return Ok(
                this._mapper.Map<IEnumerable<PlatformReadDto>>(platforms)
            );
        }

        [HttpPost]
        public ActionResult TestInboundConn()
        {
            System.Console.WriteLine("Serving Inbound Connection");
            return Ok();
        }
    }
}