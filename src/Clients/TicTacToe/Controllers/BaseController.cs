using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using GrainInterfaces;
using TicTacToe.Hubs;

namespace TicTacToe.Controllers;

[ApiController]
[Route("[controller]")]
public class BaseController : ControllerBase
{
    private const string PlayerIdKey = "x-player-id";
    protected readonly IClusterClient Client;
    protected readonly IHubContext<GameHub> HubContext;
    protected readonly ILogger Logger;

    public BaseController(ILogger logger, IClusterClient client, IHubContext<GameHub> hubContext)
    {
        Client = client;
        HubContext = hubContext;
        Logger = logger;
    }

    protected Guid GetPlayerId()
    {
        if (Request.Headers.TryGetValue(PlayerIdKey, out var id) &&
            Guid.TryParse(id.FirstOrDefault(), out var playerId))
        {
            return playerId;
        }

        playerId = Guid.NewGuid();
        Response.Headers.Append(PlayerIdKey, playerId.ToString());
        return playerId;
    }

    protected IPlayerGrain GetPlayerGrain()
    {
        var playerId = GetPlayerId();
        return Client.GetGrain<IPlayerGrain>(playerId);
    }

    protected IPairingGrain GetPairingGrain()
    { 
        return Client.GetGrain<IPairingGrain>(0);
    }

    protected IGameGrain GetGameGrain(Guid gameId)
    {
        return Client.GetGrain<IGameGrain>(gameId);
    }
}
