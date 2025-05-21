using ScreenLab.Extensions;
using System.CodeDom;
using System.Drawing.Imaging;

namespace ScreenLab.Screen.Regions
{
    public class MultiRegionColorAnalyzer : MultiRegionAnalyzer, IRegionAnalyzerFactory<MultiRegionColorAnalyzer>, IRegionColorAnalyzer
    {
        public MultiRegionColorAnalyzer(string name) : base(name)
        {
        }

        public MultiRegionColorAnalyzer(string name, Rectangle selection) : base(name, selection)
        {
        }

        public MultiRegionColorAnalyzer(string name, Rectangle selection, params Rectangle[] exclusions) : base(name, selection, exclusions)
        {
        }

        public Color GetAverageColor()
        {
            using (Bitmap bmp = GetBitmap())
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    unsafe
                    {
                        byte* scan0 = (byte*)data.Scan0;
                        int stride = data.Stride;
                        long r = 0, g = 0, b = 0;
                        int validPixelCount = 0;

                        for (int y = 0; y < data.Height; y++)
                        {
                            byte* row = scan0 + (y * stride);
                            for (int x = 0; x < data.Width; x++)
                            {
                                int i = x * 4;
                                byte bVal = row[i + 0];
                                byte gVal = row[i + 1];
                                byte rVal = row[i + 2];
                                byte aVal = row[i + 3];

                                // Skip transparent pixels (excluded regions)
                                if (aVal == 0) continue;

                                r += rVal;
                                g += gVal;
                                b += bVal;
                                validPixelCount++;
                            }
                        }

                        if (validPixelCount == 0)
                            throw new InvalidOperationException("No pixels to average — region may be fully excluded");

                        return Color.FromArgb(
                            (int)(r / validPixelCount),
                            (int)(g / validPixelCount),
                            (int)(b / validPixelCount)
                        );
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
            }
        }
        public Bitmap GetBitmap()
        {
            if (!HasSelection)
                throw new InvalidOperationException("No selection exists");

            try
            {
                // Final image matches size of original selection
                Bitmap fullBitmap = new Bitmap(Selection.Width, Selection.Height);

                using (Graphics g = Graphics.FromImage(fullBitmap))
                {
                    // Clear background to transparent
                    g.Clear(Color.Transparent);

                    foreach (var region in GetEffectiveRegion())
                    {
                        if (region.Width == 0 || region.Height == 0)
                            continue;
                        // Size of the fragment
                        using Bitmap fragment = new Bitmap(region.Width, region.Height);

                        // Capture that screen fragment
                        using (Graphics fg = Graphics.FromImage(fragment))
                        {
                            fg.CopyFromScreen(region.Location, Point.Empty, region.Size);
                        }

                        // Draw it at the correct offset in the final bitmap
                        Point offset = new Point(region.X - Selection.X, region.Y - Selection.Y);
                        g.DrawImageUnscaled(fragment, offset);
                    }
                }
                
                return fullBitmap;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to capture effective screen region", e);
            }
        }
        public Color GetDominantColor()
        {
            using (Bitmap bmp = GetBitmap())
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    unsafe
                    {
                        byte* scan0 = (byte*)data.Scan0;
                        int stride = data.Stride;

                        Dictionary<int, int> colorCount = new(); // Use int to avoid boxing Color

                        for (int y = 0; y < data.Height; y++)
                        {
                            byte* row = scan0 + (y * stride);

                            for (int x = 0; x < data.Width; x++)
                            {
                                int i = x * 4;
                                byte bVal = row[i + 0];
                                byte gVal = row[i + 1];
                                byte rVal = row[i + 2];
                                byte aVal = row[i + 3];

                                if (aVal == 0) continue; // skip excluded pixels

                                // Pack ARGB into int
                                int argb = (aVal << 24) | (rVal << 16) | (gVal << 8) | bVal;

                                if (colorCount.ContainsKey(argb))
                                    colorCount[argb]++;
                                else
                                    colorCount[argb] = 1;
                            }
                        }

                        if (colorCount.Count == 0)
                            throw new InvalidOperationException("No visible pixels — region may be fully excluded");

                        int dominantArgb = colorCount.OrderByDescending(kv => kv.Value).First().Key;
                        return Color.FromArgb(dominantArgb);
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
            }
        }
        public Dictionary<Color, int> GetColorCount()
        {
            using (Bitmap bmp = GetBitmap()) // Already respects exclusions
            {
                Dictionary<int, int> colorCounts = new();

                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    unsafe
                    {
                        byte* ptr = (byte*)bmpData.Scan0;
                        int stride = bmpData.Stride;

                        for (int y = 0; y < bmp.Height; y++)
                        {
                            byte* row = ptr + y * stride;
                            for (int x = 0; x < bmp.Width; x++)
                            {
                                int index = x * 4;
                                byte b = row[index + 0];
                                byte g = row[index + 1];
                                byte r = row[index + 2];
                                byte a = row[index + 3];

                                if (a == 0) continue; // Skip excluded/transparent

                                int argb = (a << 24) | (r << 16) | (g << 8) | b;

                                if (colorCounts.ContainsKey(argb))
                                    colorCounts[argb]++;
                                else
                                    colorCounts[argb] = 1;
                            }
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(bmpData);
                }

                // Convert packed ARGB ints back to Color for return
                return colorCounts.ToDictionary(
                    kvp => Color.FromArgb(kvp.Key),
                    kvp => kvp.Value
                );
            }
        }
        public Point? FindColorInRegion(Color targetColor, int tolerance = 10)
        {
            using (Bitmap bmp = GetBitmap()) // already handles exclusions
            {
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    unsafe
                    {
                        byte* scan0 = (byte*)data.Scan0;
                        int stride = data.Stride;

                        int centerX = _selection.Width / 2;
                        int centerY = _selection.Height / 2;

                        int[] dx = { 1, 0, -1, 0 };
                        int[] dy = { 0, 1, 0, -1 };

                        int step = 1;
                        int x = centerX, y = centerY;
                        int direction = 0;

                        while (step <= Math.Max(bmp.Width, bmp.Height))
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                for (int j = 0; j < step; j++)
                                {
                                    if (x >= 0 && x < bmp.Width && y >= 0 && y < bmp.Height)
                                    {
                                        byte* row = scan0 + y * stride;
                                        int index = x * 4;
                                        byte b = row[index + 0];
                                        byte g = row[index + 1];
                                        byte r = row[index + 2];
                                        byte a = row[index + 3];

                                        if (a != 0) // skip excluded pixels
                                        {
                                            Color pixel = Color.FromArgb(a, r, g, b);
                                            if (pixel.IsColorMatch(targetColor, tolerance))
                                            {
                                                // Return global screen coordinate
                                                return new Point(Selection.X + x, Selection.Y + y);
                                            }
                                        }
                                    }

                                    x += dx[direction];
                                    y += dy[direction];
                                }
                                direction = (direction + 1) % 4;
                            }
                            step++;
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
            }

            return null;
        }
        public Point? FindClosestMatchingColor(Point referencePoint, Color targetColor, int tolerance = 10)
        {
            if (!HasSelection)
                BeginSelection();

            if (!Selection.Contains(referencePoint))
                return null;

            using (Bitmap bmp = GetBitmap()) // respects exclusions
            {
                int localX = referencePoint.X - Selection.X;
                int localY = referencePoint.Y - Selection.Y;

                if (localX < 0 || localX >= bmp.Width || localY < 0 || localY >= bmp.Height)
                    return null;

                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    int stride = data.Stride;
                    nint scan0 = data.Scan0;

                    Queue<Point> queue = new();
                    HashSet<Point> visited = new();

                    Point start = new(localX, localY);
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

                            byte* pixel = ptr + y * stride + x * 4;

                            byte b = pixel[0];
                            byte g = pixel[1];
                            byte r = pixel[2];
                            byte a = pixel[3];

                            if (a == 0) continue; // respect exclusions

                            Color current = Color.FromArgb(a, r, g, b);

                            if (current.IsColorMatch(targetColor, tolerance))
                                return new Point(Selection.X + x, Selection.Y + y);

                            for (int i = 0; i < 4; i++)
                            {
                                int nx = x + dx[i];
                                int ny = y + dy[i];
                                Point neighbor = new(nx, ny);

                                if (nx >= 0 && ny >= 0 && nx < bmp.Width && ny < bmp.Height && !visited.Contains(neighbor))
                                {
                                    queue.Enqueue(neighbor);
                                    visited.Add(neighbor);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
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

            using (Bitmap bmp = GetBitmap()) // already respects exclusions
            {
                int localX = referencePoint.X - Selection.X;
                int localY = referencePoint.Y - Selection.Y;

                if (localX < 0 || localX >= bmp.Width || localY < 0 || localY >= bmp.Height)
                    return results;

                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                try
                {
                    int stride = data.Stride;
                    nint scan0 = data.Scan0;

                    Queue<Point> queue = new();
                    HashSet<Point> visited = new();

                    Point start = new(localX, localY);
                    queue.Enqueue(start);
                    visited.Add(start);

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

                            byte* pixel = ptr + y * stride + x * 4;

                            byte b = pixel[0];
                            byte g = pixel[1];
                            byte r = pixel[2];
                            byte a = pixel[3];

                            if (a == 0) continue; // skip excluded

                            Color color = Color.FromArgb(a, r, g, b);

                            if (color.IsColorMatch(targetColor, tolerance))
                            {
                                results.Add(new Point(Selection.X + x, Selection.Y + y));
                            }

                            for (int i = 0; i < 4; i++)
                            {
                                int nx = x + dx[i];
                                int ny = y + dy[i];
                                Point neighbor = new(nx, ny);

                                if (nx >= 0 && ny >= 0 && nx < bmp.Width && ny < bmp.Height && !visited.Contains(neighbor))
                                {
                                    queue.Enqueue(neighbor);
                                    visited.Add(neighbor);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    bmp.UnlockBits(data);
                }
            }

            return results;
        }
        public List<Point> GetAllMatchingPoints(Color targetColor, int tolerance = 10)
        {
            var matchingPoints = new List<Point>();

            if (!HasSelection)
                BeginSelection();

            using (Bitmap bmp = GetBitmap()) // already respects exclusions
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
                                byte* pixel = ptr + y * stride + x * 4;

                                byte b = pixel[0];
                                byte g = pixel[1];
                                byte r = pixel[2];
                                byte a = pixel[3];

                                if (a == 0) continue; // skip excluded/transparent

                                Color color = Color.FromArgb(a, r, g, b);
                                if (color.IsColorMatch(targetColor, tolerance))
                                    matchingPoints.Add(new Point(Selection.X + x, Selection.Y + y));
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

        static MultiRegionColorAnalyzer IRegionAnalyzerFactory<MultiRegionColorAnalyzer>.Create(string name, Rectangle region) => new(name, region);
    }
}
