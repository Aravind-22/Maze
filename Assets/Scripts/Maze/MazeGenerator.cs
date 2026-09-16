using System.Collections.Generic;
using UnityEngine;

namespace MazeGame.Generation
{
    public enum EdgeDir { East, South }

    public struct WallEdge
    {
        public int r, c;
        public EdgeDir dir;
    }

    public class MazeGenerator
    {
        public readonly int rows, cols;

        // horizontalWalls[r, c] = wall between cell(r,c) and cell(r,c+1). Size [rows, cols-1].
        public bool[,] horizontalWalls;
        // verticalWalls[r, c] = wall between cell(r,c) and cell(r+1,c). Size [rows-1, cols].
        public bool[,] verticalWalls;

        public MazeGenerator(MazeConfig cfg, System.Random rng)
        {
            rows = cfg.rows;
            cols = cfg.cols;

            horizontalWalls = new bool[rows, Mathf.Max(cols - 1, 0)];
            verticalWalls = new bool[Mathf.Max(rows - 1, 0), cols];
            SetAll(horizontalWalls, true);
            SetAll(verticalWalls, true);

            var uf = new UnionFind(rows * cols);
            var edges = BuildAllEdges();
            Shuffle(edges, rng);

            foreach (var e in edges)
            {
                int a = Index(e.r, e.c);
                int b = e.dir == EdgeDir.East ? Index(e.r, e.c + 1) : Index(e.r + 1, e.c);
                if (uf.Union(a, b))
                    KnockDown(e);
            }

            if (cfg.loopChance > 0f)
                AddLoops(cfg, rng);
        }

        public bool IsOpen(int r, int c, EdgeDir dir)
        {
            return dir == EdgeDir.East ? !horizontalWalls[r, c] : !verticalWalls[r, c];
        }

        int Index(int r, int c) => r * cols + c;

        List<WallEdge> BuildAllEdges()
        {
            var edges = new List<WallEdge>();
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (c < cols - 1) edges.Add(new WallEdge { r = r, c = c, dir = EdgeDir.East });
                    if (r < rows - 1) edges.Add(new WallEdge { r = r, c = c, dir = EdgeDir.South });
                }
            }
            return edges;
        }

        void KnockDown(WallEdge e)
        {
            if (e.dir == EdgeDir.East) horizontalWalls[e.r, e.c] = false;
            else verticalWalls[e.r, e.c] = false;
        }

        // Adds a small number of extra connections beyond the spanning tree so higher levels
        // have real loops/false paths instead of a single unique solution.
        void AddLoops(MazeConfig cfg, System.Random rng)
        {
            var edges = BuildAllEdges();
            Shuffle(edges, rng);

            foreach (var e in edges)
            {
                bool alreadyOpen = e.dir == EdgeDir.East ? !horizontalWalls[e.r, e.c] : !verticalWalls[e.r, e.c];
                if (alreadyOpen) continue;
                if (rng.NextDouble() > cfg.loopChance) continue;

                KnockDown(e);
            }
        }

        static void SetAll(bool[,] arr, bool value)
        {
            for (int i = 0; i < arr.GetLength(0); i++)
                for (int j = 0; j < arr.GetLength(1); j++)
                    arr[i, j] = value;
        }

        static void Shuffle(List<WallEdge> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
