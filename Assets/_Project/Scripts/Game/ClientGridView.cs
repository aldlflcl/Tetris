using Tetris.GamePlay;
using Tetris.Game;
using Unity.Netcode;
using UnityEngine;

namespace Tetris.Network
{
    public class ClientGridView : NetworkBehaviour
    {
        private CellView[,] _cells;

        public override void OnNetworkSpawn()
        {
            if (!IsClient)
            {
                enabled = false;
            }
        }

        public void Initialize(GameSettings settings, CellColorSpritesData spritesData)
        {
            _cells = new CellView[settings.GridWidth, settings.GridHeight * 2];

            for (int y = 0; y < _cells.GetLength(1); y++)
            {
                for (int x = 0; x < _cells.GetLength(0); x++)
                {
                    var cell = new GameObject($"Cell {x} / {y}").AddComponent<CellView>();

                    var positionX =
                        x * settings.CellSize + settings.CellSize / 2 - settings.GridWidth * settings.CellSize / 2;

                    var positionY =
                        y * settings.CellSize + settings.CellSize / 2 - settings.GridHeight * settings.CellSize / 2;

                    cell.Initialize(transform, new Vector2(positionX, positionY), spritesData);
                    _cells[x, y] = cell;
                }
            }
        }

        // public void Initialize()
        // {
        //     var settings = GameManager.Instance.GameSettings;
        //     var spritesData = GameManager.Instance.CellColorSpritesData;
        //     _cells = new CellView[settings.GridWidth, settings.GridHeight * 2];
        //
        //     for (int y = 0; y < _cells.GetLength(1); y++)
        //     {
        //         for (int x = 0; x < _cells.GetLength(0); x++)
        //         {
        //             var cell = new GameObject($"Cell {x} / {y}").AddComponent<CellView>();
        //
        //             var positionX =
        //                 x * settings.CellSize + settings.CellSize / 2 - settings.GridWidth * settings.CellSize / 2;
        //
        //             var positionY =
        //                 y * settings.CellSize + settings.CellSize / 2 - settings.GridHeight * settings.CellSize / 2;
        //
        //             cell.Initialize(transform, new Vector2(positionX, positionY), spritesData);
        //             _cells[x, y] = cell;
        //         }
        //     }
        // }

        public void UpdateGridCellsColor(CellDto[] cells)
        {
            foreach (var cell in cells)
            {
                _cells[cell.X, cell.Y].ChangeSprite(cell.Color);
            }
        }
    }
}