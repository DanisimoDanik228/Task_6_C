using Domain;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Drawing;

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
        private static readonly List<Model> _history = new List<Model>();

        public async Task DrawToServer(Model data)
        {
            lock (_history)
            {
                _history.Add(data);
            }

            await Clients.All.SendAsync("DrawToClient", data);
        }

        public async Task HistoryToServer(string connectionId)
        {
            List<Model> data;

            lock (_history)
            {
                data = _history.ToList();
            }

            await Clients.Caller.SendAsync("HistoryToClient", data, connectionId);
        }
    }
}