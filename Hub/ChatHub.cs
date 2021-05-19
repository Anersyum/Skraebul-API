using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Hubs
{
    public class ChatHub : Hub
    {
        private static Dictionary<string, string> Users = new Dictionary<string, string>();

        public async Task SendMessage(string username, string message)
        {
            await Clients.All.SendAsync("RecieveMessage", new {username = username, message = message});
        }

        // todo: generate random room ids for groups
        public override async Task OnConnectedAsync() 
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Test");

            string users = "";
            if (Users.ContainsKey("Test"))
            {
                users = Users["Test"];
            }

            var username = Context.GetHttpContext().Request.Query["username"].ToString();

            if (username == "")
            {
                Context.Abort();
            }

            Context.Items.Add("Username", username);

            users += username + "|";
            Users["Test"] = users;

            await Clients.Group("Test").SendAsync("Connected", users, username);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception) 
        {
            string username = Context.Items["Username"].ToString();
            Users["Test"] = Users["Test"].Replace(username + "|", "");
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Test");
            await Clients.Group("Test").SendAsync("Disconnected", Users["Test"], username);
            await base.OnDisconnectedAsync(exception);
        }
    }
}