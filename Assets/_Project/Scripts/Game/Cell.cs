using Tetris.Game;

namespace Tetris.Game
{
    public class Cell
    {
        public int X => _x;
        public int Y => _y;
        public bool IsOccupied => _isOccupied;
        private bool _isOccupied;
        private CellColor _color = CellColor.Empty;
        private int _x;
        private int _y;
        public CellColor Color => _color;

        public Cell(int x, int y)
        {
            _x = x;
            _y = y;
        }

        private Cell()
        { }

        public Cell WithColor(CellColor color)
        {
            return new Cell()
            {
                _color = color,
                _isOccupied = IsOccupied,
                _x = X,
                _y = Y
            };
        }

        public Cell With(CellColor color)
        {
            return new Cell()
            {
                _color = color,
                _isOccupied = IsOccupied,
                _x = X,
                _y = Y
            };
        }

        public Cell With(CellColor color, bool isOccupied)
        {
            return new Cell()
            {
                _color = color,
                _isOccupied = isOccupied,
                _x = X,
                _y = Y
            };
        }

        public void SetOccupy(bool occupy) => _isOccupied = occupy;
    }
}