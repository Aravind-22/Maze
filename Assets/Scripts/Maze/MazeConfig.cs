using UnityEngine;

namespace MazeGame.Generation
{
    [CreateAssetMenu(menuName = "Maze/MazeConfig")]
    public class MazeConfig : ScriptableObject
    {
        [Header("Grid")]
        public int rows = 8;
        public int cols = 8;
        public float cellSize = 1f;

        [Header("Difficulty / Generation")]
        public int targetPathLength = 20;      // BFS solution length this maze must hit
        public int pathLengthTolerance = 2;
        [Range(0f, 0.3f)] public float loopChance = 0f; // braided-maze extra connections, higher levels only
        public int maxGenerationAttempts = 50;

        [Header("Tilt")]
        public float maxTiltAngle = 15f;
        public float maxTiltAngularSpeed = 40f;  // top speed the tilt can reach
        public float tiltAngularAccel = 60f;     // ramp-up rate while a direction is held
        public float tiltAngularDecel = 90f;     // ease-off rate on release (snappier than the ramp-up)

        [Header("Ball Sim")]
        public float ballRadius = 0.2f;
        public float gravityScale = 4f;
        public float friction = 0.6f;
        public float restitution = 0.4f;
        public float maxBallSpeed = 2.5f;        // hard cap, keeps the pace slow and controllable
        public float holeRadius = 0.25f;
    }
}
