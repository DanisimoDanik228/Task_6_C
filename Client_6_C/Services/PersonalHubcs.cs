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
        public async Task History(string myConnectionId,string groupId)
        {
            await _bridge.HistoryToServer(Context.ConnectionId, groupId);
        }
        public async Task AvaliableGroups(string myConnectionId)
        {
            await _bridge.AvaliableGroupsToServer(myConnectionId);
        }
        public async Task JoinGroup(string myConnectionId, string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId); 
            await _bridge.JoinGroupToServer(Context.ConnectionId, groupId); 
        }

        public async Task LeaveGroup(string myConnectionId, string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
            await _bridge.LeaveGroupToServer(Context.ConnectionId, groupId);
        }

        public async Task Draw(Model shape, string myConnectionId, string groupId)
        {
            await _bridge.DrawToServer(shape, myConnectionId, groupId);
        }
    }
}
