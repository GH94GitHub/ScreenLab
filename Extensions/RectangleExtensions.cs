namespace ScreenLab.Extensions
{
    public static class RectangleExtensions
    {
        public static Rectangle UnionAll(this IEnumerable<Rectangle> rectangles)
        {
            if (rectangles == null || !rectangles.Any())
                return Rectangle.Empty;

            using var enumerator = rectangles.GetEnumerator();
            enumerator.MoveNext();
            Rectangle union = enumerator.Current;

            while (enumerator.MoveNext())
            {
                union = Rectangle.Union(union, enumerator.Current);
            }

            return union;
        }
        public static List<Rectangle> SubtractRectangle(this Rectangle source, Rectangle toRemove)
        {
            var result = new List<Rectangle>();

            // Intersection area
            var intersect = Rectangle.Intersect(source, toRemove);
            if (intersect.IsEmpty)
            {
                result.Add(source); // Nothing to subtract
                return result;
            }

            // Top segment
            if (intersect.Top > source.Top)
            {
                result.Add(new Rectangle(
                    source.Left,
                    source.Top,
                    source.Width,
                    intersect.Top - source.Top
                ));
            }

            // Bottom segment
            if (intersect.Bottom < source.Bottom)
            {
                result.Add(new Rectangle(
                    source.Left,
                    intersect.Bottom,
                    source.Width,
                    source.Bottom - intersect.Bottom
                ));
            }

            // Left segment
            if (intersect.Left > source.Left)
            {
                int top = Math.Max(source.Top, intersect.Top);
                int bottom = Math.Min(source.Bottom, intersect.Bottom);
                result.Add(new Rectangle(
                    source.Left,
                    top,
                    intersect.Left - source.Left,
                    bottom - top
                ));
            }

            // Right segment
            if (intersect.Right < source.Right)
            {
                int top = Math.Max(source.Top, intersect.Top);
                int bottom = Math.Min(source.Bottom, intersect.Bottom);
                result.Add(new Rectangle(
                    intersect.Right,
                    top,
                    source.Right - intersect.Right,
                    bottom - top
                ));
            }

            return result;
        }
    }
}
