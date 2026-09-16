using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeGame.Generation
{
    public class MazeGameManager : MonoBehaviour
    {
        [SerializeField] MazeConfig baseConfig;
        [SerializeField] MazeBuilder builder;
        [SerializeField] MazeTiltController tiltController;
        [SerializeField] BallSimController ball;
        [SerializeField] Key regenerateKey = Key.F5;

        public int CurrentLevel { get; private set; } = 1;

        MazeConfig runtimeConfig;
        GameStateMachine machine;

        void Awake()
        {
            machine = new GameStateMachine();
        }

        void Start()
        {
            EnableTilt(false);
            machine.ChangeState(new ReadyState(this));
            GenerateLevel(CurrentLevel);
        }

        void Update()
        {
            machine.Tick(Time.deltaTime);

            if (Keyboard.current[regenerateKey].wasPressedThisFrame)
                RegenerateCurrentLevel();
        }

        public void GenerateLevel(int level)
        {
            CurrentLevel = Mathf.Clamp(level, 1, 10);

            runtimeConfig = ScriptableObject.Instantiate(baseConfig);
            MazeDifficultyCurve.Apply(runtimeConfig, CurrentLevel);

            var solver = new MazeSolver();
            MazeGenerator maze = null;
            int actualLength = -1;

            for (int attempt = 0; attempt < runtimeConfig.maxGenerationAttempts; attempt++)
            {
                var candidate = new MazeGenerator(runtimeConfig, new System.Random());
                var start = Vector2Int.zero;
                var end = new Vector2Int(runtimeConfig.rows - 1, runtimeConfig.cols - 1);
                int length = solver.SolvePathLength(candidate, start, end);

                maze = candidate;
                actualLength = length;

                if (Mathf.Abs(length - runtimeConfig.targetPathLength) <= runtimeConfig.pathLengthTolerance)
                    break; // good enough, stop early
            }

            builder.Build(maze, runtimeConfig);
            ball.Init(builder.CollisionGrid, builder.StartLocalPos, builder.HoleLocalPos);
            CameraManager.instance.SetCamPos(level);
            ball.OnFellInHole = HandleLevelWon;

            machine.ChangeState(new PlayingState(this));

            Debug.Log($"Level {CurrentLevel}: {runtimeConfig.rows}x{runtimeConfig.cols}, " +
                      $"target path {runtimeConfig.targetPathLength}, actual {actualLength}");
        }

        [ContextMenu("Regenerate Current Level")]
        public void RegenerateCurrentLevel() => GenerateLevel(CurrentLevel);

        void HandleLevelWon()
        {
            machine.ChangeState(new WonState(this));
        }

        public void AdvanceToNextLevel()
        {
            if (CurrentLevel >= 10)
            {
                Debug.Log("All 10 levels complete.");
                return;
            }
            GenerateLevel(CurrentLevel + 1);
        }

        public void EnableTilt(bool enable)
        {
            tiltController.SetActive(enable);
        }
    }
}
