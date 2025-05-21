using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenLab.Utility
{
    public class DbscanClustering
    {
        private readonly double _epsSquared;
        private readonly int _minPts;

        public DbscanClustering(double eps, int minPts)
        {
            if (eps <= 0)
                throw new ArgumentException("eps must be positive");
            if (minPts < 1)
                throw new ArgumentException("minPts must be at least 1");

            _epsSquared = eps * eps;
            _minPts = minPts;
        }

        public Dictionary<Point, int> Cluster(List<Point> points)
        {
            var clusters = new Dictionary<Point, int>();
            var visited = new HashSet<Point>();
            int clusterId = 0;

            foreach (var point in points)
            {
                if (visited.Contains(point))
                    continue;

                visited.Add(point);
                var neighbors = GetNeighbors(point, points);

                if (neighbors.Count < _minPts)
                {
                    clusters[point] = -1; // noise
                    continue;
                }

                clusterId++;
                clusters[point] = clusterId;

                var seedSet = new Queue<Point>(neighbors);
                while (seedSet.Count > 0)
                {
                    var q = seedSet.Dequeue();

                    if (!visited.Contains(q))
                    {
                        visited.Add(q);
                        var qNeighbors = GetNeighbors(q, points);
                        if (qNeighbors.Count >= _minPts)
                        {
                            foreach (var n in qNeighbors)
                            {
                                if (!seedSet.Contains(n))
                                    seedSet.Enqueue(n);
                            }
                        }
                    }

                    if (!clusters.ContainsKey(q))
                        clusters[q] = clusterId;
                }
            }

            return clusters;
        }

        public Dictionary<int, List<Point>> GetClusterGroups(List<Point> points)
        {
            var pointToCluster = Cluster(points);
            return pointToCluster
                .Where(kvp => kvp.Value != -1) // exclude noise
                .GroupBy(kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.Select(kvp => kvp.Key).ToList());
        }

        public List<Point> GetLargestCluster(List<Point> points)
        {
            var clusters = GetClusterGroups(points);
            return clusters
                .OrderByDescending(g => g.Value.Count)
                .FirstOrDefault().Value ?? new List<Point>();
        }

        public Point? GetCenterOfLargestCluster(List<Point> points)
        {
            var largestCluster = GetLargestCluster(points);
            if (largestCluster.Count == 0)
                return null;

            double centerX = largestCluster.Average(p => p.X);
            double centerY = largestCluster.Average(p => p.Y);

            return largestCluster
                .OrderBy(p => Math.Pow(p.X - centerX, 2) + Math.Pow(p.Y - centerY, 2))
                .First();
        }

        private List<Point> GetNeighbors(Point center, List<Point> points)
        {
            return points
                .Where(p => GetSquaredDistance(center, p) <= _epsSquared)
                .ToList();
        }

        private static double GetSquaredDistance(Point p1, Point p2)
        {
            int dx = p1.X - p2.X;
            int dy = p1.Y - p2.Y;
            return dx * dx + dy * dy;
        }
    }
}
