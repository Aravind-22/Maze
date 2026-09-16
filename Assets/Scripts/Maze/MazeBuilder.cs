using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MazeGame.Generation
{
    // Wall/floor visuals are primitives with colliders removed - collision is math-driven via
    // MazeCollisionGrid, not Unity physics. TODO: mesh-combine static walls like Project 1's
    // DungeonMeshCombiner once the maze is stable, for draw-call reduction.
    public class MazeBuilder : MonoBehaviour
    {
        [SerializeField] Transform mazeRoot;
        [SerializeField] GameObject wallPrefab;   // simple cube primitive, no collider needed
        [SerializeField] GameObject floorPrefab;
        [SerializeField] GameObject holePrefab;
        [SerializeField] float wallHeight = 0.5f;
        [SerializeField] float wallThickness = 0.1f;

        public MazeCollisionGrid CollisionGrid { get; private set; }
        public Vector2Int StartCell { get; private set; }
        public Vector2Int EndCell { get; private set; }
        public Vector2 StartLocalPos { get; private set; }
        public Vector2 HoleLocalPos { get; private set; }

        Vector2 worldOffset;

        readonly List<GameObject> spawnedObjects = new List<GameObject>();

        public void Build(MazeGenerator maze, MazeConfig cfg)
        {
            Clear();
            this.transform.localEulerAngles = Vector3.zero;
            CollisionGrid = new MazeCollisionGrid(cfg.cellSize);

            float w = cfg.cols * cfg.cellSize;
            float h = cfg.rows * cfg.cellSize;
            worldOffset = new Vector2(-w * 0.5f, -h * 0.5f);

            StartCell = new Vector2Int(0, 0);
            EndCell = new Vector2Int(cfg.rows - 1, cfg.cols - 1);
            StartLocalPos = new Vector2((StartCell.y + 0.5f) * cfg.cellSize + worldOffset.x, (StartCell.x + 0.5f) * cfg.cellSize + worldOffset.y);
            HoleLocalPos = new Vector2((EndCell.y + 0.5f) * cfg.cellSize + worldOffset.x, (EndCell.x + 0.5f) * cfg.cellSize + worldOffset.y);
            Debug.Log("End pos : "+HoleLocalPos);

            SpawnFloor(cfg);
            SpawnPerimeterWalls(cfg);
            SpawnInteriorWalls(maze, cfg);
            SpawnHole();
            // CombineChildren(transform);
        }

        void SpawnFloor(MazeConfig cfg)
        {
            var floor = Instantiate(floorPrefab, mazeRoot);
            float w = cfg.cols * cfg.cellSize;
            float h = cfg.rows * cfg.cellSize;

            floor.transform.localScale = new Vector3(w, h, 1f);
            floor.transform.localPosition = Vector3.zero; //new Vector3(w / 2f, 0f, h / 2f);
            spawnedObjects.Add(floor);
        }

        void SpawnHole()
        {
            var hole = Instantiate(holePrefab, mazeRoot);
            hole.transform.localPosition = new Vector3(HoleLocalPos.x , -0.06f , HoleLocalPos.y);
            spawnedObjects.Add(hole);
        }

        void SpawnPerimeterWalls(MazeConfig cfg)
        {
            float w = cfg.cols * cfg.cellSize;
            float h = cfg.rows * cfg.cellSize;
            float t = wallThickness;

            SpawnWallSegment(new Vector3(w / 2f, 0, 0), new Vector3(w, wallHeight, t));
            SpawnWallSegment(new Vector3(w / 2f, 0, h), new Vector3(w, wallHeight, t));
            SpawnWallSegment(new Vector3(0, 0, h / 2f), new Vector3(t, wallHeight, h));
            SpawnWallSegment(new Vector3(w, 0, h / 2f), new Vector3(t, wallHeight, h));

            RegisterAABB(new Vector2(0, -t / 2f), new Vector2(w, t / 2f), cfg);
            RegisterAABB(new Vector2(0, h - t / 2f), new Vector2(w, h + t / 2f), cfg);
            RegisterAABB(new Vector2(-t / 2f, 0), new Vector2(t / 2f, h), cfg);
            RegisterAABB(new Vector2(w - t / 2f, 0), new Vector2(w + t / 2f, h), cfg);
        }

        void SpawnInteriorWalls(MazeGenerator maze, MazeConfig cfg)
        {
            float cs = cfg.cellSize;
            float t = wallThickness;

            for (int r = 0; r < maze.rows; r++)
            {
                for (int c = 0; c < maze.cols; c++)
                {
                    if (c < maze.cols - 1 && !maze.IsOpen(r, c, EdgeDir.East))
                    {
                        Vector3 pos = new Vector3((c + 1) * cs, 0, (r + 0.5f) * cs);
                        SpawnWallSegment(pos, new Vector3(t, wallHeight, cs));
                        RegisterAABB(
                            new Vector2((c + 1) * cs - t / 2f, r * cs),
                            new Vector2((c + 1) * cs + t / 2f, (r + 1) * cs), cfg);
                    }

                    if (r < maze.rows - 1 && !maze.IsOpen(r, c, EdgeDir.South))
                    {
                        Vector3 pos = new Vector3((c + 0.5f) * cs, 0, (r + 1) * cs);
                        SpawnWallSegment(pos, new Vector3(cs, wallHeight, t));
                        RegisterAABB(
                            new Vector2(c * cs, (r + 1) * cs - t / 2f),
                            new Vector2((c + 1) * cs, (r + 1) * cs + t / 2f), cfg);
                    }
                }
            }
        }

        void RegisterAABB(Vector2 min, Vector2 max, MazeConfig cfg)
        {
            min += worldOffset;
            max += worldOffset;
            var aabb = new WallAABB { min = min, max = max };

            int cMin = Mathf.FloorToInt(min.x / cfg.cellSize);
            int cMax = Mathf.FloorToInt(max.x / cfg.cellSize);
            int rMin = Mathf.FloorToInt(min.y / cfg.cellSize);
            int rMax = Mathf.FloorToInt(max.y / cfg.cellSize);

            for (int cx = cMin; cx <= cMax; cx++)
                for (int cy = rMin; cy <= rMax; cy++)
                    CollisionGrid.Register(new Vector2Int(cx, cy), aabb);
        }

        void SpawnWallSegment(Vector3 localPos, Vector3 scale)
        {
            var wall = Instantiate(wallPrefab, mazeRoot);
            wall.transform.localPosition = localPos + new Vector3(worldOffset.x, 0f, worldOffset.y);;
            wall.transform.localScale = scale;
            spawnedObjects.Add(wall);
        }

        void Clear()
        {
            for (int i = 0; i < spawnedObjects.Count; i++)
                DestroyImmediate(spawnedObjects[i]);
            spawnedObjects.Clear();
        }

        public void CombineChildren(Transform root)
        {
            var byMaterial = new Dictionary<Material, List<CombineInstance>>();
            var filters = root.GetComponentsInChildren<MeshFilter>();
            var toDestroy = new List<GameObject>();

            foreach (var filter in filters)
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                if (renderer == null) continue;

                var mat = renderer.sharedMaterial;
                if (!byMaterial.TryGetValue(mat, out var list))
                {
                    list = new List<CombineInstance>();
                    byMaterial[mat] = list;
                }

                list.Add(new CombineInstance
                {
                    mesh = filter.sharedMesh,
                    transform = filter.transform.localToWorldMatrix
                });

                var tileGO = filter.gameObject;
                bool hasCollider = tileGO.GetComponent<Collider>() != null;

                if (hasCollider)
                {
                    // Wall tile: keep the GameObject for its collider, strip only rendering
                    Destroy(renderer);
                    Destroy(filter);
                }
                else
                {
                    // Floor tile: nothing left worth keeping once the mesh is combined
                    toDestroy.Add(tileGO);
                }
            }

            foreach (var go in toDestroy)
                Destroy(go);

            foreach (var kvp in byMaterial)
            {
                var combinedGO = new GameObject($"Combined_{kvp.Key.name}");
                combinedGO.transform.SetParent(root, false);
                combinedGO.isStatic = true;

                var mf = combinedGO.AddComponent<MeshFilter>();
                var mr = combinedGO.AddComponent<MeshRenderer>();

                var combinedMesh = new Mesh
                {
                    indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
                };
                combinedMesh.CombineMeshes(kvp.Value.ToArray(), true, true);

                mf.sharedMesh = combinedMesh;
                mr.sharedMaterial = kvp.Key;

                spawnedObjects.Add(combinedGO);
            }
        }
    }
}
