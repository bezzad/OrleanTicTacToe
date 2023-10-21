using GrainInterfaces.Models;

namespace GrainInterfaces;

public interface IGameGrain : IGrainWithGuidKey
{
    [ResponseTimeout("00:00:05")] // 5s timeout
    Task<GameState> AddPlayerToGame(Guid player);
    Task<GameState> GetState();
    Task<List<GameMove>> GetMoves();
    Task<GameState> MakeMove(GameMove move);
    Task<GameSummary> GetSummary(Guid player);
    Task SetName(string name);
    Task<Guid[]> GetPlayers();
}