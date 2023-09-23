using Microsoft.AspNetCore.SignalR;

namespace TicTacToe.Hubs;

public class GameHub : Hub
{
    public async Task UpdateAvailableGames(string user, string message)
    {
        await Clients.All.SendAsync("broadcastMessage", user, message);
    }
}
