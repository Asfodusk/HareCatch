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
    private System.Action<bool> _onClosed;
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

    // Находит PassengerTicket на самом объекте, его родителях ИЛИ детях.
    private static PassengerTicket FindPassengerTicket(Transform npc)
    {
        if (npc == null) return null;
        var pt = npc.GetComponentInParent<PassengerTicket>();
        if (pt == null) pt = npc.GetComponentInChildren<PassengerTicket>();
        return pt;
    }

    // Открыть проверку для конкретного НПС.
    // onClosed(bool kicked) вызывается после решения игрока: kicked = true, если выгнали.
    public void Begin(Transform npc, System.Action<bool> onClosed)
    {
        _onClosed = onClosed;
        // У некоторых НПС тег/коллайдер и PassengerTicket на разных объектах —
        // ищем компонент по всей иерархии, а не только на кликнутом объекте.
        _data = FindPassengerTicket(npc);
        // При «Выгнать» удаляем объект с PassengerTicket (корень НПС), а не дочерний коллайдер.
        _npc = _data != null ? _data.transform : npc;

        if (_data == null)
        {
            Debug.LogWarning($"[TicketInspection] На «{(npc ? npc.name : "null")}» нет компонента " +
                             "PassengerTicket — проверка пропущена.");
            CloseInternal(false);
            return;
        }

        if (panelRoot == null)
        {
            Debug.LogError("[TicketInspection] Не назначен panelRoot — открыть панель проверки нельзя.", this);
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
        // finally гарантирует, что панель закроется и Task диалога завершится,
        // даже если начисление награды бросит исключение (иначе — зависший диалог).
        try
        {
            if (game != null) game.ApplyReward(_data.ApproveMoney, _data.ApproveKarma);
            _data.MarkChecked(); // пассажир проверен — повторно проверить нельзя
            if (game != null) game.RecordApproved(_data.Id); // запоминаем выбор для сохранения
        }
        finally { CloseInternal(false); }
    }

    // Решение «Выгнать»: начисляем награду за высадку и удаляем пассажира из сцены.
    public void Kick()
    {
        if (!_active || _data == null) return;
        _active = false; // защита от повторного клика по кнопке в тот же кадр
        try
        {
            if (game != null) game.ApplyReward(_data.KickMoney, _data.KickKarma);
            if (game != null) game.RecordKicked(_data.Id); // запоминаем выбор для сохранения
        }
        finally { CloseInternal(true); }
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
        try { cb?.Invoke(removeNpc); }
        catch (System.Exception e) { Debug.LogError("[TicketInspection] Ошибка в колбэке закрытия: " + e, this); }
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
