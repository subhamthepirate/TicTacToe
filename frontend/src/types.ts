// Type definitions for the game models
export enum GameState {
  InProgress = "InProgress",
  PlayerXWon = "PlayerXWon",
  PlayerOWon = "PlayerOWon",
  Draw = "Draw",
}

export enum GameMode {
  TwoPlayer = "TwoPlayer",
  Computer = "Computer"
}

export interface Move {
  moveNumber: number;
  player: string;
  row: number;
  column: number;
  timestamp: string;
}

export interface Position {
  row: number;
  column: number;
}

export interface GameResponse {
  gameId: string;
  board: string[][];
  currentPlayer: string;
  state: GameState;
  gameMode: GameMode;
  moveHistory: Move[];
  winningPositions: Position[];
  xWins: number;
  oWins: number;
  draws: number;
  message: string;
}

export interface MakeMoveRequest {
  row: number;
  column: number;
}
