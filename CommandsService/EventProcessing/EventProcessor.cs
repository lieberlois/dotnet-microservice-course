using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMapper _mapper;

        public EventProcessor(IServiceScopeFactory serviceScopeFactory, IMapper mapper)
        {
            this._serviceScopeFactory = serviceScopeFactory;
            this._mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            var eventType = GetEventType(message);

            switch (eventType)
            {
                case EventType.PlatformPublish:
                    AddPlatform(message);
                    break;
                default:
                    break;
            }
        }

        private EventType GetEventType(string eventString)
        {
            var eventType = JsonSerializer.Deserialize<GenericEventDto>(eventString);

            switch (eventType.Event)
            {
                case "Platform_Publish":
                    Console.WriteLine("--> Platform Publish event found");
                    return EventType.PlatformPublish;
                default:
                    return EventType.Unknown;
            }

        }

        private void AddPlatform(string platformPublishMessage)
        {
            using (var scope = this._serviceScopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<ICommandRepository>();

                var platformPublishDto = JsonSerializer.Deserialize<PlatformPublishDto>(platformPublishMessage);

                try
                {
                    var platform = this._mapper.Map<Platform>(platformPublishDto);

                    if (!repository.ExternalPlatformExists(platform.ExternalId))
                    {
                        repository.CreatePlatform(platform);
                        repository.SaveChanges();
                        Console.WriteLine($"--> Added Platform");
                    }
                    else
                    {
                        Console.WriteLine($"--> Platform already exists");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--> Could not add Platform to DB: {ex.Message}");
                }
            }
        }
    }

    enum EventType
    {
        PlatformPublish,
        Unknown
    }
}