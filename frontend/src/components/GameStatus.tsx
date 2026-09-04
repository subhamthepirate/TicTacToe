import { GameState } from "../types";

interface GameStatusProps {
  currentPlayer: string;
  gameState: GameState;
  onResetGame: () => void;
  onResetScoreboard: () => void;
  onUndoMove: () => void;
  canUndo: boolean;
}

export function GameStatus({
  currentPlayer,
  gameState,
  onResetGame,
  onResetScoreboard,
  onUndoMove,
  canUndo,
}: GameStatusProps) {
  const getStatusMessage = () => {
    switch (gameState) {
      case GameState.InProgress:
        return `Current Player: ${currentPlayer}`;
      case GameState.PlayerXWon:
        return "🎉 Player X Won!";
      case GameState.PlayerOWon:
        return "🎉 Player O Won!";
      case GameState.Draw:
        return "🤝 It's a Draw!";
      default:
        return "";
    }
  };

  const isGameFinished = gameState !== GameState.InProgress;

  return (
    <div className="game-status">
      <h2>{getStatusMessage()}</h2>
      <div className="button-group">
        <button
          onClick={onUndoMove}
          disabled={!canUndo}
          className="btn btn-secondary"
        >
          ↶ Undo Move
        </button>
        <button onClick={onResetGame} className="btn btn-primary">
          🔄 Reset Game
        </button>
        <button onClick={onResetScoreboard} className="btn btn-danger">
          🗑️ Reset Scoreboard
        </button>
      </div>
    </div>
  );
}
