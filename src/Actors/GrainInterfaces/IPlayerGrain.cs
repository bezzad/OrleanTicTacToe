using GrainInterfaces.Models;
using Orleans.Concurrency;

namespace GrainInterfaces;

public interface IPlayerGrain : IGrainWithGuidKey
{
    // get a list of all active games

    [AlwaysInterleave]
    [ReadOnly]
    Task<PairingSummary[]> GetAvailableGames();

    [AlwaysInterleave]
    [ReadOnly]
    Task<List<GameSummary>> GetGameSummaries();

    // create a new game and join it
    Task<Guid> CreateGame();

    // join an existing game
    Task<GameState> JoinGame(Guid gameId);

    [ResponseTimeout("00:00:03")]
    Task LeaveGame(Guid gameId, GameOutcome outcome);

    Task SetUsername(string username);

    Task SetConnectionId(string connectionId);

    Task<string> GetUsername();

    [AlwaysInterleave]
    [ReadOnly]
    Task<User> GetUser();
}
