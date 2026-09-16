using System.Collections.Generic;
using UnityEngine;

namespace MazeGame.Generation
{
    public class MazeCollisionGrid
    {
        readonly Dictionary<Vector2Int, List<WallAABB>> cellWalls = new Dictionary<Vector2Int, List<WallAABB>>();
        readonly float cellSize;

        public MazeCollisionGrid(float cellSize)
        {
            this.cellSize = cellSize;
        }

        public void Register(Vector2Int cell, WallAABB wall)
        {
            if (!cellWalls.TryGetValue(cell, out var list))
            {
                list = new List<WallAABB>();
                cellWalls[cell] = list;
            }
            list.Add(wall);
        }

        public Vector2Int CellOf(Vector2 localPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(localPos.x / cellSize),
                Mathf.FloorToInt(localPos.y / cellSize));
        }

        // Returns walls in the ball's current cell plus its 8 neighbors.
        public IEnumerable<WallAABB> GetNearbyWalls(Vector2 localPos)
        {
            Vector2Int center = CellOf(localPos);
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var key = new Vector2Int(center.x + dx, center.y + dy);
                    if (cellWalls.TryGetValue(key, out var list))
                    {
                        for (int i = 0; i < list.Count; i++)
                            yield return list[i];
                    }
                }
            }
        }
    }
}
