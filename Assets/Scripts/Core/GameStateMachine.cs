namespace MazeGame.Generation
{
    public class GameStateMachine
    {
        public IGameState Current { get; private set; }

        public void ChangeState(IGameState next)
        {
            Current?.Exit();
            Current = next;
            Current?.Enter();
        }

        public void Tick(float deltaTime)
        {
            Current?.Tick(deltaTime);
        }
    }
}
