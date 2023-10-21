using GrainInterfaces;
using GrainInterfaces.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TicTacToe.Hubs;

namespace TicTacToe.Controllers;

public class AdminController : BaseController
{
    public AdminController(ILogger<AdminController> logger, IClusterClient client, IHubContext<GameHub> hubContext)
        : base(logger, client, hubContext) { }

    [HttpDelete("DeactivePlayer")]
    public async Task<IActionResult> DeactivePlayer(Guid player)
    {
        var grain = Client.GetGrain<IPlayerGrain>(player);
        var grainReferenceAsInterface = grain.AsReference<IGrainDeactivateExtension>();

        await grainReferenceAsInterface.Deactivate("Admin blocked it!");

        return Ok();
    }
}
