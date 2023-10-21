using GrainInterfaces;
using GrainInterfaces.Models;
using Orleans.Providers;

namespace Grains;

[StorageProvider(ProviderName = "OrleansStorage")]
public class PlayerGrain : Grain<User>, IPlayerGrain
{
    public override async Task OnActivateAsync(CancellationToken token)
    {
        await ReadStateAsync();
        var playerId = this.GetPrimaryKey();  // our player id
        State.Id = playerId;
        State.ActiveGames ??= new List<Guid>();
        State.PastGames ??= new List<Guid>();

        await base.OnActivateAsync(token);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        await Console.Out.WriteLineAsync(reason.Description);
        await base.OnDeactivateAsync(reason, cancellationToken);
    }

    public async Task<PairingSummary[]> GetAvailableGames()
    {
        var grain = GrainFactory.GetGrain<IPairingGrain>(0);
        var games = await grain.GetGames();
        var availableGames = games.Where(x => !State.ActiveGames.Contains(x.GameId)).ToArray();
        return availableGames;
    }

    // create a new game, and add oursleves to that game
    public async Task<Guid> CreateGame()
    {
        State.GamesStarted++;

        var gameId = Guid.NewGuid();
        var gameGrain = GrainFactory.GetGrain<IGameGrain>(gameId);  // create new game

        // add ourselves to the game
        var playerId = this.GetPrimaryKey();  // our player id
        await gameGrain.AddPlayerToGame(playerId);
        State.ActiveGames.Add(gameId);
        var name = $"{State.Username}'s {AddOrdinalSuffix(State.GamesStarted.ToString())} game";
        await gameGrain.SetName(name);

        var pairingGrain = GrainFactory.GetGrain<IPairingGrain>(0);
        await pairingGrain.AddGame(gameId, name, playerId);
        await WriteStateAsync();

        return gameId;
    }

    // join a game that is awaiting players
    public async Task<GameState> JoinGame(Guid gameId)
    {
        var gameGrain = GrainFactory.GetGrain<IGameGrain>(gameId);
        var state = await gameGrain.AddPlayerToGame(this.GetPrimaryKey());
        State.ActiveGames.Add(gameId);
        var pairingGrain = GrainFactory.GetGrain<IPairingGrain>(0);
        await pairingGrain.RemoveGame(gameId);
        await WriteStateAsync();

        return state;
    }

    // leave game when it is over
    public async Task LeaveGame(Guid gameId, GameOutcome outcome)
    {
        // manage game list
        State.ActiveGames.Remove(gameId);
        State.PastGames.Add(gameId);
        await WriteStateAsync();

        // manage running total
        _ = outcome switch
        {
            GameOutcome.Win => State.Wins++,
            GameOutcome.Lose => State.Loses++,
            _ => 0
        };
    }

    public async Task<List<GameSummary>> GetGameSummaries()
    {
        var tasks = new List<Task<GameSummary>>();
        foreach (var gameId in State.ActiveGames)
        {
            var game = GrainFactory.GetGrain<IGameGrain>(gameId);
            tasks.Add(game.GetSummary(this.GetPrimaryKey()));
        }

        await Task.WhenAll(tasks);
        return tasks.Select(x => x.Result).ToList();
    }

    public async Task SetUsername(string name)
    {
        State.Username = name;
        await WriteStateAsync();
    }

    public async Task SetConnectionId(string connectionId)
    {
        State.ClientConnectionId = connectionId;
        await WriteStateAsync();
    }

    public Task<string> GetUsername() => Task.FromResult(State.Username);
    public Task<User> GetUser() => Task.FromResult(State);

    private static string AddOrdinalSuffix(string number)
    {
        var n = int.Parse(number);
        var nMod100 = n % 100;

        return nMod100 switch
        {
            >= 11 and <= 13 => string.Concat(number, "th"),
            _ => (n % 10) switch
            {
                1 => string.Concat(number, "st"),
                2 => string.Concat(number, "nd"),
                3 => string.Concat(number, "rd"),
                _ => string.Concat(number, "th"),
            }
        };
    }
}
