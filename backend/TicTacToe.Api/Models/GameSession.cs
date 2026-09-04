namespace TicTacToe.Api.Models;

public class GameSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public char[,] Board { get; set; } = new char[3, 3];
    public char CurrentPlayer { get; set; } = 'X';
    public GameState State { get; set; } = GameState.InProgress;
    public List<Move> MoveHistory { get; set; } = new();
    public List<Position> WinningPositions { get; set; } = new();
    
    // Scoreboard
    public int XWins { get; set; } = 0;
    public int OWins { get; set; } = 0;
    public int Draws { get; set; } = 0;

    public GameSession()
    {
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Board[i, j] = ' ';
            }
        }
    }

    public void ResetGame()
    {
        Board = new char[3, 3];
        InitializeBoard();
        CurrentPlayer = 'X';
        State = GameState.InProgress;
        MoveHistory.Clear();
        WinningPositions.Clear();
    }
}
