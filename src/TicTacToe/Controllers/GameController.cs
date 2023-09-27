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
        var gameId = await player.CreateGame();
        var gameGrain = GetGameGrain(gameId);
        var gameSummery = await gameGrain.GetSummary(player.GetPrimaryKey());

        var pairing = GetPairingGrain();
        var pairingSummary = await pairing.GetGame(gameId);

        // Notify connected SignalR clients with some data:
        await HubContext.Clients.All.SendAsync(nameof(GameHub.OnNewGame), pairingSummary).ConfigureAwait(false);

        return Ok(gameSummery);
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
        var game = GetGameGrain(id);
        var moves = await game.GetMoves();
        var summary = await game.GetSummary(GetPlayerId());
        return Ok(new { moves, summary });
    }

    [HttpPost("Move/{id}")]
    public async Task<IActionResult> MakeMove(Guid id, int x, int y)
    {
        // move
        var currentPlayer = GetPlayerId();
        var game = GetGameGrain(id);
        var move = new GameMove { PlayerId = currentPlayer, X = x, Y = y };
        var state = await game.MakeMove(move);

        // create last state response for players
        var moves = await game.GetMoves();
        var playerGameSummary = await game.GetSummary(currentPlayer);

        // notify next player to move
        var players = await game.GetPlayers();
        var nextPlayerId = players.Where(p => p != currentPlayer).FirstOrDefault();
        var nextPlayer = GrainFactory.GetGrain<IPlayerGrain>(nextPlayerId);
        var nextPlayerUser = await nextPlayer.GetUser();
        var nextPlayerGameSummary = await game.GetSummary(nextPlayerId);
        await HubContext.Clients.Client(nextPlayerUser.ClientConnectionId)
            .SendAsync(nameof(GameHub.OnUpdateBoard), new { moves, summary = nextPlayerGameSummary })
            .ConfigureAwait(false);

        return Ok(new { moves, summary = playerGameSummary });
    }

    [HttpGet("State/{id}")]
    public async Task<IActionResult> QueryGame(Guid id)
    {
        var game = GrainFactory.GetGrain<IGameGrain>(id);
        var state = await game.GetState();
        return Ok(state);

    }
}
