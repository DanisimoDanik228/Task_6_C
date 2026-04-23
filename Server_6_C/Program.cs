using Domain;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Server_6_C
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllPolicy", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            var app = builder.Build();

            app.MapHub<DrawHub>("/draw");

            app.Run("http://localhost:5123");
        }
    }

    public class DrawHub : Hub
    {
        private static readonly Dictionary<string, List<Model>> _groupHistory = new();

        public async Task AvaliableGroupsToServer(string connectionId)
        {
            await Clients.Caller.SendAsync("AvaliableGroupsToClient", _groupHistory, connectionId);
        }

        public async Task JoinGroupToServer(string connectionId, string groupId)
        {
            await Groups.AddToGroupAsync(connectionId, groupId);
        }

        public async Task DrawToServer(Model data, string connectionId, string groupId)
        {
            lock (_groupHistory)
            {
                if (!_groupHistory.ContainsKey(groupId))
                { 
                    _groupHistory[groupId] = new();
                }

                if (!data.isPreview)
                {
                    _groupHistory[groupId].Add(data);
                }

            }

            await Clients.All.SendAsync("DrawToClient", data, connectionId, groupId);
        }

        public async Task HistoryToServer(string connectionId, string groupId)
        {
            List<Model> data;

            lock (_groupHistory)
            {
                if (!_groupHistory.ContainsKey(groupId))
                {
                    _groupHistory[groupId] = new();
                }

                data = _groupHistory[groupId].ToList();
            }

            await Clients.Caller.SendAsync("HistoryToClient", data, connectionId, groupId);
        }
    }
}