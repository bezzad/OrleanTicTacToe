using TicTacToe.Models;

namespace TicTacToe.Grains;

public interface IGameGrain : IGrainWithGuidKey
{
    Task<GameState> AddPlayerToGame(Guid player);
    Task<GameState> GetState();
    Task<List<GameMove>> GetMoves();
    Task<GameState> MakeMove(GameMove move);
    Task<GameSummary> GetSummary(Guid player);
    Task SetName(string name);
}