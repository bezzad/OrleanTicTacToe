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
        var player = _grainFactory.GetGrain<IPlayerGrain>(guid);
        var user = await player.GetUser();

        return Ok(user);
    }

    [HttpPost("SetUsername")]
    public async Task<IActionResult> SetUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest($"{nameof(username)} is null or empty!");
        }

        var player = _grainFactory.GetGrain<IPlayerGrain>(GetPlayerId());
        await player.SetUsername(username);
        return Ok();
    }
}
