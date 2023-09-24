using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TicTacToe.Grains;
using TicTacToe.Hubs;
using TicTacToe.Models;

namespace TicTacToe.Controllers;

public class GameController : BaseController
{
    public GameController(ILogger<GameController> logger, IGrainFactory grainFactory, IHubContext<GameHub> hubContext)
        : base(logger, grainFactory, hubContext) { }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var player = GetPlayerGrain();
        var gamesTask = player.GetGameSummaries();
        var availableTask = player.GetAvailableGames();
        await Task.WhenAll(gamesTask, availableTask);

        return Ok(new { currentGames = gamesTask.Result, availableGames = availableTask.Result });
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllGames()
    {
        var grain = GetPairingGrain();
        var games = await grain.GetGames();

        return Ok(games);
    }

    [HttpPost("CreateGame")]
    public async Task<IActionResult> CreateGame()
    {
        var player = GetPlayerGrain();
        var gameIdTask = await player.CreateGame();

        // Notify connected SignalR clients with some data:
        await HubContext.Clients.All.SendAsync("broadcastMessage", "the weatherman", $" HIiii").ConfigureAwait(false);

        return Ok(new { GameId = gameIdTask });
    }

    [HttpPost("Join/{id}")]
    public async Task<IActionResult> Join(Guid id)
    {
        var player = GetPlayerGrain();
        var state = await player.JoinGame(id);
        return Ok(new { GameState = state });
    }

    [HttpGet("Moves/{id}")]
    public async Task<IActionResult> GetMoves(Guid id)
    {
        var game = GrainFactory.GetGrain<IGameGrain>(id);
        var moves = await game.GetMoves();
        var summary = await game.GetSummary(GetPlayerId());
        return Ok(new { moves, summary });
    }

    [HttpPost("Move/{id}")]
    public async Task<IActionResult> MakeMove(Guid id, int x, int y)
    {
        var game = GrainFactory.GetGrain<IGameGrain>(id);
        var move = new GameMove { PlayerId = GetPlayerId(), X = x, Y = y };
        var state = await game.MakeMove(move);
        return Ok(state);
    }

    [HttpGet("State/{id}")]
    public async Task<IActionResult> QueryGame(Guid id)
    {
        var game = GrainFactory.GetGrain<IGameGrain>(id);
        var state = await game.GetState();
        return Ok(state);

    }
}
