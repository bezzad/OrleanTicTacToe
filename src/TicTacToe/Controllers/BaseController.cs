using Microsoft.AspNetCore.Mvc;

namespace TicTacToe.Controllers;

[ApiController]
[Route("[controller]")]
public class BaseController : ControllerBase
{
    private const string PlayerIdKey = "x-playerid";
    protected readonly IGrainFactory _grainFactory;

    public BaseController(IGrainFactory grainFactory) => _grainFactory = grainFactory;

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
}
