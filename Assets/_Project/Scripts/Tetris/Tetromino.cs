using System;
using System.Collections.Generic;
using System.Linq;
using Tetris.Extensions;
using UnityEngine;

namespace Tetris.Game
{
    public class Tetromino
    {
        private readonly TetrominoCoordinates _coordinates;
        private readonly Rotation _rotation;
        private readonly Grid _grid;

        private RotateDirection _rotateDirection = RotateDirection.Neutral;
        private readonly CellColor _color;

        private IList<Vector2Int> _ghostCoordinates = new List<Vector2Int>();

        public CellColor Color => _color;
        public IList<Vector2Int> Coordinates => _coordinates.CurrentCoordinates;


        public Tetromino(TetrominoData data, int xOffset, int yOffset, Grid grid)
        {
            _color = data.Color;
            _rotation = new Rotation(data.RotationOffsets);
            _coordinates = new TetrominoCoordinates(data.Coordinates, xOffset, yOffset);
            _grid = grid;
            ChangePosition(_coordinates.CurrentCoordinates);
        }

        public bool Move(int x, int y)
        {
            var movedCoordinates = _coordinates.GetMovedCoordinates(x, y);
            return ChangePosition(movedCoordinates);
        }

        public void SuperRotate(bool isClockwise)
        {
            var diffs = _rotation.GetOffsetsDiff(_rotateDirection, _rotateDirection.GetNext(isClockwise));
            foreach (var diff in diffs)
            {
                var rotateCoordinates = _coordinates.GetRotateCoordinates(isClockwise, diff);
                if (ChangePosition(rotateCoordinates))
                {
                    _rotateDirection = _rotateDirection.GetNext(isClockwise);
                    return;
                }
            }
        }

        public void HardDrop()
        {
            ChangePosition(_ghostCoordinates);
        }

        private bool ChangePosition(IList<Vector2Int> newCoordinates)
        {
            if (_grid.PositionChange(Coordinates, newCoordinates, _color))
            {
                _coordinates.UpdateCoordinates(newCoordinates);
                _grid.ClearGhost(_ghostCoordinates);
                _ghostCoordinates = GetGhostPosition();
                _grid.UpdateGhost(_ghostCoordinates);

                return true;
            }

            return false;
        }

        private IList<Vector2Int> GetGhostPosition()
        {
            var verticalDrop = 0;

            while (_grid.CanMove(_coordinates.GetMovedCoordinates(0, verticalDrop - 1)))
            {
                verticalDrop--;
            }

            return _coordinates.GetMovedCoordinates(0, verticalDrop);
        }
    }

    public class TetrominoCoordinates
    {
        private IList<Vector2Int> _currentCoordinates;
        public IList<Vector2Int> CurrentCoordinates => _currentCoordinates;
        private Vector2Int First => _currentCoordinates[0];

        public TetrominoCoordinates(List<Vector2Int> initialCoordinates, int xOffset, int yOffset)
        {
            var adjustedCoordinates = initialCoordinates
                .Select(coordinate => coordinate + new Vector2Int(xOffset, yOffset));

            _currentCoordinates = new List<Vector2Int>(adjustedCoordinates);
        }

        public List<Vector2Int> GetMovedCoordinates(int x, int y)
        {
            var movedCoordinates = _currentCoordinates
                .Select(coordinate => coordinate + new Vector2Int(x, y));
            return new List<Vector2Int>(movedCoordinates);
        }

        public List<Vector2Int> GetRotateCoordinates(bool isClockWise, Vector2Int offset)
        {
            List<Vector2Int> rotatedCoordinates = new();
            foreach (var coordinate in _currentCoordinates)
            {
                var relativeCoordinate = coordinate - First;
                var rotatedCoordinate = relativeCoordinate.Rotate(isClockWise);
                rotatedCoordinate += First + offset;
                rotatedCoordinates.Add(rotatedCoordinate);
            }

            return rotatedCoordinates;
        }

        public void UpdateCoordinates(IList<Vector2Int> newCoordinates)
        {
            _currentCoordinates = newCoordinates;
        }
    }

    public class Rotation
    {
        private readonly Dictionary<RotateDirection, RotationOffset> _rotationOffsets = new();

        public Rotation(List<RotationOffset> rotationOffsetList)
        {
            foreach (var rotationOffset in rotationOffsetList)
            {
                _rotationOffsets[rotationOffset.RotateDirection] = rotationOffset;
            }
        }

        public List<Vector2Int> GetOffsetsDiff(RotateDirection from, RotateDirection to)
        {
            var fromRotationOffset = _rotationOffsets[from];
            var toRotationOffset = _rotationOffsets[to];

            var result = new List<Vector2Int>(fromRotationOffset.Offsets.Count);

            for (int i = 0; i < fromRotationOffset.Offsets.Count; i++)
            {
                var offsetDifference = fromRotationOffset.Offsets[i] - toRotationOffset.Offsets[i];
                result.Add(offsetDifference);
            }

            return result;
        }
    }

    public enum TetrominoType
    {
        I, S, J,
        T, L, Z,
        O
    }

    public enum CellColor
    {
        Empty, Blue, Cyan,
        Ghost, Green, Orange,
        Purple, Red, Yellow,
        Grey,
    }


    [Serializable]
    public class RotationOffset
    {
        public RotateDirection RotateDirection;
        public List<Vector2Int> Offsets;

        public RotationOffset(RotateDirection rotateDirection, List<Vector2Int> offsets)
        {
            RotateDirection = rotateDirection;
            Offsets = offsets;
        }
    }

}