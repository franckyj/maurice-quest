using Vortice.Mathematics;

namespace MyTetris;

public readonly record struct BoardCellState(bool Filled, bool IsFromCurrentPiece, Color4 Color);
public readonly record struct BoardCell(int X, int Y);

internal class Board
{
    private readonly BoardCellState[,] _state;

    private Piece _currentPiece;
    private BoardCell[] _currentPieceCells;
    private int _currentLine;

    public short ColumnsCount { get; init; }
    public short RowsCount { get; init; }

    public Board()
    {
        ColumnsCount = 10;
        RowsCount = 20;

        _state = new BoardCellState[ColumnsCount, RowsCount];
        Reset();
    }

    public BoardCellState GetCellState(int column, int row)
    {
        if (column < 0 || row < 0 || column >= ColumnsCount || row >= RowsCount)
        {
            return new BoardCellState();
        }

        return _state[column, row];
    }

    public bool PushPieceDown()
    {
        if (_currentPiece is null) return false;

        // if pushing down the piece would result in overlapping
        // we need to update the board and start with a new piece
        var nextLineDown = _currentLine - 1;
        var nextFilledCells = _currentPiece.GetFilledCellsFromCurrentLine(nextLineDown);

        // are the cells overlapping or outside of the board?
        if (IsBelowBoard(nextFilledCells))
        {
            // need to start with the new piece
            // and not move this one anymore
            var newPiece = GenerateNewPiece();
            StartWithNewPiece(newPiece);

            // check if any rows have been filled
            ClearFilledLines();
        }
        else if (AreCellsOverlapping(nextFilledCells))
        {
            // also need to start with a new piece
            // but also need to validate if the game is over
            var newPiece = GenerateNewPiece();
            StartWithNewPiece(newPiece);

            // are we already overlapping?
            if (AreCellsOverlapping(_currentPieceCells))
            {
                // the game is over
                return false;
            }

            // check if any rows have been filled
            ClearFilledLines();
        }
        else
        {
            // un-fill the cells of the current piece
            EraseCurrentPieceFromBoard();
            PushDownCurrentPiece();
        }

        // fill the board with the current piece at the current line
        AddCurrentPieceToBoard();

        return true;
    }

    public void StartNewGame()
    {
        Reset();

        var newPiece = GenerateNewPiece();
        StartWithNewPiece(newPiece);
        AddCurrentPieceToBoard();
    }

    public void RotatePieceClockwise()
    {
        if (_currentPiece is null) return;

        var nextRotation = _currentPiece.GetNextClockwiseRotation();
        var wannaBeCells = _currentPiece.GetFilledCellsFromCurrentLine(_currentLine, nextRotation);

        // meaning that this rotation is invalid
        if (IsBelowBoard(wannaBeCells) || IsAboveBoard(wannaBeCells) || AreCellsOverlapping(wannaBeCells)) return;

        EraseCurrentPieceFromBoard();
        _currentPiece.RotateClockwise();
        _currentPieceCells = _currentPiece.GetFilledCellsFromCurrentLine(_currentLine);
        AddCurrentPieceToBoard();
    }

    public void RotatePieceCounterClockwise()
    {
        if (_currentPiece is null) return;

        var nextRotation = _currentPiece.GetNextCounterClockwiseRotation();
        var wannaBeCells = _currentPiece.GetFilledCellsFromCurrentLine(_currentLine, nextRotation);

        // meaning that this rotation is invalid
        if (IsBelowBoard(wannaBeCells) || IsAboveBoard(wannaBeCells) || AreCellsOverlapping(wannaBeCells)) return;

        EraseCurrentPieceFromBoard();
        _currentPiece.RotateCounterClockwise();
        _currentPieceCells = _currentPiece.GetFilledCellsFromCurrentLine(_currentLine);
        AddCurrentPieceToBoard();
    }

    public void MovePieceRight() => MovePiece(1);
    public void MovePieceLeft() => MovePiece(-1);

    private void ClearFilledLines()
    {
        int dropAmount = 0;

        for (int row = 0; row < RowsCount - 1; row++)
        {
            bool isClear = true;
            for (int column = 0; column < ColumnsCount; column++)
            {
                var state = GetCellState(column, row);
                if (!state.Filled)
                {
                    isClear = false;

                    if (dropAmount == 0 || row == 0)
                        break;

                    for (int drop = 0; drop < ColumnsCount; drop++)
                    {
                        var toDrop = GetCellState(drop, row);
                        if (toDrop.Filled)
                        {
                            _state[drop, row] = new BoardCellState(false, false, Colors.Black);
                            _state[drop, row - dropAmount] = toDrop;
                        }
                    }
                    break;
                }
            }

            if (isClear)
            {
                // need to clear this line
                for (int clear = 0; clear < ColumnsCount; clear++)
                {
                    _state[clear, row] = new BoardCellState(false, false, Colors.Black);
                }

                dropAmount++;
            }
        }
    }

