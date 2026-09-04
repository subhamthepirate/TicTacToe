namespace TicTacToe.Api.Services;

using TicTacToe.Api.Models;

public interface IGameService
{
    GameSession CreateNewGame();
    GameResponse MakeMove(string gameId, int row, int column);
    GameResponse UndoLastMove(string gameId);
    GameResponse ResetGame(string gameId);
    GameResponse ResetScoreboard(string gameId);
    GameResponse GetGameState(string gameId);
}

public class GameService : IGameService
{
    private readonly Dictionary<string, GameSession> _games = new();

    public GameSession CreateNewGame()
    {
        var game = new GameSession();
        _games[game.Id] = game;
        return game;
    }

    public GameResponse MakeMove(string gameId, int row, int column)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return new GameResponse { Message = "Game not found." };
        }

        // Validate move
        if (row < 0 || row > 2 || column < 0 || column > 2)
        {
            return new GameResponse { Message = "Invalid row or column. Must be 0-2." };
        }

        if (game.Board[row, column] != ' ')
        {
            return new GameResponse { Message = "Cell already occupied." };
        }

        if (game.State != GameState.InProgress)
        {
            return new GameResponse { Message = "Game is already finished." };
        }

        // Make the move
        game.Board[row, column] = game.CurrentPlayer;

        var move = new Move
        {
            MoveNumber = game.MoveHistory.Count + 1,
            Player = game.CurrentPlayer,
            Row = row,
            Column = column,
            Timestamp = DateTime.UtcNow
        };

        game.MoveHistory.Add(move);

        // Check for win
        if (CheckWin(game, row, column))
        {
            game.State = game.CurrentPlayer == 'X' ? GameState.PlayerXWon : GameState.PlayerOWon;
            if (game.CurrentPlayer == 'X')
            {
                game.XWins++;
            }
            else
            {
                game.OWins++;
            }
        }
        // Check for draw
        else if (IsBoardFull(game.Board))
        {
            game.State = GameState.Draw;
            game.Draws++;
        }
        else
        {
            // Switch player
            game.CurrentPlayer = game.CurrentPlayer == 'X' ? 'O' : 'X';
        }

        return GetGameStateResponse(game);
    }

    public GameResponse UndoLastMove(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return new GameResponse { Message = "Game not found." };
        }

        if (game.MoveHistory.Count == 0)
        {
            return new GameResponse { Message = "No moves to undo." };
        }

        // Remove last move from board
        var lastMove = game.MoveHistory[game.MoveHistory.Count - 1];
        game.Board[lastMove.Row, lastMove.Column] = ' ';
        game.MoveHistory.RemoveAt(game.MoveHistory.Count - 1);

        // Reset game state
        game.State = GameState.InProgress;
        game.WinningPositions.Clear();

        // Switch back to previous player
        game.CurrentPlayer = lastMove.Player;

        return GetGameStateResponse(game);
    }

    public GameResponse ResetGame(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return new GameResponse { Message = "Game not found." };
        }

        game.ResetGame();
        return GetGameStateResponse(game);
    }

    public GameResponse ResetScoreboard(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return new GameResponse { Message = "Game not found." };
        }

        game.ResetGame();
        game.XWins = 0;
        game.OWins = 0;
        game.Draws = 0;

        return GetGameStateResponse(game);
    }

    public GameResponse GetGameState(string gameId)
    {
        if (!_games.TryGetValue(gameId, out var game))
        {
            return new GameResponse { Message = "Game not found." };
        }

        return GetGameStateResponse(game);
    }

    private bool CheckWin(GameSession game, int lastRow, int lastColumn)
    {
        var player = game.Board[lastRow, lastColumn];
        game.WinningPositions.Clear();

        // Check row
        if (CheckLine(game.Board, (lastRow, 0), (lastRow, 1), (lastRow, 2), player))
        {
            game.WinningPositions = new List<Position> 
            { 
                new Position(lastRow, 0), 
                new Position(lastRow, 1), 
                new Position(lastRow, 2) 
            };
            return true;
        }

        // Check column
        if (CheckLine(game.Board, (0, lastColumn), (1, lastColumn), (2, lastColumn), player))
        {
            game.WinningPositions = new List<Position> 
            { 
                new Position(0, lastColumn), 
                new Position(1, lastColumn), 
                new Position(2, lastColumn) 
            };
            return true;
        }

        // Check main diagonal
        if (lastRow == lastColumn && CheckLine(game.Board, (0, 0), (1, 1), (2, 2), player))
        {
            game.WinningPositions = new List<Position> 
            { 
                new Position(0, 0), 
                new Position(1, 1), 
                new Position(2, 2) 
            };
            return true;
        }

        // Check anti-diagonal
        if (lastRow + lastColumn == 2 && CheckLine(game.Board, (0, 2), (1, 1), (2, 0), player))
        {
            game.WinningPositions = new List<Position> 
            { 
                new Position(0, 2), 
                new Position(1, 1), 
                new Position(2, 0) 
            };
            return true;
        }

        return false;
    }

    private bool CheckLine(char[,] board, (int, int) pos1, (int, int) pos2, (int, int) pos3, char player)
    {
        return board[pos1.Item1, pos1.Item2] == player &&
               board[pos2.Item1, pos2.Item2] == player &&
               board[pos3.Item1, pos3.Item2] == player;
    }

    private bool IsBoardFull(char[,] board)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (board[i, j] == ' ')
                    return false;
            }
        }
        return true;
    }

    private GameResponse GetGameStateResponse(GameSession game)
    {
        var response = new GameResponse
        {
            GameId = game.Id,
            Board = ConvertBoardToStringJagged(game.Board),
            CurrentPlayer = game.CurrentPlayer.ToString(),
            State = game.State,
            MoveHistory = game.MoveHistory,
            WinningPositions = game.WinningPositions,
            XWins = game.XWins,
            OWins = game.OWins,
            Draws = game.Draws
        };

        return response;
    }

    private char[][] ConvertBoardToJagged(char[,] board)
    {
        var jagged = new char[3][];
        for (int i = 0; i < 3; i++)
        {
            jagged[i] = new char[3];
            for (int j = 0; j < 3; j++)
            {
                jagged[i][j] = board[i, j];
            }
        }
        return jagged;
    }

    private string[][] ConvertBoardToStringJagged(char[,] board)
    {
        var jagged = new string[3][];
        for (int i = 0; i < 3; i++)
        {
            jagged[i] = new string[3];
            for (int j = 0; j < 3; j++)
            {
                jagged[i][j] = board[i, j].ToString();
            }
        }
        return jagged;
    }
}
