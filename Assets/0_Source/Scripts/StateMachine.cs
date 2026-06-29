public abstract class State
{
    public abstract void Enter();
    public abstract void Update(float deltaTime);
    public abstract void Exit();
}

public abstract class State<TState> : State where TState : State<TState>
{
    public override void Enter() => ((TState)this).OnEnter();
    public override void Update(float deltaTime) => ((TState)this).OnUpdate(deltaTime);
    public override void Exit() => ((TState)this).OnExit();

    protected abstract void OnEnter();
    protected abstract void OnUpdate(float deltaTime);
    protected abstract void OnExit();
}

public class StateMachine
{
    private State currentState;

    // Текущее состояние (для проверок снаружи, напр. можно ли кликать по НПС).
    public State CurrentState => currentState;

    public void ChangeState(State state)
    {
        if (state == null)
            throw new System.ArgumentNullException(nameof(state),
                "ChangeState ������� null: ��������� �� ������� � Awake().");

        currentState?.Exit();
        currentState = state;
        currentState.Enter();
    }

    public void Update(float deltaTime)
    {
        currentState?.Update(deltaTime);
    }
}
