using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TicTacToe.Hubs;
using TicTacToe.Models;

namespace TicTacToe.Controllers;

public class PlayerController : BaseController
{
    public PlayerController(ILogger<PlayerController> logger, 
        IGrainFactory grainFactory, IHubContext<GameHub> hubContext)
        : base(logger, grainFactory, hubContext) { }

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
