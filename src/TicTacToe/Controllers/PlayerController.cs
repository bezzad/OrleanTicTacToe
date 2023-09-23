using Microsoft.AspNetCore.Mvc;
using TicTacToe.Grains;
using TicTacToe.Models;

namespace TicTacToe.Controllers;

public class PlayerController : BaseController
{
    public PlayerController(IGrainFactory grainFactory) : base(grainFactory) { }

    [HttpGet("Info")]
    public async Task<ActionResult<User>> GetInfo()
    {
        var guid = GetPlayerId();
        var player = GrainFactory.GetGrain<IPlayerGrain>(guid);
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

        var guid = GetPlayerId();
        var player = GrainFactory.GetGrain<IPlayerGrain>(guid);
        await player.SetUsername(username);
        var user = await player.GetUser();
        return Ok(user);
    }
}
