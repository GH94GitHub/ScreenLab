using ScreenLab.Extensions;
using ScreenLab.Models;
using System.Drawing.Imaging;
using static ScreenLab.Models.Directions;

namespace ScreenLab.Screen.Regions
{
    public class RegionColorAnalyzer : RegionAnalyzer, IRegionAnalyzerFactory<RegionColorAnalyzer>, IRegionColorAnalyzer
    {
        private Color _selectionColor;

        public RegionColorAnalyzer(string name) 
            : base(name)
        {
        }

        public RegionColorAnalyzer(string name, Rectangle selection) 
            : base(name, selection)
        {
        }

        public RegionColorAnalyzer(string name, Color color) 
            : base(name)
        {
            _selectionColor = color;
        }

        public RegionColorAnalyzer(string name, Rectangle selection, Color color) 
            : base(name, selection)
        {
            _selectionColor = color;
        }

        /// <summary>
        /// Captures screenshot of region.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If no selection exists</exception>
        /// <exception cref="Exception"></exception>
        public Bitmap GetBitmap()
        {
            if (!HasSelection)
                throw new InvalidOperationException("No selection exists");

            try
            {
                Bitmap bitmap = new Bitmap(Selection.Width, Selection.Height);

                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Selection.Location, Point.Empty, Selection.Size);
                }
                return bitmap;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to Capture Screen Region", e);
            }
        }

        /// <summary>
        /// Computes the average color in a given screen region.
        /// </summary>
        public Color GetAverageColor()
        {
            using (Bitmap bmp = GetBitmap())
            {
                long r = 0, g = 0, b = 0;
                int totalPixels = Selection.Width * Selection.Height;

                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        r += pixel.R;
                        g += pixel.G;
                        b += pixel.B;
                    }
                }

                return Color.FromArgb((int)(r / totalPixels), (int)(g / totalPixels), (int)(b / totalPixels));
            }
        }

        /// <summary>
        /// Finds the most dominant color in a screen region.
        /// </summary>
        public Color GetDominantColor()
        {
            using (Bitmap bmp = GetBitmap())
            {
                Dictionary<Color, int> colorCount = new Dictionary<Color, int>();

                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        Color pixel = bmp.GetPixel(x, y);
                        if (colorCount.ContainsKey(pixel))
                            colorCount[pixel]++;
                        else
                            colorCount[pixel] = 1;
                    }
                }

                return colorCount.OrderByDescending(kv => kv.Value).First().Key;
            }
        }
        public Dictionary<Color, int> GetColorCount()
        {
            using (Bitmap bmp = GetBitmap())
            {
                Dictionary<Color, int> colorCount = new Dictionary<Color, int>();

                // Lock the bitmap's bits for direct access.
                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* ptr = (byte*)bmpData.Scan0;
                    int stride = bmpData.Stride;

                    for (int y = 0; y < bmp.Height; y++)
                    {
                        byte* row = ptr + y * stride;
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            byte b = row[x * 4 + 0];
                            byte g = row[x * 4 + 1];
                            byte r = row[x * 4 + 2];
                            byte a = row[x * 4 + 3];

                            Color pixel = Color.FromArgb(a, r, g, b);

                            if (colorCount.ContainsKey(pixel))
                                colorCount[pixel]++;
                            else
                                colorCount[pixel] = 1;
                        }
                    }
                }

                bmp.UnlockBits(bmpData);
                return colorCount;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="region"></param>
        /// <param name="targetColor"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public Point? FindColorInRegion(Color targetColor, int tolerance = 10)
        {
            using (Bitmap bmp = GetBitmap())
            {
                int centerX = bmp.Width / 2;
                int centerY = bmp.Height / 2;

                // Directions: Right, Down, Left, Up
                int[] dx = { 1, 0, -1, 0 };
                int[] dy = { 0, 1, 0, -1 };

                // Spiral search pattern
                int step = 1; // Initial step size
                int x = centerX, y = centerY; // Start at center
                int direction = 0; // Start moving right

                while (step <= Math.Max(bmp.Width, bmp.Height)) // Expand outward
                {
                    for (int i = 0; i < 2; i++) // Two movements per layer (increasing range)
                    {
                        for (int j = 0; j < step; j++)
                        {
                            if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                            {
                                Color pixel = bmp.GetPixel(x, y);
                                if (pixel.IsColorMatch(targetColor, tolerance))
                                {
                                    return new Point(Selection.X + x, Selection.Y + y);
                                }
                            }
                            x += dx[direction];
                            y += dy[direction];
                        }
                        direction = (direction + 1) % 4; // Change direction
                    }
                    step++; // Increase step size every full cycle
                }
            }
            return null; // No match found
        }
        public Point? FindClosestMatchingColor(Point referencePoint, Color targetColor, int tolerance = 10)
        {
            if (!HasSelection)
                BeginSelection();

            if (!Selection.Contains(referencePoint))
                return null;

            using (Bitmap bmp = GetBitmap())
            {
                int localX = referencePoint.X - Selection.X;
                int localY = referencePoint.Y - Selection.Y;

                if (localX < 0 || localX >= bmp.Width || localY < 0 || localY >= bmp.Height)
                    return null;

                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int stride = data.Stride;
                nint scan0 = data.Scan0;

                Queue<Point> queue = new Queue<Point>();
                HashSet<Point> visited = new HashSet<Point>();

                Point start = new Point(localX, localY);
                queue.Enqueue(start);
                visited.Add(start);

                int[] dx = { 1, -1, 0, 0 };
                int[] dy = { 0, 0, 1, -1 };

                unsafe
                {
                    byte* ptr = (byte*)scan0;

                    while (queue.Count > 0)
                    {
                        Point p = queue.Dequeue();
                        int x = p.X;
                        int y = p.Y;

                        if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height)
                            continue;

                        byte* pixel = ptr + y * stride + x * 4;

                        Color current = Color.FromArgb(pixel[2], pixel[1], pixel[0]);

                        if (current.IsColorMatch(targetColor, tolerance))
                        {
                            bmp.UnlockBits(data);
                            return new Point(Selection.X + x, Selection.Y + y);
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            Point neighbor = new Point(x + dx[i], y + dy[i]);
                            if (!visited.Contains(neighbor) &&
                                neighbor.X >= 0 && neighbor.Y >= 0 &&
                                neighbor.X < bmp.Width && neighbor.Y < bmp.Height)
                            {
                                queue.Enqueue(neighbor);
                                visited.Add(neighbor);
                            }
                        }
                    }
                }

                bmp.UnlockBits(data);
            }

            return null;
        }
        public List<Point> FindClosestMatchingColors(Point referencePoint, Color targetColor, int maxClosestCount, int tolerance = 10)
        {
            var results = new List<Point>();

            if (!HasSelection)
                BeginSelection();

            if (!Selection.Contains(referencePoint))
                return results;

            using (Bitmap bmp = GetBitmap())
            {
                int localX = referencePoint.X - Selection.X;
                int localY = referencePoint.Y - Selection.Y;

                if (localX < 0 || localX >= bmp.Width || localY < 0 || localY >= bmp.Height)
                    return results;

                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int stride = data.Stride;
                nint scan0 = data.Scan0;

                Queue<Point> queue = new Queue<Point>();
                HashSet<Point> visited = new HashSet<Point>();

                queue.Enqueue(new Point(localX, localY));
                visited.Add(new Point(localX, localY));

                int[] dx = { 1, -1, 0, 0 };
                int[] dy = { 0, 0, 1, -1 };

                unsafe
                {
                    byte* ptr = (byte*)scan0;

                    while (queue.Count > 0 && results.Count < maxClosestCount)
                    {
                        Point p = queue.Dequeue();
                        int x = p.X;
                        int y = p.Y;

                        if (x < 0 || y < 0 || x >= bmp.Width || y >= bmp.Height)
                            continue;

                        byte* pixel = ptr + y * stride + x * 4;
                        Color color = Color.FromArgb(pixel[2], pixel[1], pixel[0]);

                        if (color.IsColorMatch(targetColor, tolerance))
                        {
                            // Convert back to screen coordinates
                            results.Add(new Point(Selection.X + x, Selection.Y + y));
                        }

                        for (int i = 0; i < 4; i++)
                        {
                            int nx = x + dx[i];
                            int ny = y + dy[i];
                            Point neighbor = new Point(nx, ny);

                            if (!visited.Contains(neighbor) &&
                                nx >= 0 && ny >= 0 &&
                                nx < bmp.Width && ny < bmp.Height)
                            {
                                queue.Enqueue(neighbor);
                                visited.Add(neighbor);
                            }
                        }
                    }
                }

                bmp.UnlockBits(data);
            }

            return results;
        }

        public List<Point> GetAllMatchingPoints(Color targetColor, int tolerance = 10)
        {
            var matchingPoints = new List<Point>();

            if (!HasSelection)
                BeginSelection();

            using (Bitmap bmp = GetBitmap())
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                int stride = data.Stride;
                nint scan0 = data.Scan0;
                unsafe
                {
                    byte* ptr = (byte*)scan0;

                    for (int y = 0; y < bmp.Height; y++)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            byte* pixel = ptr + y * stride + x * 4;
                            Color color = Color.FromArgb(pixel[2], pixel[1], pixel[0]);

                            if (color.IsColorMatch(targetColor, tolerance))
                                matchingPoints.Add(new Point(Selection.X + x, Selection.Y + y));
                        }
                    }
                }

                bmp.UnlockBits(data);
            }

            return matchingPoints;
        }
        public List<Point> GetAllMatchingPoints(Point referencePoint, Color targetColor, Directions.Direction[] directions, int tolerance = 10)
        {
            var matchingPoints = new List<Point>();

            if (!HasSelection)
                BeginSelection();

            using (Bitmap bmp = GetBitmap())
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    int stride = data.Stride;
                    nint scan0 = data.Scan0;

                    unsafe
                    {
                        byte* ptr = (byte*)scan0;

                        for (int y = 0; y < bmp.Height; y++)
                        {
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                Point screenPoint = new Point(Selection.X + x, Selection.Y + y);

                                bool isInAnyCone = directions.Any(dir => IsInDirectionalCone(referencePoint, screenPoint, dir));
                                if (!isInAnyCone)
                                    continue;

                                byte* pixel = ptr + y * stride + x * 4;

                                byte b = pixel[0];
                                byte g = pixel[1];
                                byte r = pixel[2];
                                byte a = pixel[3];

                                if (a == 0) continue;

                                Color color = Color.FromArgb(a, r, g, b);
                                if (color.IsColorMatch(targetColor, tolerance))
                                    matchingPoints.Add(screenPoint);
                            }
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
            }

            return matchingPoints;

        }
        public List<Point> GetAllMatchingPoints(
            Point referencePoint,
            Color targetColor,
            Directions.Direction[] directions,
            int tolerance = 10,
            double proximity = double.MaxValue)
        {
            var matchingPoints = new List<Point>();
            if (!HasSelection)
                BeginSelection();

            using (Bitmap bmp = GetBitmap())
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    int stride = data.Stride;
                    nint scan0 = data.Scan0;
                    double proximitySquared = proximity * proximity;

                    // Direction cache per pixel
                    Dictionary<Point, Directions.Direction?> directionMap = new();

                    unsafe
                    {
                        byte* ptr = (byte*)scan0;

                        for (int y = 0; y < bmp.Height; y++)
                        {
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                Point screenPoint = new Point(Selection.X + x, Selection.Y + y);

                                int dx = screenPoint.X - referencePoint.X;
                                int dy = screenPoint.Y - referencePoint.Y;
                                double distanceSquared = dx * dx + dy * dy;

                                if (distanceSquared > proximitySquared)
                                    continue;

                                if (!directionMap.TryGetValue(screenPoint, out var dir))
                                {
                                    dir = DirectionFromPoint(referencePoint, screenPoint);
                                    directionMap[screenPoint] = dir;
                                }

                                if (!dir.HasValue || !directions.Contains(dir.Value))
                                    continue;

                                byte* pixel = ptr + y * stride + x * 4;
                                byte b = pixel[0], g = pixel[1], r = pixel[2], a = pixel[3];
                                if (a == 0) continue;

                                Color color = Color.FromArgb(a, r, g, b);
                                if (color.IsColorMatch(targetColor, tolerance))
                                    matchingPoints.Add(screenPoint);
                            }
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
            }

            return matchingPoints;
        }

        public Directions.Direction? DirectionFromPoint(Point referencePoint, Point target)
        {
            int dx = target.X - referencePoint.X;
            int dy = target.Y - referencePoint.Y;

            if (dx == 0 && dy == 0)
                return null;

            double angle = Math.Atan2(-dy, dx) * (180.0 / Math.PI); // -dy: Y axis down to up
            if (angle < 0)
                angle += 360;

            if (angle >= 337.5 || angle < 22.5) return Directions.Direction.Right;
            if (angle < 67.5) return Directions.Direction.UpRight;
            if (angle < 112.5) return Directions.Direction.Up;
            if (angle < 157.5) return Directions.Direction.UpLeft;
            if (angle < 202.5) return Directions.Direction.Left;
            if (angle < 247.5) return Directions.Direction.DownLeft;
            if (angle < 292.5) return Directions.Direction.Down;
            if (angle < 337.5) return Directions.Direction.DownRight;

            return null;
        }
        public bool IsInDirectionalCone(Point referencePoint, Point target, Direction direction, double coneThresholdCos = 0.707) // ~45°
        {
            if (!DirectionVectors.TryGetValue(direction, out var dirVec))
                return false;

            int dx = target.X - referencePoint.X;
            int dy = target.Y - referencePoint.Y;

            if (dx == 0 && dy == 0) return false;

            double len = Math.Sqrt(dx * dx + dy * dy);
            double dirLen = Math.Sqrt(dirVec.dx * dirVec.dx + dirVec.dy * dirVec.dy);

            double dot = (dx * dirVec.dx + dy * dirVec.dy) / (len * dirLen); // Cosine of angle

            return dot >= coneThresholdCos; // e.g. cos(45°) = ~0.707
        }
        new public static RegionColorAnalyzer Create(string name, Rectangle region) => new(name, region);

    }
}
