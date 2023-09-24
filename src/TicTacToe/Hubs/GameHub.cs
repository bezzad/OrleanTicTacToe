using Microsoft.AspNetCore.SignalR;
using TicTacToe.Models;

namespace TicTacToe.Hubs;

public interface IGameClient
{
    public Task BroadcastMessage(string name, string message);
    public Task OnNewGame(PairingSummary summary);
}

public class GameHub : Hub<IGameClient>
{
    public async Task BroadcastMessage(string name, string message)
    {
        await Clients.All.BroadcastMessage(name, message);
    }

    public async Task OnNewGame(PairingSummary summary)
    {
        await Clients.Others.OnNewGame(summary);
    }

    public async Task CallerMessage(string user, string message)
    {
        await Clients.Caller.BroadcastMessage(user, message);
    }

    public override async Task OnConnectedAsync()
    {
        //var userId = Context.User!.Identity!.Name!;
        var connectionId = Context.ConnectionId;
        await base.OnConnectedAsync();
    }
}
