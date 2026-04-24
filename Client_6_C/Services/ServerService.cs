using Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Client;
using System.Drawing;
using System.Text.RegularExpressions;
using Task_6_C.Controllers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Task_6_C.Services
{
    public class ServerService : BackgroundService
    {
        private static string _serverAdress = "http://localhost:5123/draw";

        private readonly IHubContext<PersonalHub> _internalHub;
        private readonly HubConnection _consoleConnection;

        public ServerService(IHubContext<PersonalHub> internalHub)
        {
            _internalHub = internalHub;

            _consoleConnection = new HubConnectionBuilder()
                .WithUrl(_serverAdress)
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task AvaliableGroupsToServer(string myConnectionId)
        {
            await _consoleConnection.InvokeAsync("AvaliableGroupsToServer", myConnectionId);
        }

        public async Task JoinGroupToServer(string myConnectionId, string groupId) 
        { 
            await _consoleConnection.InvokeAsync("JoinGroupToServer", groupId);
        }
        public async Task LeaveGroupToServer(string myConnectionId, string groupId)
        {
            await _consoleConnection.InvokeAsync("LeaveGroupToServer", groupId);
        }

        public async Task DrawToServer(Model data, string myConnectionId, string groupId)
        {
            await _consoleConnection.InvokeAsync("DrawToServer", data, myConnectionId, groupId);
        }

        public async Task HistoryToServer(string myConnectionId, string groupId)
        {
            await _consoleConnection.InvokeAsync("HistoryToServer", myConnectionId, groupId);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consoleConnection.On<List<string>, string>("AvaliableGroupsToClient", async (data, myConnectionId) =>
            {
                await _internalHub.Clients.Client(myConnectionId).SendAsync("GroupName", data, myConnectionId);
            });

            _consoleConnection.On<Model,string, string>("DrawToClient", async (data, connectionId, groupId) =>
            {
                await _internalHub.Clients.Group(groupId).SendAsync("Update", data, connectionId, groupId);

                await _internalHub.Clients.Group("Home").SendAsync("Update", data, connectionId, groupId);
            });

            _consoleConnection.On<List<Model>,string, string>("HistoryToClient", async (data, myConnectionId, groupId) =>
            {
                // MUST BE ONLY ONCE SEND
                foreach (var item in data)
                {
                    await _internalHub.Clients.Client(myConnectionId).SendAsync("Update", item, myConnectionId, groupId);
                }
            });

            await _consoleConnection.StartAsync(cancellationToken);
        }
    }
}
