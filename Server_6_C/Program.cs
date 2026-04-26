using Domain;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        private static readonly GroupMembers _groupMembers = new();
        private static readonly PermintationManager _permintationManager = new();

        public async Task SetName(string Name)
        {
            bool res= _permintationManager.SetName(Context.ConnectionId, Name);

            await Clients.Caller.SendAsync("SetName", res);

            if (res)
            {
                await Clients.Group("Home").SendAsync("AllUsers", _permintationManager.GetAllUsers());
            }
        }

        public async Task SetStatus(string[] connectionIds, string status)
        {
            lock(_permintationManager)
            {
                if (!_permintationManager.MayEditUser(Context.ConnectionId))
                {
                    return;
                }

                _permintationManager.SetStatus(connectionIds, status);
            }

            foreach (var connectionId in connectionIds)
            { 
                await Clients.Client(connectionId).SendAsync("SetStatus", status);
            }

            await Clients.Group("Home").SendAsync("AllUsers", _permintationManager.GetAllUsers());
        }

        public async Task RemoveUser()
        {
            //await OnDisconnectedAsync(null);
        }

        public async Task AddUser()
        {
            _permintationManager.AddUser(Context.ConnectionId);
            await Clients.Group("Home").SendAsync("AllUsers", _permintationManager.GetAllUsers());
        }
        public async Task DeleteGroup(string groupId)
        {
            if (!_permintationManager.MayEditGroup(Context.ConnectionId))
            {
                return;
            }

            var res = _groupMembers.RemoveGroup(groupId);
            lock (_groupHistory)
            {
                _groupHistory.Remove(groupId);
            }

            if (res)
            {
                await Clients.Group(groupId).SendAsync("DeleteMainGroup");
                await Clients.Group("Home").SendAsync("AllGroupIds", _groupMembers.GetAllGroups());
            }
        }

        public async Task CreateHomeGroup()
        {
            _groupMembers.AddGroup("Home");
        }

        public async Task CreateGroup(string groupId)
        {
            if (!_permintationManager.MayEditGroup(Context.ConnectionId))
            {
                return;
            }

            var res = _groupMembers.AddGroup(groupId);

            if (res)
            {
                await Clients.Group("Home").SendAsync("AllGroupIds", _groupMembers.GetAllGroups());
            }
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _groupMembers.RemoveUser(Context.ConnectionId);
            _permintationManager.RemoveUser(Context.ConnectionId);

            await Clients.Group("Home").SendAsync("AllUsers", _permintationManager.GetAllUsers());

            await base.OnDisconnectedAsync(exception);
        }
        public async Task GetAllGroupIds()
        {
            await Clients.Caller.SendAsync("AllGroupIds", _groupMembers.GetAllGroups());
        }
        public async Task GetAllUsers()
        {
            await Clients.Caller.SendAsync("AllUsers", _permintationManager.GetAllUsers());
        }

        public async Task JoinGroup(string groupId)
        {
            var c1 = _permintationManager.GetCountUsers();
            _groupMembers.AddUserToGroup(Context.ConnectionId, groupId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);

            if (c1 != _permintationManager.GetCountUsers())
            {
                await Clients.Group("Home").SendAsync("AllUsers", _permintationManager.GetAllUsers());
            }

            Console.WriteLine(_groupMembers);
            Console.WriteLine(_permintationManager);
        }

        public async Task LeaveGroup(string groupId)
        {
            _groupMembers.RemoveUser(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task SendData(Model data, string groupId)
        {
            if (!_permintationManager.MayEditPage(Context.ConnectionId))
            {
                return;
            }

            lock (_groupHistory)
            {
                if (!_groupHistory.ContainsKey(groupId))
                {
                    return;
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