    private void MovePiece(int x)
    {
        if (_currentPiece is null) return;

        // validate that the piece's cells don't overflow the board
        foreach (var cell in _currentPieceCells)
        {
            var newX = cell.X + x;
            if (newX < 0 || newX >= ColumnsCount)
                return;

            // would the new offset make the piece overlap?
            // if not move the piece
            // otherwise do nothing
            if (_state[newX, cell.Y].Filled && !_state[newX, cell.Y].IsFromCurrentPiece)
                return;
        }

        _currentPiece.CurrentOffset += x;

        EraseCurrentPieceFromBoard();
        _currentPieceCells = _currentPiece.GetFilledCellsFromCurrentLine(_currentLine);
        AddCurrentPieceToBoard();
    }

    private void StartWithNewPiece(Piece piece)
    {
        _currentPiece = piece;

        // fill the cells with the current rotation
        _currentLine = RowsCount - 1;
        var cellsToFill = _currentPiece.GetFilledCellsFromCurrentLine(_currentLine);

        if (_currentPieceCells is not null)
        {
            // copy the piece to the board
            foreach (var (x, y) in _currentPieceCells)
            {
                var cell = _state[x, y];
                _state[x, y] = cell with { IsFromCurrentPiece = false };
            }
        }

        _currentPieceCells = cellsToFill;
    }

    private void Reset()
    {
        // initialize the board with all empty cells
        for (int row = 0; row < RowsCount; row++)
        {
            for (int col = 0; col < ColumnsCount; col++)
            {
                //_state[col, row] = new BoardCellState(false, false, Colors.White);
                var filled = col < ColumnsCount - 2 && row < 2;
                _state[col, row] = new BoardCellState(filled, false, Colors.White);
            }
        }

        _currentLine = RowsCount - 1;
        _currentPiece = null;
        _currentPieceCells = null;
    }

    private void PushDownCurrentPiece()
    {
        if (_currentPiece is null) return;

        // decrement the current line
        _currentLine--;
        _currentPieceCells = _currentPiece.GetFilledCellsFromCurrentLine(_currentLine);
    }

    private void EraseCurrentPieceFromBoard()
    {
        if (_currentPieceCells is null) return;

        foreach (var (x, y) in _currentPieceCells)
        {
            var cell = _state[x, y];
            _state[x, y] = cell with { Filled = false, IsFromCurrentPiece = false, Color = Colors.White };
        }
    }

    private void AddCurrentPieceToBoard()
    {
        if (_currentPieceCells is null) return;

        foreach (var (x, y) in _currentPieceCells)
        {
            var cell = _state[x, y];
            _state[x, y] = cell with { Filled = true, IsFromCurrentPiece = true, Color = Colors.Black };
        }
    }

    private bool AreCellsOverlapping(BoardCell[] cellsToFill)
    {
        var overlapping = false;

        foreach (var (x, y) in cellsToFill)
        {
            var cell = _state[x, y];
            if (cell.Filled && !cell.IsFromCurrentPiece)
            {
                overlapping = true;
                break;
            }
        }

        return overlapping;
    }

    private bool IsAboveBoard(BoardCell[] cellsToFill)
    {
        var outside = false;

        foreach (var (x, y) in cellsToFill)
        {
            if (y >= RowsCount)
            {
                outside = true;
                break;
            }
        }

        return outside;
    }

    private bool IsBelowBoard(BoardCell[] cellsToFill)
    {
        var outside = false;

        foreach (var (x, y) in cellsToFill)
        {
            if (y < 0)
            {
                outside = true;
                break;
            }
        }

        return outside;
    }

    public Piece GenerateNewPiece()
    {
        //var newPiece = DateTime.UtcNow.Second % 2 == 0
        //    ? PieceSpawner.GetO()
        //    : PieceSpawner.GetI();

        if (_currentPiece == null)
            return PieceSpawner.GetZ();

        return _currentPiece.Type == PieceType.O
            ? PieceSpawner.GetI()
            : PieceSpawner.GetO();
    }
}
