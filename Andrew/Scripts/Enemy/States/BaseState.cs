public abstract class BaseState
{
    // экземпляр класса NPC
    public Enemy enemy;
    public StateMachine stateMachine;
    // экземпляр класса StateMachine

    public abstract void Enter();
    public abstract void Perform();
    public abstract void Exit();
}