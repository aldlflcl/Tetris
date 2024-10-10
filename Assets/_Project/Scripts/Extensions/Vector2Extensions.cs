using UnityEngine;

namespace Tetris.Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2Int ToVector2Int(this Vector2 value) => new Vector2Int((int)value.x, (int)value.y);
    }

    public static class Vector2IntExtensions
    {
        private static readonly int[,] RotationMatrix = new int[2, 2]
        {
            { 0, -1 },
            { 1, 0 }
        };

        public static Vector2Int Rotate(this Vector2Int value, bool isClockWise = false)
        {
            int direction = isClockWise ? -1 : 1;
            int x = value.x;
            int y = value.y;
            int calculatedX = (RotationMatrix[0, 0] * x + RotationMatrix[0, 1] * y) * direction;
            int calculatedY = (RotationMatrix[1, 0] * x + RotationMatrix[1, 1] * y) * direction;
            
            return new Vector2Int(calculatedX, calculatedY);
        }

    }
}