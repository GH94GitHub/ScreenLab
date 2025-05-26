namespace ScreenLab.Models
{
    public static class Directions
    {
        public enum Direction
        {
            Right = 0, Down = 1, Left = 2, Up = 3,
            UpRight = 4, DownRight = 5, DownLeft = 6, UpLeft = 7
        }

        public static readonly Dictionary<Direction, (int dx, int dy)> DirectionVectors = new()
        {
            { Direction.Right,     (1,  0) },
            { Direction.Left,     (-1,  0) },
            { Direction.Up,        (0, -1) },
            { Direction.Down,      (0,  1) },
            { Direction.UpRight,   (1, -1) },
            { Direction.DownRight, (1,  1) },
            { Direction.DownLeft, (-1,  1) },
            { Direction.UpLeft,   (-1, -1) },
        };
    }
}
