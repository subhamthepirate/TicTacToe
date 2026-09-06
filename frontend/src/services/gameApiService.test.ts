import gameApiService from './gameApiService';
import { GameResponse, GameState, GameMode } from '../types';

// Mock fetch
const mockFetch = vi.fn();
global.fetch = mockFetch;

describe('gameApiService', () => {
  beforeEach(() => {
    mockFetch.mockReset();
  });

  describe('createNewGame', () => {
    it('should create a new game with default mode', async () => {
      const mockResponse: GameResponse = {
        gameId: 'test-id',
        board: [[' ', ' ', ' '], [' ', ' ', ' '], [' ', ' ', ' ']],
        currentPlayer: 'X',
        state: GameState.InProgress,
        moveHistory: [],
        winningPositions: [],
        xWins: 0,
        oWins: 0,
        draws: 0,
        message: ''
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      } as Response);

      const result = await gameApiService.createNewGame();

      expect(mockFetch).toHaveBeenCalledWith('http://localhost:5118/api/games?mode=TwoPlayer', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      expect(result).toEqual(mockResponse);
    });

    it('should create a new game with computer mode', async () => {
      const mockResponse: GameResponse = {
        gameId: 'test-id',
        board: [[' ', ' ', ' '], [' ', ' ', ' '], [' ', ' ', ' ']],
        currentPlayer: 'X',
        state: GameState.InProgress,
        moveHistory: [],
        winningPositions: [],
        xWins: 0,
        oWins: 0,
        draws: 0,
        message: ''
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      } as Response);

      const result = await gameApiService.createNewGame(GameMode.Computer);

      expect(mockFetch).toHaveBeenCalledWith('http://localhost:5118/api/games?mode=Computer', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
      });
      expect(result).toEqual(mockResponse);
    });

    it('should throw an error if the response is not ok', async () => {
      mockFetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        statusText: 'Internal Server Error'
      } as Response);

      await expect(gameApiService.createNewGame()).rejects.toThrow('Failed to create new game');
    });
  });

  describe('makeMove', () => {
    it('should make a move and return the updated game state', async () => {
      const gameId = 'test-id';
      const mockResponse: GameResponse = {
        gameId,
        board: [['X', ' ', ' '], [' ', ' ', ' '], [' ', ' ', ' ']],
        currentPlayer: 'O',
        state: GameState.InProgress,
        moveHistory: [{ moveNumber: 1, player: 'X', row: 0, column: 0, timestamp: new Date() }],
        winningPositions: [],
        xWins: 0,
        oWins: 0,
        draws: 0,
        message: ''
      };

      mockFetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      } as Response);

      const result = await gameApiService.makeMove(gameId, 0, 0);

      expect(mockFetch).toHaveBeenCalledWith(`http://localhost:5118/api/games/${gameId}/move`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ row: 0, column: 0 })
      });
      expect(result).toEqual(mockResponse);
    });
  });
});