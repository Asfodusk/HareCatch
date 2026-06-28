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
    // true, пока открыта панель проверки билетов — откладывает возврат камеры.
    private bool _inspectionActive = false;

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

            // Команда <<check_ticket>> вызывается диалоговой опцией «Проверить билет»
            // (см. Assets/0_Source/Dialogue/FirstD.yarn).
            _dialogueRunner.AddCommandHandler("check_ticket", (System.Action)BeginTicketCheck);
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
        }

        _dialogueRunner.StartDialogue(_dialogueNodeName);
        _dialogueStarted = true;

        // Блокируем управление игроком на время диалога (и последующей проверки билета).
        if (_player != null)
            _player.statemachine.ChangeState(_player.ignor);
    }

    private void OnDialogueComplete()
    {
        // Если игрок выбрал «Проверить билет», диалог завершается, но возврат
        // камеры/игрока откладываем до закрытия панели проверки (FinishTicketCheck).
        if (_inspectionActive) return;

        Debug.Log("Диалог завершен. Возвращаем камеру.");
        RestoreAfterInteraction();
    }

    // Вызывается Yarn-командой <<check_ticket>> из диалоговой опции «Проверить билет».
    private void BeginTicketCheck()
    {
        if (_ticketInspection == null || _currentNpc == null) return;
        _inspectionActive = true;
        _ticketInspection.Begin(_currentNpc, FinishTicketCheck);
    }

    // Вызывается панелью проверки после решения игрока (Одобрить/Выгнать).
    private void FinishTicketCheck()
    {
        _inspectionActive = false;
        RestoreAfterInteraction();
    }

    // Возврат камеры на игрока, восстановление позиции игрока и разблокировка кликов.
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
