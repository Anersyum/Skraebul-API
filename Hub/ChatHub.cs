using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string username, string message)
        {
            await Clients.All.SendAsync("RecieveMessage", new {username = username, message = message});
        }
    }
}