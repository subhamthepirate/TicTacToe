namespace TicTacToe.Api.Models;

public class GameResponse
{
    public string GameId { get; set; } = string.Empty;
    public string[][] Board { get; set; } = new string[3][];
    public string CurrentPlayer { get; set; } = "X";
    public GameState State { get; set; }
    public List<Move> MoveHistory { get; set; } = new();
    public List<Position> WinningPositions { get; set; } = new();
    public int XWins { get; set; }
    public int OWins { get; set; }
    public int Draws { get; set; }
    public string Message { get; set; } = string.Empty;
}
