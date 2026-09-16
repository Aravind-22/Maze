namespace MazeGame.Generation
{
    public interface IGameState
    {
        void Enter();
        void Exit();
        void Tick(float deltaTime);
    }
}
