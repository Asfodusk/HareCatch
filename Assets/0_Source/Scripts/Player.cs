using UnityEngine;

//Кодим игрока
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.0f;

    //здесь будем "запоминать" текущий барьер
    private IMovementBarrier currentBarrier;

    //Игрок либо стоит на месте либо двигается
    private enum PlayerState
    {
        Idle,
        Moving,
        FacingPassengers
    }

    private PlayerState state = PlayerState.Idle;
    private int moveDirection = 0; // +1 вперёд по локальной оси X, -1 назад по X

    private void Update()
    {
        //В состоянии бездействия мы читаем инпуты, в состоянии ходьбы мы игнорируем их и движемся пока не дойдем до барьера
        switch (state)
        {
            case PlayerState.Idle:
                HandleInputIdle();
                break;

            case PlayerState.Moving:
                HandleAutoMove();
                break;

            case PlayerState.FacingPassengers:
                HandleFacingPassengers();
                break;
        }
    }

    //Чтение импута
    private void HandleInputIdle()
    {
        // Повороты A / D
        if (Input.GetKeyDown(KeyCode.A))
        {
            RotateY(-90f);
            state = PlayerState.FacingPassengers;
            return;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            RotateY(+90f);
            state = PlayerState.FacingPassengers;
            return;
        }

        // Движение W / S по направлению взгляда (локальный X)
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (CanMoveInDirection(+1))
            {
                moveDirection = +1;
                state = PlayerState.Moving;
            }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (CanMoveInDirection(-1))
            {
                moveDirection = -1;
                state = PlayerState.Moving;
            }
        }
    }

    //Режим, когда контролёр смотрит на пассажиров
    private void HandleFacingPassengers()
    {
        //В этом состоянии игнорируем движение W / S, но реагируем на A / D, чтобы развернуться дальше
        if (Input.GetKeyDown(KeyCode.A))
        {
            RotateY(-90f);
            state = PlayerState.Idle;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            RotateY(+90f);
            state = PlayerState.Idle;
        }
    }

    //Ходьба до упора
    private void HandleAutoMove()
    {
        // Пока идём — ввод игнорируем
        if (!CanMoveInDirection(moveDirection))
        {
            state = PlayerState.Idle;
            moveDirection = 0;
            return;
        }

        // Движение по локальной оси X (right) — вперёд/назад по взгляду
        Vector3 move = Vector3.right * moveDirection * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.Self);
    }

    //Эта часть чтоб не было забавных ситуаций когда игрок буквально сбегает из игры
    private bool CanMoveInDirection(int direction)
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
            state = PlayerState.Idle;
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
    private void RotateY(float delta)
    {
        Vector3 euler = transform.eulerAngles;
        euler.y += delta;
        transform.eulerAngles = euler;
    }
}