using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Tetris.Game
{
    public class Grid
    {
        private Cell[,] _grid;
        private int _width, _height;
        public event Action<Cell> ValueChanged;

        public void Initialize(GameSettings settings)
        {
            _width = settings.GridWidth;
            // 블럭이 Grid의 위에서부터 나오므로 height는 크게 함
            _height = settings.GridHeight * 2;
            _grid = new Cell[_width, _height];
            for (var y = 0; y < _height; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    _grid[x, y] = new Cell(x, y);
                }
            }
        }

        public Cell this[int x, int y]
        {
            get => _grid[x, y];
            set
            {
                _grid[x, y] = value;
                ValueChanged?.Invoke(value);
            }
        }

        private void PrintStatus()
        {
            StringBuilder sb = new();
            for (int y = _height - 1; y >= 0; y--)
            {
                for (int x = 0; x < _width; x++)
                {
                    sb.Append($"| {_grid[x, y].Color} / {_grid[x, y].IsOccupied} | ");
                }

                sb.AppendLine();
            }

            Debug.Log(sb);
        }


        private bool IsValid(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height && !_grid[x, y].IsOccupied;
        }

        private bool IsValid(IList<Vector2Int> indexes)
        {
            foreach (var index in indexes)
            {
                if (!IsValid(index.x, index.y))
                {
                    return false;
                }
            }

            return true;
        }

        // 좌표의 cell occupy를 true로 변경
        public void PlaceTetromino(IList<Vector2Int> currentTetrominoCoordinates)
        {
            foreach (var coordinate in currentTetrominoCoordinates)
            {
                this[coordinate.x, coordinate.y].SetOccupy(true);
            }
        }

        // 완성된 줄을 제거
        public int ClearFullLines(IEnumerable<Vector2Int> coordinates)
        {
            int minHeight = coordinates.Select(coordinate => coordinate.y).Min();

            int clearedLineExceptGarbage = 0;

            for (int height = minHeight; height < _height; height++)
            {
                if (IsLineFull(height))
                {
                    if (this[0, height].Color != CellColor.Grey)
                    {
                        clearedLineExceptGarbage++;
                    }

                    ClearLine(height);
                    ShiftLinesDown(height);
                    height--;
                }
            }

            return clearedLineExceptGarbage;
        }

        private bool IsLineFull(int height)
        {
            for (var x = 0; x < _width; x++)
            {
                if (!this[x, height].IsOccupied)
                {
                    return false;
                }
            }

            return true;
        }

        private void ClearLine(int height)
        {
            for (var x = 0; x < _width; x++)
            {
                if (this[x, height].IsOccupied)
                {
                    this[x, height] = this[x, height].WithColor(CellColor.Empty);
                    this[x, height].SetOccupy(false);
                }
            }
        }


        // height + 1 줄부터 한칸씩 내림
        private void ShiftLinesDown(int height)
        {
            for (var y = height; y < _height - 1; y++)
            {
                for (var x = 0; x < _width; x++)
                {
                    if (this[x, y + 1].IsOccupied)
                    {
                        var current = this[x, y];
                        var above = this[x, y + 1];

                        Swap(current, above);
                    }
                }
            }
        }

        // 테트로미노가 다른 테트로미노와 겹치는지 확인
        public bool CheckCollision(IEnumerable<Vector2Int> coordinates)
        {
            foreach (var coordinate in coordinates)
            {
                if (_grid[coordinate.x, coordinate.y].IsOccupied)
                {
                    return true;
                }
            }

            return false;
        }

        private void ShiftLinesUp(int amount)
        {
            for (int y = _height - amount - 1; y >= 0; y--)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (this[x, y].IsOccupied)
                    {
                        var current = this[x, y];
                        var above = this[x, y + amount];

                        Swap(current, above);
                    }
                }
            }
        }

        // Cell 2개를 서로 교체
        private void Swap(Cell cell1, Cell cell2)
        {
            this[cell1.X, cell1.Y] = cell1.With(cell2.Color, cell2.IsOccupied);

            this[cell2.X, cell2.Y] = cell2.With(cell1.Color, cell1.IsOccupied);
        }


        /// <summary>
        /// 쓰레기 줄 삽입
        /// </summary>
        /// <param name="amount">쓰레기 줄의 개수</param>
        public void InsertGarbageBlock(int amount)
        {
            ShiftLinesUp(amount);
            for (int y = 0; y < amount; y++)
            {
                var randomEmptyIndex = Random.Range(0, _width);
                for (int x = 0; x < _width; x++)
                {
                    if (x == randomEmptyIndex)
                    {
                        this[x, y] = this[x, y].With(CellColor.Empty, false);
                    }
                    else
                    {
                        this[x, y] = this[x, y].With(CellColor.Grey, true);
                    }
                }
            }
        }


        public bool PositionChange(IList<Vector2Int> previousCoordinates, IList<Vector2Int> updatedCoordinate,
            CellColor color)
        {
            if (!IsValid(updatedCoordinate))
            {
                return false;
            }


            UpdateCellsColor(previousCoordinates, CellColor.Empty);

            UpdateCellsColor(updatedCoordinate, color);

            return true;
        }

        private void UpdateCellsColor(IList<Vector2Int> coordinates, CellColor color)
        {
            foreach (var coordinate in coordinates)
            {
                this[coordinate.x, coordinate.y] = this[coordinate.x, coordinate.y].WithColor(color);
            }
        }

        public bool CanMove(List<Vector2Int> movedCoordinates)
        {
            return IsValid(movedCoordinates);
        }

        public void ClearGhost(IList<Vector2Int> ghostCoordinates)
        {
            foreach (var ghostCoordinate in ghostCoordinates)
            {
                if (this[ghostCoordinate.x, ghostCoordinate.y].Color == CellColor.Ghost)
                {
                    this[ghostCoordinate.x, ghostCoordinate.y] =
                        this[ghostCoordinate.x, ghostCoordinate.y].WithColor(CellColor.Empty);
                }
            }
        }

        public void UpdateGhost(IList<Vector2Int> ghostCoordinates)
        {
            foreach (var ghostCoordinate in ghostCoordinates)
            {
                if (this[ghostCoordinate.x, ghostCoordinate.y].Color == CellColor.Empty)
                {
                    this[ghostCoordinate.x, ghostCoordinate.y] =
                        this[ghostCoordinate.x, ghostCoordinate.y].WithColor(CellColor.Ghost);
                }
            }
        }

    }
}