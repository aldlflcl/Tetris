using System.Collections.Generic;
using UnityEngine;

namespace Tetris.Game
{
    [CreateAssetMenu(menuName = "Create Tetromino Data", fileName = "Tetromino Data")]
    public class TetrominoData : ScriptableObject
    {
        public TetrominoType Type;
        public CellColor Color;
        public List<Vector2Int> Coordinates;
        public List<RotationOffset> RotationOffsets;
    }
}