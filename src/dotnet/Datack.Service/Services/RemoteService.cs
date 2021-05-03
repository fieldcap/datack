using Microsoft.AspNetCore.SignalR;

namespace Datack.Service.Services
{
    public interface IRemoteService
    {

    }

    public class RemoteService : IRemoteService
    {
        private readonly IHubContext<DatackHub> _hub;

        public RemoteService(IHubContext<DatackHub> hub)
        {
            _hub = hub;
        }
    }
}
