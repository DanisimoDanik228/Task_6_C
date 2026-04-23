using Domain;
using Microsoft.AspNetCore.SignalR;

namespace Task_6_C.Services
{
    public class PersonalHub : Hub
    {
        private readonly ServerService _bridge;

        public PersonalHub(IEnumerable<IHostedService> hostedServices)
        {
            _bridge = hostedServices.OfType<ServerService>().First();
        }

        public async Task Draw(Model shape, string connectionId)
        {
            await _bridge.DrawToServer(shape, connectionId);
        }

        public async Task History(string connectionId)
        {
            await _bridge.HistoryToServer(connectionId);
        }
    }
}
