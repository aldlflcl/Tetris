using System.Collections.Generic;
using Tetris.GamePlay;
using Tetris.Game;
using Unity.Netcode;
using UnityEngine;

namespace Tetris.Network
{
    public class ClientTetrominoNextView : NetworkBehaviour
    {
        [SerializeField] private TetrominoFactory _tetrominoFactory;
        [SerializeField] private CellColorSpritesData _spritesData;

        private TetrominoType[] _nextTetromino;
        private CellView[][,] _cellViews;

        private int _start;
        private int _length;

        private void Awake() => enabled = false;

        public override void OnNetworkSpawn()
        {
            if (!IsClient)
            {
                return;
            }

            enabled = true;
        }

        public void Initialize(List<TetrominoType> tetrominoTypes)
        {
            _length = 4;
            _nextTetromino = new TetrominoType[_length];
            _cellViews = new CellView[_length][,];

            for (int i = 0; i < _length; i++)
            {
                _cellViews[i] = GenerateCellView(i);
            }

            foreach (var tetrominoType in tetrominoTypes)
            {
                PushBackWithNoViewUpdate(tetrominoType);
            }

            UpdateViewList();
        }

        public void PushBack(TetrominoType tetrominoType)
        {
            PushBackWithNoViewUpdate(tetrominoType);
            UpdateViewList();
        }

        private void PushBackWithNoViewUpdate(TetrominoType tetrominoType)
        {
            _nextTetromino[_start] = tetrominoType;
            _start = (_start + 1) % _length;
        }

        private void UpdateViewList()
        {
            for (int i = 0; i < _length; i++)
            {
                int index = (_start + i) % _length;
                var next = _nextTetromino[index];
                EraseCellView(_cellViews[i]);
                FillCellView(_cellViews[i], next);
            }
        }

        private void EraseCellView(CellView[,] cells)
        {
            foreach (var cell in cells)
            {
                cell.ChangeSprite(CellColor.Empty);
            }
        }

        private void FillCellView(CellView[,] cells, TetrominoType type)
        {
            var tetrominoData = _tetrominoFactory.GetTetrominoData(type);
            foreach (var coordinate in tetrominoData.Coordinates)
            {
                cells[coordinate.x + 1, coordinate.y + 1].ChangeSprite(tetrominoData.Color);
            }
        }

        private CellView[,] GenerateCellView(int index)
        {
            var cells = new CellView[4, 4];
            var offset = Vector2.down * (3 * index);
            for (int y = 0; y < cells.GetLength(1); y++)
            {
                for (int x = 0; x < cells.GetLength(0); x++)
                {
                    var cell = new GameObject($"Cell {x} / {y}").AddComponent<CellView>();

                    var positionX = x + offset.x;

                    var positionY = y + offset.y;

                    cell.Initialize(transform, new Vector2(positionX, positionY), _spritesData);
                    cells[x, y] = cell;
                }
            }

            return cells;
        }

    }
}