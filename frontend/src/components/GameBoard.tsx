import { Position } from "../types";
import "../styles/GameBoard.css";

interface GameBoardProps {
  board: string[][];
  onCellClick: (row: number, column: number) => void;
  winningPositions: Position[];
  gameFinished: boolean;
}

export function GameBoard({
  board,
  onCellClick,
  winningPositions,
  gameFinished,
}: GameBoardProps) {
  const isWinningCell = (row: number, column: number): boolean => {
    return winningPositions.some((pos) => pos.row === row && pos.column === column);
  };

  const handleCellClick = (row: number, column: number) => {
    if (!gameFinished && board[row][column] === " ") {
      onCellClick(row, column);
    }
  };

  return (
    <div className="game-board">
      {board.map((row, rowIndex) => (
        <div key={rowIndex} className="board-row">
          {row.map((cell, colIndex) => (
            <button
              key={`${rowIndex}-${colIndex}`}
              className={`board-cell ${isWinningCell(rowIndex, colIndex) ? "winning" : ""} ${cell !== " " ? "filled" : ""}`}
              onClick={() => handleCellClick(rowIndex, colIndex)}
              disabled={cell !== " " || gameFinished}
            >
              {cell !== " " ? cell : ""}
            </button>
          ))}
        </div>
      ))}
    </div>
  );
}
