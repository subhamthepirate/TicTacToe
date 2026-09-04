namespace TicTacToe.Api.Models;

public class Move
{
    public int MoveNumber { get; set; }
    public char Player { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public DateTime Timestamp { get; set; }
}
