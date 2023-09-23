namespace TicTacToe.Models
{
    [GenerateSerializer]
    public class User
    {
        //[PrimaryKey]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int Wins { get; set; }
        public int Loses { get; set; }
        public int GamesStarted { get; set; }
    }
}
