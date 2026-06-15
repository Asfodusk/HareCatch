using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using System.Collections;
using System;

public class Ignor : State<Ignor>
{
    protected override void OnEnter() { }
    protected override void OnUpdate(float deltaTime) { }
    protected override void OnExit() { }
}
public class Idle : State<Idle>
{
    protected readonly Player player;
    public Idle(Player player) { this.player = player; }

    protected override void OnEnter() { }


    protected override void OnUpdate(float deltaTime)
    {
        // Повороты A / D
        if (Input.GetKeyDown(KeyCode.A))
        {
            player.RotateY(-90f, player.facingpassengers);
            return;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            player.RotateY(+90f, player.facingpassengers);
            return;
        }

        // Движение W / S по направлению взгляда (локальный X)
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (player.CanMoveInDirection(+1))
            {
                player.moveDirection = +1;
                player.statemachine.ChangeState(player.moving);
            }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (player.CanMoveInDirection(-1))
            {
                player.moveDirection = -1;
                player.statemachine.ChangeState(player.moving);
            }
        }
    }


    protected override void OnExit() { }
}


public class FacingPassengers : State<FacingPassengers>
{
    protected readonly Player player;
    public FacingPassengers(Player player) { this.player = player; }


    protected override void OnEnter() { }
    protected override void OnUpdate(float deltaTime)
    {
        //В этом состоянии игнорируем движение W / S, но реагируем на A / D, чтобы развернуться дальше
        if (Input.GetKeyDown(KeyCode.A))
        {
            player.RotateY(-90f, player.idle);
            return;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            player.RotateY(+90f, player.idle);
            return;
        }
    }
    protected override void OnExit()
    {
    }
}


public class Moving : State<Moving>
{
    protected readonly Player player;
    public Moving(Player player) { this.player = player; }


    protected override void OnEnter() { }
    protected override void OnUpdate(float deltaTime)
    {
        // Пока идём — ввод игнорируем
        if (!player.CanMoveInDirection(player.moveDirection))
        {
            player.statemachine.ChangeState(player.idle);
            player.moveDirection = 0;
            return;
        }

        // Движение по локальной оси X (right) — вперёд/назад по взгляду
        Vector3 move = Vector3.right * player.moveDirection * player.moveSpeed * deltaTime;
        player.transform.Translate(move, Space.Self);
    }
    protected override void OnExit() { }
}


public class Player : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 4.0f;
    [SerializeField] public float rotationSpeed = 180.0f;

    //здесь будем "запоминать" текущий барьер
    private IMovementBarrier currentBarrier;

    public StateMachine statemachine;
    public Idle idle;
    public FacingPassengers facingpassengers;
    public Moving moving;
    public Ignor ignor;

    public int moveDirection = 0; // +1 вперёд по локальной оси X, -1 назад по X

    private void Awake()
    {
        idle = new Idle(this);
        ignor = new Ignor();
        facingpassengers = new FacingPassengers(this);
        moving = new Moving(this);
        statemachine = new StateMachine();
        statemachine.ChangeState(idle);
    }

    private void Update()
    {
        //В состоянии бездействия мы читаем инпуты, в состоянии ходьбы мы игнорируем их и движемся пока не дойдем до барьера
        statemachine.Update(Time.deltaTime);
    }
    public bool CanMoveInDirection(int direction)
    {
        if (currentBarrier == null)
            return true; // барьеров нет, можно куда угодно

        // Локальное направление движения (по взгляду, по локальному X)
        Vector3 localMoveDir = Vector3.right * direction;
        // Преобразуем его в мировые координаты
        Vector3 worldMoveDir = transform.TransformDirection(localMoveDir);

        // Смотрим, куда это по мировой оси X (коридор)
        float dot = Vector3.Dot(worldMoveDir, Vector3.right);

        if (dot > 0f)
            return currentBarrier.CanMoveForward;
        if (dot < 0f)
            return currentBarrier.CanMoveBackward;

        // Если вдруг движение строго поперёк коридора — блокируем
        return false;
    }

    //Время потрогать траву (барьер)
    private void OnTriggerEnter(Collider other)
    {
        IMovementBarrier barrier = other.GetComponent<IMovementBarrier>();
        if (barrier != null)
        {
            currentBarrier = barrier;

            // Всегда просто останавливаемся, не меняя координаты
            statemachine.ChangeState(idle);
            moveDirection = 0;
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

    //Вертел я этого игрока на оси Y
    public void RotateY(float delta, State goalstate)
    {
        statemachine.ChangeState(ignor);
        StartCoroutine(RotateYSmooth(delta, goalstate));
    }

    private IEnumerator RotateYSmooth(float delta, State goalstate)
    {
        Vector3 startEuler = transform.eulerAngles;
        float startRotation = startEuler.y;
        float targetRotation = startRotation + delta;

        // Нормализуем угол
        while (targetRotation >= 360f) targetRotation -= 360f;
        while (targetRotation < 0f) targetRotation += 360f;

        float diff = targetRotation - startRotation;

        // Нормализуем разницу в [-180, 180] для кратчайшего пути
        while (diff > 180f) diff -= 360f;
        while (diff < -180f) diff += 360f;

        float timeElapsed = 0f;
        float duration = Mathf.Abs(diff) / rotationSpeed; // Время поворота в секундах

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / duration);

            float currentRotation = Mathf.Lerp(0f, diff, t);
            transform.eulerAngles = new Vector3(startEuler.x, startRotation + currentRotation, startEuler.z);

            yield return null;
        }

        transform.eulerAngles = new Vector3(startEuler.x, targetRotation, startEuler.z);
        statemachine.ChangeState(goalstate);
    }
}