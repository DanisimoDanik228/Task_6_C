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

        public async Task Draw(Model shape)
        {
            await _bridge.DrawToServer(shape);
        }

        public async Task History()
        {
            await _bridge.HistoryToServer(Context.ConnectionId);
        }
    }
}
