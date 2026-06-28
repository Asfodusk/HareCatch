using UnityEngine;
using UnityEngine.UI;

// Контроллер проверки билетов (полноэкранная панель-оверлей в той же сцене).
// Показывает образец и билет текущего пассажира, обрабатывает кнопки
// «Одобрить»/«Выгнать», начисляет деньги/карму через Game, прячет деньги в HUD,
// а на паузе скрывает билет пассажира и блокирует кнопки (чтобы нельзя было
// изучать билет «из паузы»).
//
// Открывается из MouseClickRaycast по Yarn-команде <<check_ticket>>.
public class TicketInspection : MonoBehaviour
{
    [Header("Панель")]
    [Tooltip("Корневой объект панели проверки билетов. По умолчанию выключен.")]
    [SerializeField] private GameObject panelRoot;

    [Header("Картинки билетов")]
    [Tooltip("Образец билета — эталон, с которым игрок сравнивает билет пассажира.")]
    [SerializeField] private Sprite sampleTicket;
    [Tooltip("UI Image, в котором отображается ОБРАЗЕЦ.")]
    [SerializeField] private Image sampleTicketImage;
    [Tooltip("UI Image, в котором отображается билет ТЕКУЩЕГО пассажира.")]
    [SerializeField] private Image passengerTicketImage;

    [Header("Кнопки")]
    [Tooltip("Кнопка «Одобрить».")]
    [SerializeField] private Button approveButton;
    [Tooltip("Кнопка «Выгнать».")]
    [SerializeField] private Button kickButton;

    [Header("Связи (можно оставить пустыми — найдутся сами)")]
    [Tooltip("Менеджер игры: деньги, карма, HUD.")]
    [SerializeField] private Game game;
    [Tooltip("Менеджер паузы: чтобы прятать билет пассажира на паузе.")]
    [SerializeField] private PauseManager pauseManager;

    private Transform _npc;
    private PassengerTicket _data;
    private System.Action _onClosed;
    private bool _active;

    public bool IsActive => _active;

    private void Awake()
    {
        if (game == null) game = FindFirstObjectByType<Game>();
        if (pauseManager == null) pauseManager = FindFirstObjectByType<PauseManager>();

        if (approveButton) approveButton.onClick.AddListener(Approve);
        if (kickButton) kickButton.onClick.AddListener(Kick);
        if (pauseManager != null) pauseManager.PauseChanged += OnPauseChanged;

        if (panelRoot) panelRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        if (approveButton) approveButton.onClick.RemoveListener(Approve);
        if (kickButton) kickButton.onClick.RemoveListener(Kick);
        if (pauseManager != null) pauseManager.PauseChanged -= OnPauseChanged;
    }

    // Открыть проверку для конкретного НПС.
    // onClosed вызывается после решения игрока — им MouseClickRaycast возвращает камеру/игрока.
    public void Begin(Transform npc, System.Action onClosed)
    {
        _npc = npc;
        _onClosed = onClosed;
        _data = npc != null ? npc.GetComponent<PassengerTicket>() : null;

        if (_data == null)
        {
            Debug.LogWarning($"[TicketInspection] На «{(npc ? npc.name : "null")}» нет компонента " +
                             "PassengerTicket — проверка пропущена.");
            CloseInternal(false);
            return;
        }

        if (sampleTicketImage) sampleTicketImage.sprite = sampleTicket;
        if (passengerTicketImage) passengerTicketImage.sprite = _data.Ticket;

        if (game != null) game.ShowMoney(false);
        if (panelRoot) panelRoot.SetActive(true);
        _active = true;

        // На случай, если панель открыли на уже включённой паузе.
        ApplyPauseVisibility(pauseManager != null && pauseManager.IsPaused);
    }

    // Решение «Одобрить»: начисляем награду за одобрение, пассажир остаётся.
    public void Approve()
    {
        if (!_active || _data == null) return;
        _active = false; // защита от повторного клика по кнопке в тот же кадр
        if (game != null) game.ApplyReward(_data.ApproveMoney, _data.ApproveKarma);
        CloseInternal(false);
    }

    // Решение «Выгнать»: начисляем награду за высадку и удаляем пассажира из сцены.
    public void Kick()
    {
        if (!_active || _data == null) return;
        _active = false; // защита от повторного клика по кнопке в тот же кадр
        if (game != null) game.ApplyReward(_data.KickMoney, _data.KickKarma);
        CloseInternal(true);
    }

    private void CloseInternal(bool removeNpc)
    {
        _active = false;
        if (panelRoot) panelRoot.SetActive(false);
        if (game != null) game.ShowMoney(true);

        if (removeNpc && _npc != null) Destroy(_npc.gameObject);

        var cb = _onClosed;
        _onClosed = null;
        _npc = null;
        _data = null;
        cb?.Invoke();
    }

    private void OnPauseChanged(bool paused)
    {
        if (!_active) return;
        ApplyPauseVisibility(paused);
    }

    // На паузе прячем билет пассажира и блокируем кнопки решения.
    private void ApplyPauseVisibility(bool paused)
    {
        if (passengerTicketImage) passengerTicketImage.enabled = !paused;
        if (approveButton) approveButton.interactable = !paused;
        if (kickButton) kickButton.interactable = !paused;
    }
}
