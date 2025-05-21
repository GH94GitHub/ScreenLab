namespace ScreenLab.Extensions
{
    public static class CoordinateExtensions
    {
        public static Point? GetClosestPoint(this List<Point> otherPoints, Point reference, double? distanceTolerance = null)
        {
            if (otherPoints == null || !otherPoints.Any())
                return null;

            var pointDistances = otherPoints
                .Select(p => new { Point = p, SquaredDistance = Math.Pow(p.X - reference.X, 2) + Math.Pow(p.Y - reference.Y, 2) })
                .ToList();

            double minSquaredDistance = pointDistances.Min(p => p.SquaredDistance);

            // Filter all points at the minimum distance (handling ties)
            var closestPoints = pointDistances
                .Where(p => p.SquaredDistance == minSquaredDistance)
                .Select(p => p.Point)
                .ToList();

            // If distance tolerance is provided, check actual distance
            if (distanceTolerance.HasValue)
            {
                double actualDistance = Math.Sqrt(minSquaredDistance);
                if (actualDistance > distanceTolerance.Value)
                    return null;
            }

            // Return one point at random if there are ties
            Random rnd = new();
            return closestPoints[rnd.Next(closestPoints.Count)];
        }

        public static List<Point> FindClosestGroup(this List<List<Point>> groups, Point referencePoint)
        {
            if (groups == null || groups.Count == 0)
                throw new ArgumentException("Groups list is null or empty.");

            List<Point> closestGroup = null;
            double closestDistanceSquared = double.MaxValue;

            foreach (var group in groups)
            {
                if (group == null || group.Count == 0)
                    continue;

                // Middle point of the group
                Point centerPoint = group.GetCenterMostPoint();

                // Distance squared (avoids sqrt for better performance)
                double distanceSquared = Math.Pow(centerPoint.X - referencePoint.X, 2) + Math.Pow(centerPoint.Y - referencePoint.Y, 2);

                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestGroup = group;
                }
            }

            if (closestGroup == null || closestGroup.Count == 0)
                throw new InvalidOperationException("No valid closest group found.");

            return closestGroup;
        }

        public static List<List<Point>> OrderGroupsByProximity(this List<List<Point>> groups, Point referencePoint, bool ascendingOrder = true)
        {
            if (groups == null || groups.Count == 0)
                return new List<List<Point>>();

            // Order by the squared distance of each group's middle point to the reference point
            var orderedGroups = groups
                .Where(group => group.Count > 0);

            if (ascendingOrder)
            {
                orderedGroups = orderedGroups.OrderBy(group =>
                {
                    Point centerPoint = group[group.Count / 2];
                    int dx = centerPoint.X - referencePoint.X;
                    int dy = centerPoint.Y - referencePoint.Y;
                    return dx * dx + dy * dy; // Distance squared
                });
            }
            else
            {
                orderedGroups = orderedGroups.OrderByDescending(group =>
                {
                    Point centerPoint = group[group.Count / 2];
                    int dx = centerPoint.X - referencePoint.X;
                    int dy = centerPoint.Y - referencePoint.Y;
                    return dx * dx + dy * dy; // Distance squared
                });
            }

            return orderedGroups.ToList();
        }

        public static List<List<Point>> FindConnectedGoups(this List<Point> points, int filterMinCoords = 0, bool use8Directions = false)
        {
            var visited = new HashSet<Point>();
            var pointSet = new HashSet<Point>(points);
            var clusters = new List<List<Point>>();

            var directions = use8Directions
                ? new (int dx, int dy)[] { (-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (-1, 1), (1, -1), (1, 1) } // 8-directional
                : new (int dx, int dy)[] { (-1, 0), (1, 0), (0, -1), (0, 1) }; // 4-directional

            foreach (var start in points)
            {
                if (visited.Contains(start)) continue;

                var cluster = new List<Point>();
                var queue = new Queue<Point>();
                queue.Enqueue(start);
                visited.Add(start);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    cluster.Add(current);

                    foreach (var (dx, dy) in directions)
                    {
                        var neighbor = new Point(current.X + dx, current.Y + dy);
                        if (pointSet.Contains(neighbor) && !visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }

                clusters.Add(cluster);
            }

            return filterMinCoords > 0 ? clusters.Where((g) => g.Count > filterMinCoords).ToList() : clusters;
        }

        public static Point GetMiddleBottom(
            this List<Point> points,
            int yRangeTolerance = 10,
            int xRangeTolerance = 10)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("'points' is null or has no elements");

            // Step 1: Find the highest Y value (farthest down on screen)
            int maxY = points.Max(p => p.Y);

            // Step 2: Filter all points within Y range tolerance of max Y
            var bottomYPoints = points.Where(p => Math.Abs(p.Y - maxY) <= yRangeTolerance).ToList();

            if (bottomYPoints.Count == 0)
                throw new InvalidOperationException("'bottomYPoints' has no elements");

            // Step 3: Sort by X and find middle X
            var xSorted = bottomYPoints.OrderBy(p => p.X).ToList();
            int midX = xSorted[xSorted.Count / 2].X;

            // Step 4: Filter by X range tolerance
            var finalCandidates = bottomYPoints.Where(p => Math.Abs(p.X - midX) <= xRangeTolerance).ToList();

            // Step 5: Return one at random (or null if none)
            if (finalCandidates.Count == 0)
                throw new InvalidOperationException("'finalCandidates' has no elements");

            Random rnd = new();
            return finalCandidates[rnd.Next(finalCandidates.Count)];
        }

        public static Point GetMiddleTop(
            this List<Point> points,
            int yRangeTolerance = 10,
            int xRangeTolerance = 10)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("'points' is null or has no elements");

            int minY = points.Min(p => p.Y);
            var topPoints = points.Where(p => Math.Abs(p.Y - minY) <= yRangeTolerance).ToList();

            if (topPoints.Count == 0)
                throw new InvalidOperationException("'topPoints' has no elements");

            int midX = topPoints.OrderBy(p => p.X).ToList()[topPoints.Count / 2].X;
            var candidates = topPoints.Where(p => Math.Abs(p.X - midX) <= xRangeTolerance).ToList();

            return candidates.Count > 0 ? candidates[new Random().Next(candidates.Count)] : throw new InvalidOperationException("'candidates' has no elements"); ;
        }

        public static Point GetMiddleLeft(
            this List<Point> points,
            int xRangeTolerance = 10,
            int yRangeTolerance = 10)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("'points' is null or has no elements");

            int minX = points.Min(p => p.X);
            var leftPoints = points.Where(p => Math.Abs(p.X - minX) <= xRangeTolerance).ToList();

            if (leftPoints.Count == 0)
                throw new InvalidOperationException("'leftPoints' has no elements");

            int midY = leftPoints.OrderBy(p => p.Y).ToList()[leftPoints.Count / 2].Y;
            var candidates = leftPoints.Where(p => Math.Abs(p.Y - midY) <= yRangeTolerance).ToList();

            return candidates.Count > 0 ? candidates[new Random().Next(candidates.Count)] : throw new InvalidOperationException("'candidates' has no elements"); ;
        }

        public static Point GetMiddleRight(
            this List<Point> points,
            int xRangeTolerance = 10,
            int yRangeTolerance = 10)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("'points' is null or has no elements");

            int maxX = points.Max(p => p.X);
            var rightPoints = points.Where(p => Math.Abs(p.X - maxX) <= xRangeTolerance).ToList();

            if (rightPoints.Count == 0)
                throw new InvalidOperationException("'rightPoints' has no elements");

            int midY = rightPoints.OrderBy(p => p.Y).ToList()[rightPoints.Count / 2].Y;
            var candidates = rightPoints.Where(p => Math.Abs(p.Y - midY) <= yRangeTolerance).ToList();

            return candidates.Count > 0 ? candidates[new Random().Next(candidates.Count)] : throw new InvalidOperationException("'candidates' has no elements");
        }

        // Centroid - Returns point closest to center
        public static Point GetCenterMostPoint(this IEnumerable<Point> points)
        {
            if (points == null || !points.Any())
                throw new ArgumentException("'points' is null or has no elements");

            (double avgX,double avgY) = points.GetCenter();

            return points
                .OrderBy(p => Math.Pow(p.X - avgX, 2) + Math.Pow(p.Y - avgY, 2))
                .First();
        }

        // Centroid - Returns center of the points (may not be one of the initial points)
        public static (double avgX, double avgY) GetCenter(this IEnumerable<Point> points)
        {
            if (points == null || !points.Any())
                throw new ArgumentException("'points' is null or has no elements");

            double avgX = points.Average(p => p.X);
            double avgY = points.Average(p => p.Y);

            return (avgX, avgY);
        }

        public static Point GetTopLeft(this IEnumerable<Point> points)
        {
            if (points == null || points.Count() == 0)
                throw new ArgumentException("'points' is null or has no elements");

            int minX = points.Min(p => p.X);
            int minY = points.Min(p => p.Y);

            return points
                .OrderBy(p => Math.Pow(p.X - minX, 2) + Math.Pow(p.Y - minY, 2))
                .FirstOrDefault();
        }

        public static Point GetTopRight(this IEnumerable<Point> points)
        {
            if (points == null || points.Count() == 0)
                throw new ArgumentException("'points' is null or has no elements");

            int maxX = points.Max(p => p.X);
            int minY = points.Min(p => p.Y);

            return points
                .OrderBy(p => Math.Pow(p.X - maxX, 2) + Math.Pow(p.Y - minY, 2))
                .FirstOrDefault();
        }

        public static Point GetBottomLeft(this IEnumerable<Point> points)
        {
            if (points == null || points.Count() == 0)
                throw new ArgumentException("'points' is null or has no elements");

            int minX = points.Min(p => p.X);
            int maxY = points.Max(p => p.Y);

            return points
                .OrderBy(p => Math.Pow(p.X - minX, 2) + Math.Pow(p.Y - maxY, 2))
                .FirstOrDefault();
        }

        public static Point GetBottomRight(this IEnumerable<Point> points)
        {
            if (points == null || points.Count() == 0)
                throw new ArgumentException("'points' is null or has no elements");

            int maxX = points.Max(p => p.X);
            int maxY = points.Max(p => p.Y);

            return points
                .OrderBy(p => Math.Pow(p.X - maxX, 2) + Math.Pow(p.Y - maxY, 2))
                .FirstOrDefault();
        }

    }
}
