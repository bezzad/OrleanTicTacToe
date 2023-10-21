using GrainInterfaces.Models;
using Orleans.Concurrency;

namespace GrainInterfaces;

public interface IGameGrain : IGrainWithGuidKey
{
    [ResponseTimeout("00:00:05")] // 5s timeout
    Task<GameState> AddPlayerToGame(Guid player);
    
    [AlwaysInterleave]
    [ReadOnly]
    Task<GameState> GetState();

    [AlwaysInterleave]
    [ReadOnly]
    Task<List<GameMove>> GetMoves();
    Task<GameState> MakeMove(GameMove move);

    [AlwaysInterleave]
    [ReadOnly]
    Task<GameSummary> GetSummary(Guid player);
    
    Task SetName(string name);

    [AlwaysInterleave]
    [ReadOnly]
    Task<Guid[]> GetPlayers();
}