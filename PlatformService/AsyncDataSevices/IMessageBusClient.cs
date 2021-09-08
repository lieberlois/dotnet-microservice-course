using PlatformService.Dtos;

namespace PlatformService.AsyncDataServices.Http
{
    public interface IMessageBusClient
    {
        void PublishNewPlatform(PlatformPublishDto platformPublishDto);
    }
}