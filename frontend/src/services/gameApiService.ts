import { GameResponse, MakeMoveRequest, GameMode } from "../types";

const API_BASE_URL = "http://localhost:5118/api/games";

class GameApiService {
  async createNewGame(gameMode: GameMode = GameMode.TwoPlayer): Promise<GameResponse> {
    const response = await fetch(`${API_BASE_URL}?mode=${gameMode}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error("Failed to create new game");
    }

    return response.json();
  }

  async makeMove(gameId: string, row: number, column: number): Promise<GameResponse> {
    const request: MakeMoveRequest = { row, column };

    const response = await fetch(`${API_BASE_URL}/${gameId}/move`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error("Failed to make move");
    }

    return response.json();
  }

  async undoLastMove(gameId: string): Promise<GameResponse> {
    const response = await fetch(`${API_BASE_URL}/${gameId}/undo`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error("Failed to undo move");
    }

    return response.json();
  }

  async resetGame(gameId: string): Promise<GameResponse> {
    const response = await fetch(`${API_BASE_URL}/${gameId}/reset`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error("Failed to reset game");
    }

    return response.json();
  }

  async resetScoreboard(gameId: string): Promise<GameResponse> {
    const response = await fetch(`${API_BASE_URL}/${gameId}/reset-scoreboard`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error("Failed to reset scoreboard");
    }

    return response.json();
  }

  async getGameState(gameId: string): Promise<GameResponse> {
    const response = await fetch(`${API_BASE_URL}/${gameId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error("Failed to get game state");
    }

    return response.json();
  }
}

export default new GameApiService();
