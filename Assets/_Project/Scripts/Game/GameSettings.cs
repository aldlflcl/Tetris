using UnityEngine;

namespace Tetris
{
    [CreateAssetMenu(menuName = "Tetris/GameSettings", fileName = "GameSettings")]
    public class GameSettings : ScriptableObject
    {
        public int GridWidth;
        public int GridHeight;
        public float CellSize;
        public float GameTick;
    }
}