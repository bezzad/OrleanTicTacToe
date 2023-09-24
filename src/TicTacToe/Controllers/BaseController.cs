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
    protected readonly IHubContext<GameHub> HubContext;
    protected readonly ILogger Logger;

    public BaseController(ILogger logger, IGrainFactory grainFactory, IHubContext<GameHub> hubContext)
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

    protected IPlayerGrain GetPlayer()
    {
        var playerId = GetPlayerId();
        return GrainFactory.GetGrain<IPlayerGrain>(playerId);
    }
}
