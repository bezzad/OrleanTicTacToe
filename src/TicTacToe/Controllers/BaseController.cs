using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TicTacToe.Grains;
using TicTacToe.Hubs;

namespace TicTacToe.Controllers;

[ApiController]
[Route("[controller]")]
public class BaseController : ControllerBase
{
    private const string PlayerIdKey = "x-player-id";
    protected readonly IGrainFactory GrainFactory;
    protected readonly IHubContext<GameHub, IGameClient> HubContext;
    protected readonly ILogger Logger;

    public BaseController(ILogger logger, IGrainFactory grainFactory, IHubContext<GameHub, IGameClient> hubContext)
    {
        GrainFactory = grainFactory;
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
        return GrainFactory.GetGrain<IPlayerGrain>(playerId);
    }

    protected IPairingGrain GetPairingGrain()
    {
        return GrainFactory.GetGrain<IPairingGrain>(0);
    }

    protected IGameGrain GetGameGrain(Guid gameId)
    {
        return GrainFactory.GetGrain<IGameGrain>(gameId);
    }
}
