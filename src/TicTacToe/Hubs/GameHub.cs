using Microsoft.AspNetCore.SignalR;
using TicTacToe.Grains;
using TicTacToe.Models;

namespace TicTacToe.Hubs;

public class GameHub : Hub
{
    private readonly IGrainFactory _grainFactory;

    public GameHub(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task BroadcastMessage(string message)
    {
        await Clients.All.SendAsync(nameof(BroadcastMessage), message);
    }

    public async Task OnNewGame(PairingSummary summary)
    {
        await Clients.Others.SendAsync(nameof(OnNewGame), summary);
    }

    public void OnUpdateBoard(string clientId, GameSummary summery)
    {
        Clients.Client(clientId).SendAsync(nameof(OnUpdateBoard), summery);
    }

    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var httpContext = Context.GetHttpContext();
        if (httpContext != null)
        {
            var playerToken = httpContext.Request.Cookies["x-player-id"];
            if (Guid.TryParse(playerToken, out var playerId) && !string.IsNullOrWhiteSpace(connectionId))
            {
                var player = _grainFactory.GetGrain<IPlayerGrain>(playerId);
                await player.SetConnectionId(connectionId);
            }
        }

        await base.OnConnectedAsync();
    }
}
