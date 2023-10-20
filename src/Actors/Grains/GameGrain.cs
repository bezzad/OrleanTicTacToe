using GrainInterfaces;
using GrainInterfaces.Models;
using Orleans.Concurrency;
using Orleans.Providers;

namespace Grains;

/// <summary>
/// Orleans grain implementation class GameGrain
/// </summary>
[Reentrant]
[StorageProvider(ProviderName = "OrleansStorage")]
public class GameGrain : Grain<Game>, IGameGrain
{
    // initialise 
    public override async Task OnActivateAsync(CancellationToken token)
    {
        await ReadStateAsync();

        // make sure newly formed game is in correct state 
        State.PlayerIds ??= new List<Guid>();
        State.Moves ??= new List<GameMove>();
        State.Board ??= new int[3, 3] { { -1, -1, -1 }, { -1, -1, -1 }, { -1, -1, -1 } };  // -1 is empty

        await base.OnActivateAsync(token);
    }

    // add a player into a game
    public async Task<GameState> AddPlayerToGame(Guid player)
    {
        // check if its ok to join this game
        if (State.GameState is GameState.Finished) throw new ApplicationException("Can't join game once its over");
        if (State.GameState is GameState.InPlay) throw new ApplicationException("Can't join game once its in play");

        // add player
        State.PlayerIds.Add(player);

        // check if the game is ready to play
        if (State.GameState is GameState.AwaitingPlayers && State.PlayerIds.Count is 2)
        {
            // a new game is starting
            State.GameState = GameState.InPlay;
            State.IndexNextPlayerToMove = Random.Shared.Next(0, 1);  // random as to who has the first move
        }

        await WriteStateAsync();

        // let user know if game is ready or not
        return State.GameState;
    }

    // make a move during the game
    public async Task<GameState> MakeMove(GameMove move)
    {
        try
        {
            // check if its a legal move to make
            if (State.GameState is not GameState.InPlay) throw new ApplicationException("This game is not in play");

            if (State.PlayerIds.IndexOf(move.PlayerId) < 0) throw new ArgumentException("No such playerid for this game", "move");
            if (move.PlayerId != State.PlayerIds[State.IndexNextPlayerToMove]) throw new ArgumentException("The wrong player tried to make a move", "move");

            if (move.X < 0 || move.X > 2 || move.Y < 0 || move.Y > 2) throw new ArgumentException("Bad co-ordinates for a move", "move");
            if (State.Board[move.X, move.Y] != -1) throw new ArgumentException("That square is not empty", "move");

            // record move
            State.Moves.Add(move);
            State.Board[move.X, move.Y] = State.IndexNextPlayerToMove;

            // check for a winning move
            var win = false;
            for (var i = 0; i < 3 && !win; i++)
            {
                win = IsWinningLine(State.Board[i, 0], State.Board[i, 1], State.Board[i, 2]);
            }

            if (!win)
            {
                for (var i = 0; i < 3 && !win; i++)
                {
                    win = IsWinningLine(State.Board[0, i], State.Board[1, i], State.Board[2, i]);
                }
            }

            if (!win)
            {
                win = IsWinningLine(State.Board[0, 0], State.Board[1, 1], State.Board[2, 2]);
            }

            if (!win)
            {
                win = IsWinningLine(State.Board[0, 2], State.Board[1, 1], State.Board[2, 0]);
            }

            // check for draw
            var draw = false;
            if (State.Moves.Count is 9)
            {
                draw = true;  // we could try to look for stalemate earlier, if we wanted 
            }

            // handle end of game
            if (win || draw)
            {
                // game over
                State.GameState = GameState.Finished;
                if (win)
                {
                    State.WinnerId = State.PlayerIds[State.IndexNextPlayerToMove];
                    State.LoserId = State.PlayerIds[(State.IndexNextPlayerToMove + 1) % 2];
                }

                // collect tasks up, so we await both notifications at the same time
                var promises = new List<Task>();
                // inform this player of outcome
                var playerGrain = GrainFactory.GetGrain<IPlayerGrain>(State.PlayerIds[State.IndexNextPlayerToMove]);
                promises.Add(playerGrain.LeaveGame(this.GetPrimaryKey(), win ? GameOutcome.Win : GameOutcome.Draw));

                // inform other player of outcome
                playerGrain = GrainFactory.GetGrain<IPlayerGrain>(State.PlayerIds[(State.IndexNextPlayerToMove + 1) % 2]);
                promises.Add(playerGrain.LeaveGame(this.GetPrimaryKey(), win ? GameOutcome.Lose : GameOutcome.Draw));
                await Task.WhenAll(promises);
                return State.GameState;
            }

            // if game hasnt ended, prepare for next players move
            State.IndexNextPlayerToMove = (State.IndexNextPlayerToMove + 1) % 2;

            return State.GameState;
        }
        finally
        {
            await WriteStateAsync();
        }
    }

    private static bool IsWinningLine(int i, int j, int k) => (i, j, k) switch
    {
        (0, 0, 0) => true,
        (1, 1, 1) => true,
        _ => false
    };


    public Task<GameState> GetState() => Task.FromResult(State.GameState);

    public Task<List<GameMove>> GetMoves() => Task.FromResult(State.Moves);

    public async Task<GameSummary> GetSummary(Guid player)
    {
        var promises = new List<Task<string>>();
        foreach (var p in State.PlayerIds.Where(p => p != player))
        {
            promises.Add(GrainFactory.GetGrain<IPlayerGrain>(p).GetUsername());
        }

        await Task.WhenAll(promises);

        return new GameSummary
        {
            NumMoves = State.Moves.Count,
            State = State.GameState,
            YourMove = State.GameState is GameState.InPlay && player == State.PlayerIds[State.IndexNextPlayerToMove],
            NumPlayers = State.PlayerIds.Count,
            GameId = this.GetPrimaryKey(),
            Usernames = promises.Select(x => x.Result).ToArray(),
            Name = State.Name,
            GameStarter = State.PlayerIds.FirstOrDefault() == player
        };
    }

    public async Task SetName(string name)
    {
        State.Name = name;
        await WriteStateAsync();
    }

    public Task<Guid[]> GetPlayers()
    {
        return Task.FromResult(State.PlayerIds.ToArray());
    }
}
