using Domain;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Task_6_C.Services
{
    public class PersonalHub : Hub
    {
        private readonly ServerService _bridge;

        public PersonalHub(IEnumerable<IHostedService> hostedServices)
        {
            _bridge = hostedServices.OfType<ServerService>().First();
        }
        public async Task AvaliableGroups(string myConnectionId)
        {
            await _bridge.AvaliableGroupsToServer(myConnectionId);
        }
        public async Task JoinGroup(string myConnectionId, string groupId)
        {
            await Groups.AddToGroupAsync(myConnectionId, groupId); 
            await _bridge.JoinGroupToServer(myConnectionId, groupId); 
            await _bridge.HistoryToServer(myConnectionId, groupId); 
        }

        public async Task Draw(Model shape, string myConnectionId, string groupId)
        {
            await _bridge.DrawToServer(shape, myConnectionId, groupId);
        }
    }
}
