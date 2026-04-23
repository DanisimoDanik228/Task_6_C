using Domain;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Client;
using System.Drawing;
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
                .Build();
        }

        public async Task DrawToServer(Model data)
        {
            await _consoleConnection.InvokeAsync("DrawToServer", data);
        }

        public async Task HistoryToServer(string connectionId)
        {
            await _consoleConnection.InvokeAsync("HistoryToServer", connectionId);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _consoleConnection.On<Model>("DrawToClient", async (Model data) =>
            {
                await _internalHub.Clients.All.SendAsync("Update", "Draw    " + data.ToString());
            });

            _consoleConnection.On<List<Model>,string>("HistoryToClient", async (data, connectionId) =>
            {
                // MUST BE ONLY ONCE SEND
                foreach (var item in data)
                {
                    await _internalHub.Clients.Client(connectionId).SendAsync("Update", "History " + item);
                }
            });

            await _consoleConnection.StartAsync(cancellationToken);
        }
    }
}
