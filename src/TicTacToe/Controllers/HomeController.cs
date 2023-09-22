using Microsoft.AspNetCore.Mvc;
using TicTacToe.Grains;

namespace TicTacToe.Controllers;

public class ViewModel 
{
    public string GameId { get; set; } = null!;
}

public class HomeController : ControllerBase
{
    private readonly IGrainFactory _grainFactory;
    public HomeController(IGrainFactory grainFactory) => _grainFactory = grainFactory;

    public IActionResult Index(Guid? id)
    {
        var vm = new ViewModel
        {
            GameId = id.HasValue ? id.Value.ToString() : ""
        };

        return Ok(vm);
    }

    public async Task<IActionResult> Join(Guid id)
    {
        var guid = this.GetGuid();
        var player = _grainFactory.GetGrain<IPlayerGrain>(guid);
        await player.JoinGame(id);
        return Ok(id);
    }
}
