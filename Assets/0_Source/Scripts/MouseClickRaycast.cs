using System;
using System.Threading.Tasks;
using UnityEngine;
using Yarn.Unity;

public class MouseClickRaycast : MonoBehaviour
{
    [SerializeField] private CameraFollow _cameraFollow;
    [SerializeField] private DialogueRunner _dialogueRunner;
    [SerializeField] private string _dialogueNodeName = "FirstD";
    [SerializeField] private TicketInspection _ticketInspection;
    [SerializeField] private Player _player;

    private InMemoryVariableStorage _variableStorage;
    private bool _dialogueStarted = false;

    // НПС, по которому сейчас идёт взаимодействие (нужен для проверки билета).
    private Transform _currentNpc;
    // Ожидание решения игрока в панели проверки билетов.
    private TaskCompletionSource<bool> _ticketDecision;

    private Vector3 _savedPosition;
    private Quaternion _savedRotation;
    private Transform _savedParent;

    private void Start()
    {
        if (_cameraFollow == null)
        {
            _cameraFollow = GetComponent<CameraFollow>();
            if (_cameraFollow == null)
            {
                Debug.LogError("MouseClickRaycast: Не найден компонент CameraFollow.");
            }
        }

        if (_dialogueRunner == null)
        {
            _dialogueRunner = FindFirstObjectByType<DialogueRunner>();
        }

        if (_dialogueRunner != null)
        {
            _dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);
            _variableStorage = _dialogueRunner.GetComponent<InMemoryVariableStorage>();
            if (_variableStorage == null)
            {
                _variableStorage = FindFirstObjectByType<InMemoryVariableStorage>();
            }

            // Команда <<check_ticket>> асинхронная: открывает панель проверки и ЖДЁТ
            // решения игрока. Благодаря этому прощальная реплика в диалоге проигрывается
            // уже ПОСЛЕ закрытия панели (см. Assets/0_Source/Dialogue/FirstD.yarn).
            _dialogueRunner.AddCommandHandler("check_ticket", (Func<Task>)CheckTicketCommand);
        }

        if (_ticketInspection == null)
        {
            _ticketInspection = FindFirstObjectByType<TicketInspection>();
        }

        if (_player == null)
        {
            _player = FindFirstObjectByType<Player>();
        }
    }

    private void Update()
    {
        if (_dialogueStarted) return;

        // Кликать по НПС можно ТОЛЬКО когда игрок повёрнут к пассажирам (FacingPassengers).
        // Иначе из Idle можно было начать диалог, а потом застрять: возврат всегда в
        // FacingPassengers, но игрок не развёрнут — W/S заблокированы, а уйти из вагона можно.
        if (_player != null && _player.statemachine.CurrentState != _player.facingpassengers) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            ProcessHitObject(hit.transform);
        }
    }

    private void ProcessHitObject(Transform hitTransform)
    {
        if (_cameraFollow == null) return;

        if ((hitTransform.CompareTag("Left") || hitTransform.CompareTag("Right")) && !_dialogueStarted)
        {
            _savedPosition = transform.position;
            _savedRotation = transform.rotation;
            _savedParent = transform.parent;

            _currentNpc = hitTransform;

            Debug.Log($"Попали в {hitTransform.name}! Запускаем диалог.");
            _cameraFollow.SetTarget(hitTransform);

            StartDialogue(hitTransform.name);
        }
    }

    private void StartDialogue(string targetObjectName)
    {
        if (_dialogueRunner == null) return;

        if (_variableStorage != null)
        {
            _variableStorage.SetValue("$speaker_name", targetObjectName);

            // Уже проверенным пассажирам не показываем опцию проверки билета.
            PassengerTicket pt = _currentNpc != null ? _currentNpc.GetComponent<PassengerTicket>() : null;
            _variableStorage.SetValue("$checked", pt != null && pt.IsChecked);
        }

        _dialogueRunner.StartDialogue(_dialogueNodeName);
        _dialogueStarted = true;

        // Блокируем управление игроком на время диалога (включая проверку билета).
        if (_player != null)
            _player.statemachine.ChangeState(_player.ignor);
    }

    // Yarn-команда <<check_ticket>>: открывает панель проверки и возвращает Task,
    // который завершается, когда игрок нажмёт «Одобрить» или «Выгнать». Пока Task не
    // завершён, диалог стоит на месте. Выставляет $kicked для ветвления диалога.
    private Task CheckTicketCommand()
    {
        if (_ticketInspection == null || _currentNpc == null)
            return Task.CompletedTask;

        // Обычный TaskCompletionSource: TrySetResult зовётся из обработчика кнопки на
        // главном потоке, поэтому продолжение диалога тоже идёт на главном потоке (безопасно
        // для Unity). cb вызывается последним в CloseInternal — реентрантность безвредна.
        _ticketDecision = new TaskCompletionSource<bool>();
        _ticketInspection.Begin(_currentNpc, kicked =>
        {
            try
            {
                if (_variableStorage != null)
                    _variableStorage.SetValue("$kicked", kicked);
            }
            finally
            {
                // Гарантируем завершение Task в любом случае — иначе диалог зависнет,
                // а игрок останется заблокированным (Ignor).
                _ticketDecision.TrySetResult(kicked);
            }
        });
        return _ticketDecision.Task;
    }

    private void OnDialogueComplete()
    {
        Debug.Log("Диалог завершен. Возвращаем камеру.");
        RestoreAfterInteraction();
    }

    // Возврат камеры на игрока, восстановление позиции игрока и разблокировка управления.
    private void RestoreAfterInteraction()
    {
        if (_cameraFollow != null)
        {
            _cameraFollow.SetTarget(null);
        }

        transform.parent = _savedParent;
        transform.position = _savedPosition;
        transform.rotation = _savedRotation;

        // Диалог окончен — возвращаем игрока в состояние «повёрнут к пассажирам».
        if (_player != null)
            _player.statemachine.ChangeState(_player.facingpassengers);

        _currentNpc = null;
        ResetDialogue();
    }

    private void OnDestroy()
    {
        if (_dialogueRunner != null)
        {
            _dialogueRunner.onDialogueComplete.RemoveListener(OnDialogueComplete);
        }
    }

    public void ResetDialogue()
    {
        _dialogueStarted = false;
    }
}
