using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TicTacToe.Hubs;
using GrainInterfaces.Models;

namespace TicTacToe.Controllers;

public class PlayerController : BaseController
{
    public PlayerController(ILogger<PlayerController> logger,
        IClusterClient client, IHubContext<GameHub> hubContext)
        : base(logger, client, hubContext) { }

    [HttpGet("Info")]
    public async Task<ActionResult<User>> GetInfo()
    {
        var player = GetPlayerGrain();
        var user = await player.GetUser();

        return Ok(user);
    }

    [HttpPost("SetUsername/{username}")]
    public async Task<ActionResult<User>> SetUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest($"{nameof(username)} is null or empty!");
        }

        var player = GetPlayerGrain();
        await player.SetUsername(username);
        var user = await player.GetUser();
        return Ok(user);
    }
}
