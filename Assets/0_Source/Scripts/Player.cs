using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.0f;
    public float MoveSpeed => moveSpeed;

    [SerializeField] private float rotateSpeed = 360f; // градусов в секунду
    public float RotateSpeed => rotateSpeed;

    public bool IsRotating { get; set; }
    public float TargetAngleY { get; set; }

    private IMovementBarrier currentBarrier;

    public int MoveDirection { get; set; }

    private StateMachine stateMachine;

    public IdleState Idle { get; private set; }
    public MovingState Moving { get; private set; }
    public FacingPassengersState FacingPassengers { get; private set; }
    public RotatingState Rotating { get; private set; }

    private void Awake()
    {
        stateMachine = new StateMachine();

        Idle = new IdleState(this, stateMachine);
        Moving = new MovingState(this, stateMachine);
        FacingPassengers = new FacingPassengersState(this, stateMachine);
        Rotating = new RotatingState(this, stateMachine);

        stateMachine.ChangeState(Idle);
    }

    private void Update()
    {
        stateMachine.Update(Time.deltaTime);
    }

    public bool CanMoveInDirection(int direction)
    {
        if (currentBarrier == null)
            return true;

        Vector3 localMoveDir = Vector3.right * direction;
        Vector3 worldMoveDir = transform.TransformDirection(localMoveDir);

        float dot = Vector3.Dot(worldMoveDir, Vector3.right);

        if (dot > 0f)
            return currentBarrier.CanMoveForward;
        if (dot < 0f)
            return currentBarrier.CanMoveBackward;

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        IMovementBarrier barrier = other.GetComponent<IMovementBarrier>();
        if (barrier != null)
        {
            currentBarrier = barrier;
            MoveDirection = 0;
            stateMachine.ChangeState(Idle);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IMovementBarrier barrier = other.GetComponent<IMovementBarrier>();
        if (barrier != null && barrier == currentBarrier)
        {
            currentBarrier = null;
        }
    }

    // RotateY теперь принимает обычное State (без TState)
    public void RotateY(float delta, State afterState)
    {
        if (IsRotating)
            return;

        IsRotating = true;
        float currentY = transform.eulerAngles.y;
        TargetAngleY = currentY + delta;

        stateMachine.ChangeState(Rotating);
        Rotating.SetFinalState(afterState);
    }

    public abstract class PlayerState<TState> : State<TState> where TState : PlayerState<TState>
    {
        protected Player player;
        protected StateMachine stateMachine;

        protected PlayerState(Player player, StateMachine stateMachine)
        {
            this.player = player;
            this.stateMachine = stateMachine;
        }

        protected void UpdateRotation(float deltaTime)
        {
            if (!player.IsRotating)
                return;

            float currentY = player.transform.eulerAngles.y;
            float newY = Mathf.MoveTowardsAngle(
                currentY,
                player.TargetAngleY,
                player.RotateSpeed * deltaTime
            );

            Vector3 euler = player.transform.eulerAngles;
            euler.y = newY;
            player.transform.eulerAngles = euler;

            if (Mathf.Approximately(Mathf.DeltaAngle(newY, player.TargetAngleY), 0f))
            {
                player.IsRotating = false;
            }
        }
    }

    public class IdleState : PlayerState<IdleState>
    {
        public IdleState(Player player, StateMachine stateMachine)
            : base(player, stateMachine) { }

        protected override void OnEnter()
        {
            player.MoveDirection = 0;
        }

        protected override void OnUpdate(float deltaTime)
        {
            // во время поворота — только поворот, ввод игнорируем
            if (player.IsRotating)
            {
                UpdateRotation(deltaTime);
                return;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                player.RotateY(-90f, player.FacingPassengers);
                return;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                player.RotateY(+90f, player.FacingPassengers);
                return;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (player.CanMoveInDirection(+1))
                {
                    player.MoveDirection = +1;
                    stateMachine.ChangeState(player.Moving);
                }
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                if (player.CanMoveInDirection(-1))
                {
                    player.MoveDirection = -1;
                    stateMachine.ChangeState(player.Moving);
                }
            }
        }

        protected override void OnExit()
        {
        }
    }

    public class MovingState : PlayerState<MovingState>
    {
        public MovingState(Player player, StateMachine stateMachine)
            : base(player, stateMachine) { }

        protected override void OnEnter()
        {
        }

        protected override void OnUpdate(float deltaTime)
        {
            UpdateRotation(deltaTime);

            if (!player.CanMoveInDirection(player.MoveDirection))
            {
                player.MoveDirection = 0;
                stateMachine.ChangeState(player.Idle);
                return;
            }

            Vector3 move = Vector3.right * player.MoveDirection * player.MoveSpeed * deltaTime;
            player.transform.Translate(move, Space.Self);
        }

        protected override void OnExit()
        {
        }
    }

    public class FacingPassengersState : PlayerState<FacingPassengersState>
    {
        public FacingPassengersState(Player player, StateMachine stateMachine)
            : base(player, stateMachine) { }

        protected override void OnEnter()
        {
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                player.RotateY(-90f, player.Idle);
                return;
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                player.RotateY(+90f, player.Idle);
                return;
            }
        }

        protected override void OnExit()
        {
        }
    }

    public class RotatingState : PlayerState<RotatingState>
    {
        private State finalState; // State (базовый класс), не generic

        public RotatingState(Player player, StateMachine stateMachine)
            : base(player, stateMachine) { }

        // метод без TState
        public void SetFinalState(State state)
        {
            finalState = state;
        }

        protected override void OnEnter()
        {
        }

        protected override void OnUpdate(float deltaTime)
        {
            UpdateRotation(deltaTime);

            if (!player.IsRotating)
            {
                if (finalState != null)
                    stateMachine.ChangeState(finalState);
                else
                    stateMachine.ChangeState(player.Idle);
            }
        }

        protected override void OnExit()
        {
            finalState = null;
        }
    }
}