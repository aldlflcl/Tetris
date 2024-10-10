using System.Collections.Generic;
using Tetris.GamePlay;
using Tetris.Game;
using Unity.Netcode;
using UnityEngine;

namespace Tetris.Network
{
    public class TetrominoNextView : NetworkBehaviour
    {
        private CellView[,] _first;
        private CellView[,] _second;
        private CellView[,] _third;
        private CellView[,] _fourth;
        private TetrominoFactory _tetrominoFactory;
        private CellColorSpritesData _spritesData;

        public override void OnNetworkSpawn()
        {
            if (!IsServer || !IsOwner)
            {
                enabled = false;
            }
        }

        private void GenerateCells(ref CellView[,] cells, Vector2 offset)
        {
            cells = new CellView[4, 4];
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
        }

        public void Initialize(CellColorSpritesData spritesData, TetrominoFactory tetrominoFactory
            )
        {
            _spritesData = spritesData;
            _tetrominoFactory = tetrominoFactory;
            
            GenerateCells(ref _first, Vector2.zero);
            GenerateCells(ref _second, Vector2.down * 3);
            GenerateCells(ref _third, Vector2.down * 6);
            GenerateCells(ref _fourth, Vector2.down * 9);
        }

        public void UpdateNextTetromino(IList<TetrominoType> next)
        {
            UpdateCellView(_first, next[0]);
            UpdateCellView(_second, next[1]);
            UpdateCellView(_third, next[2]);
            UpdateCellView(_fourth, next[3]);
        }

        private void UpdateCellView(CellView[,] cells, TetrominoType type)
        {
            EraseCellView(cells);
            FillCellView(cells, type);
        }

        private void FillCellView(CellView[,] cells, TetrominoType type)
        {
            var tetrominoData = _tetrominoFactory.GetTetrominoData(type);
            foreach (var coordinate in tetrominoData.Coordinates)
            {
                cells[coordinate.x + 1, coordinate.y + 1].ChangeSprite(tetrominoData.Color);
            }
        }

        private void EraseCellView(CellView[,] cells)
        {
            foreach (var cellView in cells)
            {
                cellView.ChangeSprite(CellColor.Empty);
            }
        }
    }
}