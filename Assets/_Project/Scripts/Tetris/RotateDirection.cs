using System;

namespace Tetris.Game
{
    public enum RotateDirection
    {
        Neutral = 0, ClockWise = 1, Reverse = 2,
        CounterClockWise = 3,
    }

    public static class RotationExtension
    {
        public static RotateDirection GetNext(this RotateDirection rotateDirection, bool isClockwise)
        {
            int direction = isClockwise ? 1 : -1;
            int length = Enum.GetValues(typeof(RotateDirection)).Length;
            int currentRotation = (int)rotateDirection;

            int nextRotation = ((currentRotation + direction) % length + length) % length;

            return (RotateDirection)nextRotation;
        }
    }
}