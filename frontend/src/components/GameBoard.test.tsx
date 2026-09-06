import { render, screen, fireEvent } from '@testing-library/react';
import { GameBoard } from './GameBoard';
import { Position } from '../types';
import { vi } from 'vitest';

describe('GameBoard', () => {
  it('renders the board correctly', () => {
    const onCellClick = vi.fn();
    const board: string[][] = [
      ['X', 'O', ' '],
      [' ', 'X', ' '],
      ['O', ' ', ' ']
    ];
    const winningPositions: Position[] = [
      { row: 0, column: 0 },
      { row: 1, column: 1 },
      { row: 2, column: 2 }
    ];

    render(<GameBoard board={board} onCellClick={onCellClick} winningPositions={winningPositions} gameFinished={false} />);

    // Check that all cells are rendered
    const cells = screen.getAllByRole('button');
    expect(cells).toHaveLength(9);

    // Check first row
    expect(cells[0]).toHaveTextContent('X');
    expect(cells[1]).toHaveTextContent('O');
    expect(cells[2]).toHaveTextContent('');
    // Check second row
    expect(cells[3]).toHaveTextContent('');
    expect(cells[4]).toHaveTextContent('X');
    expect(cells[5]).toHaveTextContent('');
    // Check third row
    expect(cells[6]).toHaveTextContent('O');
    expect(cells[7]).toHaveTextContent('');
    expect(cells[8]).toHaveTextContent('');
  });

  it('calls onCellClick when an empty cell is clicked', () => {
    const onCellClick = vi.fn();
    // Use a board where the center is empty for clicking
    const board: string[][] = [
      [' ', ' ', ' '],
      [' ', ' ', ' '],
      [' ', ' ', ' ']
    ];
    const winningPositions: Position[] = [];

    render(<GameBoard board={board} onCellClick={onCellClick} winningPositions={winningPositions} gameFinished={false} />);

    const allButtons = screen.getAllByRole('button');
    const centerButton = allButtons[4]; // row 1, col 1

    fireEvent.click(centerButton);

    expect(onCellClick).toHaveBeenCalledWith(1, 1);
  });

  it('displays winning cells with the winning class', () => {
    const onCellClick = vi.fn();
    const board: string[][] = [
      ['X', 'O', ' '],
      [' ', 'X', ' '],
      ['O', ' ', ' ']
    ];
    const winningPositions: Position[] = [
      { row: 0, column: 0 },
      { row: 1, column: 1 },
      { row: 2, column: 2 }
    ];

    render(<GameBoard board={board} onCellClick={onCellClick} winningPositions={winningPositions} gameFinished={true} />);

    const allButtons = screen.getAllByRole('button');
    // Check that the winning cells have the 'winning' class
    expect(allButtons[0]).toHaveClass('winning'); // (0,0)
    expect(allButtons[4]).toHaveClass('winning'); // (1,1)
    expect(allButtons[8]).toHaveClass('winning'); // (2,2)
    // Non-winning cells should not have the winning class
    expect(allButtons[1]).not.toHaveClass('winning'); // (0,1)
    expect(allButtons[2]).not.toHaveClass('winning'); // (0,2)
    expect(allButtons[3]).not.toHaveClass('winning'); // (1,0)
    expect(allButtons[5]).not.toHaveClass('winning'); // (1,2)
    expect(allButtons[6]).not.toHaveClass('winning'); // (2,0)
    expect(allButtons[7]).not.toHaveClass('winning'); // (2,1)
  });

  it('disables cells when the game is finished', () => {
    const onCellClick = vi.fn();
    const board: string[][] = [
      ['X', 'O', ' '],
      [' ', 'X', ' '],
      ['O', ' ', ' ']
    ];
    const winningPositions: Position[] = [];

    render(<GameBoard board={board} onCellClick={onCellClick} winningPositions={winningPositions} gameFinished={true} />);

    const allButtons = screen.getAllByRole('button');
    allButtons.forEach(button => {
      expect(button).toBeDisabled();
    });
  });

  it('disables cells that are already filled', () => {
    const onCellClick = vi.fn();
    const board: string[][] = [
      ['X', 'O', ' '],
      [' ', 'X', ' '],
      ['O', ' ', ' ']
    ];
    const winningPositions: Position[] = [];

    render(<GameBoard board={board} onCellClick={onCellClick} winningPositions={winningPositions} gameFinished={false} />);

    const allButtons = screen.getAllByRole('button');
    // Cells that are filled: (0,0)='X', (0,1)='O', (1,1)='X', (2,0)='O'
    expect(allButtons[0]).toBeDisabled(); // X
    expect(allButtons[1]).toBeDisabled(); // O
    expect(allButtons[4]).toBeDisabled(); // X
    expect(allButtons[6]).toBeDisabled(); // O
    // Empty cells should be enabled
    expect(allButtons[2]).toBeEnabled(); // ' '
    expect(allButtons[3]).toBeEnabled(); // ' '
    expect(allButtons[5]).toBeEnabled(); // ' '
    expect(allButtons[7]).toBeEnabled(); // ' '
    expect(allButtons[8]).toBeEnabled(); // ' '
  });
});