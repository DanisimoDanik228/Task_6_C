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
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins("http://localhost:5111") 
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); 
                });
            });

            var app = builder.Build();

            app.UseCors("CorsPolicy");
            app.MapHub<DrawHub>("/hub");

            app.Run("http://localhost:5123");
        }
    }

    public class DrawHub : Hub
    {
        private static readonly Dictionary<string, List<Model>> _groupHistory = new();

        public async Task getAllGroupIds()
        {
            await Clients.Caller.SendAsync("AllGroupIds", _groupHistory.Keys.ToList());
        }

        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task SendData(Model data, string groupId)
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

            await Clients.Group("Home").SendAsync("UpdateHome", data, Context.ConnectionId, groupId);
            await Clients.Group(groupId).SendAsync("UpdateMain", data, Context.ConnectionId, groupId);
        }

        public async Task GetHistory(string groupId)
        {
            List<Model> clasterData;

            lock (_groupHistory)
            {
                if (!_groupHistory.ContainsKey(groupId))
                {
                    _groupHistory[groupId] = new();
                }

                clasterData = _groupHistory[groupId].ToList();
            }

            await Clients.Caller.SendAsync("ReceiveHistory", clasterData, groupId);
        }
    }
}