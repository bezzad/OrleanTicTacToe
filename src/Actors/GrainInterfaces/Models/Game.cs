namespace GrainInterfaces.Models;

[GenerateSerializer]
public class Game
{
    [Id(0)]
    public Guid Id { get; set; }

    /// <summary>
    /// List of players in the current game
    /// for simplicity, player 0 always plays an "O" and player 1 plays an "X"
    ///  who starts a game is a random call once a game is started, and is set via indexNextPlayerToMove
    /// </summary>
    [Id(1)]
    public List<Guid> PlayerIds { get; set; }
    
    [Id(2)]
    public string Name { get; set; }

    [Id(3)]
    public int IndexNextPlayerToMove { get; set; } = -1; // safety default - is set when game begins to 0 or 1

    [Id(4)]
    public GameState GameState { get; set; } = GameState.AwaitingPlayers;

    [Id(5)]
    public Guid WinnerId { get; set; }
    
    [Id(6)]
    public Guid LoserId { get; set; }
    
    /// <summary>
    /// We record a game in terms of each of the moves, so we could reconstruct the sequence of play
    /// during an active game, we also use a 2D array to represent the board, to make it
    ///  easier to check for legal moves, wining lines, etc. 
    ///  -1 represents an empty square, 0 & 1 the player's index 
    /// </summary>
    [Id(7)]
    public List<GameMove> Moves { get; set; }
    
    [Id(8)]
    public int[,] Board { get; set; }
}
