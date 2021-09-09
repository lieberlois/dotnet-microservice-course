using AutoMapper;
using CommandsService.Dtos;
using CommandsService.Models;

namespace CommandsService.Profiles
{
    public class CommandsProfile : Profile
    {
        public CommandsProfile()
        {
            // Source -> Target
            CreateMap<Platform, PlatformReadDto>();
            CreateMap<Command, CommandReadDto>();
            CreateMap<CommandCreateDto, Command>();

            CreateMap<PlatformPublishDto, Platform>()
                .ForMember(
                    (dest) => dest.ExternalId,
                    (opts) => opts.MapFrom((src) => src.Id)
                );
        }
    }
}