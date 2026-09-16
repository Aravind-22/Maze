using System.Collections.Generic;
using UnityEngine;

namespace MazeGame.Generation
{
    public class MazeSolver
    {
        public int SolvePathLength(MazeGenerator maze, Vector2Int start, Vector2Int end)
        {
            int rows = maze.rows, cols = maze.cols;
            var visited = new bool[rows, cols];
            var queue = new Queue<(Vector2Int pos, int dist)>();

            queue.Enqueue((start, 0));
            visited[start.x, start.y] = true;

            while (queue.Count > 0)
            {
                var (pos, dist) = queue.Dequeue();
                if (pos == end) return dist;

                foreach (var next in GetOpenNeighbors(maze, pos))
                {
                    if (visited[next.x, next.y]) continue;
                    visited[next.x, next.y] = true;
                    queue.Enqueue((next, dist + 1));
                }
            }

            return -1; // unreachable - shouldn't happen post-Kruskal, but guard against it anyway
        }

        IEnumerable<Vector2Int> GetOpenNeighbors(MazeGenerator maze, Vector2Int pos)
        {
            int r = pos.x, c = pos.y;

            if (c < maze.cols - 1 && maze.IsOpen(r, c, EdgeDir.East)) yield return new Vector2Int(r, c + 1);
            if (c > 0 && maze.IsOpen(r, c - 1, EdgeDir.East)) yield return new Vector2Int(r, c - 1);
            if (r < maze.rows - 1 && maze.IsOpen(r, c, EdgeDir.South)) yield return new Vector2Int(r + 1, c);
            if (r > 0 && maze.IsOpen(r - 1, c, EdgeDir.South)) yield return new Vector2Int(r - 1, c);
        }
    }
}
