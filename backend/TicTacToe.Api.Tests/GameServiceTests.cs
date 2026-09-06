using TicTacToe.Api.Services;
using TicTacToe.Api.Models;
using Xunit;

namespace TicTacToe.Api.Tests
{
    public class GameServiceTests
    {
        private readonly IGameService _gameService;

        public GameServiceTests()
        {
            _gameService = new GameService();
        }

        [Fact]
        public void CreateNewGame_DefaultMode_CreatesGameWithCorrectInitialState()
        {
            Console.WriteLine("Test CreateNewGame_DefaultMode_CreatesGameWithCorrectInitialState started");
            // Act
            var game = _gameService.CreateNewGame();

            // Assert
            Assert.NotNull(game);
            Assert.Equal(GameMode.TwoPlayer, game.GameMode);
            Assert.Equal(GameState.InProgress, game.State);
            Assert.Equal('X', game.CurrentPlayer);
            Assert.Empty(game.MoveHistory);
            Assert.Equal(0, game.XWins);
            Assert.Equal(0, game.OWins);
            Assert.Equal(0, game.Draws);
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Assert.Equal(' ', game.Board[r, c]);
                }
            }
        }

        [Fact]
        public void CreateNewGame_ComputerMode_CreatesGameWithCorrectInitialState()
        {
            // Act
            var game = _gameService.CreateNewGame(GameMode.Computer);

            // Assert
            Assert.Equal(GameMode.Computer, game.GameMode);
            Assert.Equal(GameState.InProgress, game.State);
            Assert.Equal('X', game.CurrentPlayer);
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Assert.Equal(' ', game.Board[r, c]);
                }
            }
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        [InlineData(3, 0)]
        [InlineData(0, 3)]
        [InlineData(2, 3)]
        public void MakeMove_InvalidCoordinates_ReturnsError(int row, int column)
        {
            // Arrange
            var game = _gameService.CreateNewGame();

            // Act
            var response = _gameService.MakeMove(game.Id, row, column);

            // Assert
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.Contains("Invalid", response.Message);
            Assert.Equal(string.Empty, response.GameId);
        }

        [Fact]
        public void MakeMove_ValidMove_UpdatesBoardAndSwitchesPlayer()
        {
            // Arrange
            var game = _gameService.CreateNewGame();
            int row = 0, column = 0;

            // Act
            var response = _gameService.MakeMove(game.Id, row, column);

            // Assert
            Assert.Equal(GameState.InProgress, response.State);
            Assert.Equal("O", response.CurrentPlayer);
            Assert.Single(response.MoveHistory);
            Assert.Equal(1, response.MoveHistory[0].MoveNumber);
            Assert.Equal('X', response.MoveHistory[0].Player);
            Assert.Equal(0, response.MoveHistory[0].Row);
            Assert.Equal(0, response.MoveHistory[0].Column);
            // Internal state
            Assert.Equal('X', game.Board[0, 0]);
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (r == 0 && c == 0)
                    {
                        Assert.Equal('X', game.Board[r, c]);
                    }
                    else
                    {
                        Assert.Equal(' ', game.Board[r, c]);
                    }
                }
            }
        }

        [Fact]
        public void MakeMove_OccupiedCell_ReturnsError()
        {
            // Arrange
            var game = _gameService.CreateNewGame();
            _gameService.MakeMove(game.Id, 0, 0); // First move

            // Act
            var response = _gameService.MakeMove(game.Id, 0, 0); // Try to move again

            // Assert
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.Contains("occupied", response.Message);
        }

        [Fact]
        public void MakeMove_AfterGameCompletion_ReturnsError()
        {
            // Arrange
            var game = _gameService.CreateNewGame();
            // Set up a winning position for X: (0,0), (0,1), (0,2)
            _gameService.MakeMove(game.Id, 0, 0); // X
            _gameService.MakeMove(game.Id, 1, 0); // O
            _gameService.MakeMove(game.Id, 0, 1); // X
            _gameService.MakeMove(game.Id, 1, 1); // O
            _gameService.MakeMove(game.Id, 0, 2); // X wins

            // Act
            var response = _gameService.MakeMove(game.Id, 2, 0); // Try to move after win

            // Assert
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.Contains("finished", response.Message);
            Assert.Equal(string.Empty, response.GameId);
        }

        [Fact]
        public void MakeMove_ResultsInRowWin_UpdatesStateAndScoreboard()
        {
            // Arrange
            var game = _gameService.CreateNewGame();
            // X moves: (0,0), (0,1), (0,2)
            _gameService.MakeMove(game.Id, 0, 0); // X
            _gameService.MakeMove(game.Id, 1, 0); // O
            _gameService.MakeMove(game.Id, 0, 1); // X
            _gameService.MakeMove(game.Id, 1, 1); // O

            // Act
            var response = _gameService.MakeMove(game.Id, 0, 2); // X wins

            // Assert
            Assert.Equal(GameState.PlayerXWon, response.State);
            Assert.Equal(1, response.XWins);
            Assert.Equal(0, response.OWins);
            Assert.Equal(0, response.Draws);
            Assert.NotNull(response.WinningPositions);
            Assert.Equal(3, response.WinningPositions.Count);
            Assert.True(response.WinningPositions.Any(p => p.Row == 0 && p.Column == 0));
            Assert.True(response.WinningPositions.Any(p => p.Row == 0 && p.Column == 1));
            Assert.True(response.WinningPositions.Any(p => p.Row == 0 && p.Column == 2));
            // Internal state
            Assert.Equal(GameState.PlayerXWon, game.State);
            Assert.Equal('X', game.Board[0, 0]);
            Assert.Equal('X', game.Board[0, 1]);
            Assert.Equal('X', game.Board[0, 2]);
        }

        [Fact]
        public void MakeMove_ResultsInColumnWin_UpdatesStateAndScoreboard()
        {
            // Arrange
            var game = _gameService.CreateNewGame();
            // X moves: (0,0), (1,0), (2,0)
            _gameService.MakeMove(game.Id, 0, 0); // X
            _gameService.MakeMove(game.Id, 0, 1); // O
            _gameService.MakeMove(game.Id, 1, 0); // X
            _gameService.MakeMove(game.Id, 1, 1); // O

            // Act
            var response = _gameService.MakeMove(game.Id, 2, 0); // X wins

            // Assert
            Assert.Equal(GameState.PlayerXWon, response.State);
            Assert.Equal(1, response.XWins);
            Assert.Equal(0, response.OWins);
            Assert.Equal(0, response.Draws);
            Assert.NotNull(response.WinningPositions);
            Assert.Equal(3, response.WinningPositions.Count);
            Assert.True(response.WinningPositions.Any(p => p.Row == 0 && p.Column == 0));
            Assert.True(response.WinningPositions.Any(p => p.Row == 1 && p.Column == 0));
            Assert.True(response.WinningPositions.Any(p => p.Row == 2 && p.Column == 0));
            // Internal state
            Assert.Equal(GameState.PlayerXWon, game.State);
            Assert.Equal('X', game.Board[0, 0]);
            Assert.Equal('X', game.Board[1, 0]);
            Assert.Equal('X', game.Board[2, 0]);
        }

        [Fact]
        public void MakeMove_ResultsInDiagonalWin_UpdatesStateAndScoreboard()
        {
            // Arrange
            var game = _gameService.CreateNewGame();
            // X moves: (0,0), (1,1), (2,2)
            _gameService.MakeMove(game.Id, 0, 0); // X
            _gameService.MakeMove(game.Id, 0, 1); // O
            _gameService.MakeMove(game.Id, 1, 1); // X
            _gameService.MakeMove(game.Id, 0, 2); // O

            // Act
            var response = _gameService.MakeMove(game.Id, 2, 2); // X wins

            // Assert
            Assert.Equal(GameState.PlayerXWon, response.State);
            Assert.Equal(1, response.XWins);
            Assert.Equal(0, response.OWins);
            Assert.Equal(0, response.Draws);
            Assert.NotNull(response.WinningPositions);
            Assert.Equal(3, response.WinningPositions.Count);
            Assert.True(response.WinningPositions.Any(p => p.Row == 0 && p.Column == 0));
            Assert.True(response.WinningPositions.Any(p => p.Row == 1 && p.Column == 1));
            Assert.True(response.WinningPositions.Any(p => p.Row == 2 && p.Column == 2));
            // Internal state
            Assert.Equal(GameState.PlayerXWon, game.State);
            Assert.Equal('X', game.Board[0, 0]);
            Assert.Equal('X', game.Board[1, 1]);
            Assert.Equal('X', game.Board[2, 2]);
        }

        [Fact]
        public void MakeMove_ResultsInDraw_UpdatesStateAndScoreboard()
        {
            // Arrange
            var game = _gameService.CreateNewGame();
            _gameService.MakeMove(game.Id, 0, 0); // X
            _gameService.MakeMove(game.Id, 0, 1); // O
            _gameService.MakeMove(game.Id, 0, 2); // X
            _gameService.MakeMove(game.Id, 1, 1); // O
            _gameService.MakeMove(game.Id, 1, 0); // X
            _gameService.MakeMove(game.Id, 1, 2); // O
            _gameService.MakeMove(game.Id, 2, 1); // X
            _gameService.MakeMove(game.Id, 2, 0); // O
            var response = _gameService.MakeMove(game.Id, 2, 2); // X -> Draw

            // Assert
            Assert.Equal(GameState.Draw, response.State);
            Assert.Equal(0, response.XWins);
            Assert.Equal(0, response.OWins);
            Assert.Equal(1, response.Draws);
            // Internal state
            Assert.Equal(GameState.Draw, game.State);
            Assert.Equal('X', game.Board[0, 0]);
            Assert.Equal('O', game.Board[0, 1]);
            Assert.Equal('X', game.Board[0, 2]);
            Assert.Equal('X', game.Board[1, 0]);
            Assert.Equal('O', game.Board[1, 1]);
            Assert.Equal('O', game.Board[1, 2]);
            Assert.Equal('O', game.Board[2, 0]);
            Assert.Equal('X', game.Board[2, 1]);
            Assert.Equal('X', game.Board[2, 2]);
        }

        [Fact]
        public void ResetGame_ClearsBoardAndHistory_ButKeepsScoreboard()
        {
            // Arrange
            var game = _gameService.CreateNewGame();
            // Play a few moves
            _gameService.MakeMove(game.Id, 0, 0); // X
            _gameService.MakeMove(game.Id, 1, 1); // O

            // Get initial scoreboard
            var initialState = _gameService.GetGameState(game.Id);
            int initialXWins = initialState.XWins;
            int initialOWins = initialState.OWins;
            int initialDraws = initialState.Draws;

            // Act
            var response = _gameService.ResetGame(game.Id);

            // Assert
            Assert.Equal(GameState.InProgress, response.State);
            Assert.Equal("X", response.CurrentPlayer);
            Assert.Empty(response.MoveHistory);
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Assert.Equal(" ", response.Board[r][c]);
                }
            }
            // Scoreboard should be preserved
            Assert.Equal(initialXWins, response.XWins);
            Assert.Equal(initialOWins, response.OWins);
            Assert.Equal(initialDraws, response.Draws);
            // Internal state
            Assert.Equal(GameState.InProgress, game.State);
            Assert.Equal('X', game.CurrentPlayer);
            Assert.Empty(game.MoveHistory);
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Assert.Equal(' ', game.Board[r, c]);
                }
            }
            Assert.Equal(initialXWins, game.XWins);
            Assert.Equal(initialOWins, game.OWins);
            Assert.Equal(initialDraws, game.Draws);
        }

        [Fact]
        public void ResetScoreboard_ClearsScoreboard_ButKeepsGameState()
        {
            // Arrange
            var game = _gameService.CreateNewGame();
            // Play a game to completion to get some score
            _gameService.MakeMove(game.Id, 0, 0); // X
            _gameService.MakeMove(game.Id, 1, 0); // O
            _gameService.MakeMove(game.Id, 0, 1); // X
            _gameService.MakeMove(game.Id, 1, 1); // O
            _gameService.MakeMove(game.Id, 0, 2); // X wins

            var completedState = _gameService.GetGameState(game.Id);
            Assert.Equal(GameState.PlayerXWon, completedState.State);
            Assert.Equal(1, completedState.XWins);

            // Act
            var response = _gameService.ResetScoreboard(game.Id);

            // Assert
            // Game state should be unchanged (still completed)
            Assert.Equal(GameState.PlayerXWon, response.State);
            Assert.Equal("X", response.CurrentPlayer);
            // Scoreboard should be reset
            Assert.Equal(0, response.XWins);
            Assert.Equal(0, response.OWins);
            Assert.Equal(0, response.Draws);
            // Internal state
            Assert.Equal(GameState.PlayerXWon, game.State);
            Assert.Equal('X', game.CurrentPlayer);
            Assert.Equal(0, game.XWins);
            Assert.Equal(0, game.OWins);
            Assert.Equal(0, game.Draws);
        }

        [Fact]
        public void UndoLastMove_TwoPlayerMode_RemovesSingleMove()
        {
            // Arrange
            var game = _gameService.CreateNewGame(GameMode.TwoPlayer);
            _gameService.MakeMove(game.Id, 0, 0); // X
            _gameService.MakeMove(game.Id, 1, 1); // O

            // Act
            var response = _gameService.UndoLastMove(game.Id);

            // Assert
            Assert.Equal(GameState.InProgress, response.State);
            Assert.Equal("O", response.CurrentPlayer);
            Assert.Single(response.MoveHistory);
            Assert.Equal(1, response.MoveHistory[0].MoveNumber);
            Assert.Equal('X', response.MoveHistory[0].Player);
            Assert.Equal(0, response.MoveHistory[0].Row);
            Assert.Equal(0, response.MoveHistory[0].Column);
            Assert.Equal(" ", response.Board[1][1]);
            Assert.Equal("X", response.Board[0][0]);
            // Internal state
            Assert.Equal(GameState.InProgress, game.State);
            Assert.Equal('O', game.CurrentPlayer);
            Assert.Single(game.MoveHistory);
            Assert.Equal('X', game.Board[0, 0]);
            Assert.Equal(' ', game.Board[1, 1]);
        }

        [Fact]
        public void UndoLastMove_ComputerMode_RemovesHumanAndComputerPair()
        {
            // Arrange
            var game = _gameService.CreateNewGame(GameMode.Computer);
            // Human (X) moves at (0,0)
            _gameService.MakeMove(game.Id, 0, 0); // X

            // Act
            var response = _gameService.UndoLastMove(game.Id);

            // Assert
            Assert.Equal(GameState.InProgress, response.State);
            Assert.Equal("X", response.CurrentPlayer);
            Assert.Empty(response.MoveHistory);
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Assert.Equal(" ", response.Board[r][c]);
                }
            }
            // Internal state
            Assert.Equal(GameState.InProgress, game.State);
            Assert.Equal('X', game.CurrentPlayer);
            Assert.Empty(game.MoveHistory);
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    Assert.Equal(' ', game.Board[r, c]);
                }
            }
        }

        [Fact]
        public void UndoLastMove_NoMovesToUndo_ReturnsError()
        {
            // Arrange
            var game = _gameService.CreateNewGame();

            // Act
            var response = _gameService.UndoLastMove(game.Id);

            // Assert
            Assert.False(string.IsNullOrEmpty(response.Message));
            Assert.Contains("No moves", response.Message);
        }

        [Fact]
        public void GetGameState_ReturnsCurrentState()
        {
            // Arrange
            var game = _gameService.CreateNewGame();
            _gameService.MakeMove(game.Id, 0, 0); // X
            _gameService.MakeMove(game.Id, 1, 1); // O

            // Act
            var response = _gameService.GetGameState(game.Id);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(game.Id, response.GameId);
            Assert.Equal(2, response.MoveHistory.Count);
            Assert.Equal("X", response.Board[0][0]);
            Assert.Equal("O", response.Board[1][1]);
            // Internal state
            Assert.Equal('X', game.Board[0, 0]);
            Assert.Equal('O', game.Board[1, 1]);
        }
    }
}