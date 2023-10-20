using GrainInterfaces.Models;
using Orleans;

namespace GrainInterfaces;

public interface IPlayerGrain : IGrainWithGuidKey
{
    // get a list of all active games

    Task<PairingSummary[]> GetAvailableGames();

    Task<List<GameSummary>> GetGameSummaries();

    // create a new game and join it
    Task<Guid> CreateGame();

    // join an existing game
    Task<GameState> JoinGame(Guid gameId);

    Task LeaveGame(Guid gameId, GameOutcome outcome);

    Task SetUsername(string username);

    Task SetConnectionId(string connectionId);

    Task<string> GetUsername();

    Task<User> GetUser();
}