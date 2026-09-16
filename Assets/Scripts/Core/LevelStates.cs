using UnityEngine;

namespace MazeGame.Generation
{
    public class ReadyState : IGameState
    {
        readonly MazeGameManager manager;
        public ReadyState(MazeGameManager manager) { this.manager = manager; }

        public void Enter() => Debug.Log("ReadySate - Maze ready.");
        public void Exit() { }
        public void Tick(float deltaTime) { }
    }

    public class PlayingState : IGameState
    {
        readonly MazeGameManager manager;
        public PlayingState(MazeGameManager manager) { this.manager = manager; }

        public void Enter() 
        {
            manager.EnableTilt(true);
            Debug.Log($"PlayingState - Playing level {manager.CurrentLevel}");
        }
        public void Exit() { manager.EnableTilt(false); }
        public void Tick(float deltaTime) { }
    }

    public class WonState : IGameState
    {
        readonly MazeGameManager manager;
        const float transitionDelay = 1f;
        float timer;

        public WonState(MazeGameManager manager) { this.manager = manager; }

        public void Enter()
        {
            timer = 0f;
            Debug.Log($"wonState - Level {manager.CurrentLevel} complete!");
        }

        public void Exit() { }

        public void Tick(float deltaTime)
        {
            timer += deltaTime;
            if (timer >= transitionDelay)
                manager.AdvanceToNextLevel();
        }
    }
}
