using Microsoft.AspNetCore.Mvc;
using TicTacToe.Grains;

namespace TicTacToe.Controllers;

public class GameController : BaseController
{
    public GameController(IGrainFactory grainFactory) : base(grainFactory) { }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var guid = GetPlayerId();
        var player = GrainFactory.GetGrain<IPlayerGrain>(guid);
        var gamesTask = player.GetGameSummaries();
        var availableTask = player.GetAvailableGames();
        await Task.WhenAll(gamesTask, availableTask);

        return Ok(new { currentGames = gamesTask.Result, availableGames = availableTask.Result });
    }

    [HttpPost("CreateGame")]
    public async Task<IActionResult> CreateGame()
    {
        var guid = GetPlayerId();
        var player = GrainFactory.GetGrain<IPlayerGrain>(guid);
        var gameIdTask = await player.CreateGame();
        return Ok(new { GameId = gameIdTask });
    }

    [HttpPatch("Join/{id}")]
    public async Task<IActionResult> Join(Guid id)
    {
        var playerId = GetPlayerId();
        var player = GrainFactory.GetGrain<IPlayerGrain>(playerId);
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
