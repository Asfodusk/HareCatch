using System.IO;
using TMPro;
using UnityEngine;

public class Game : MonoBehaviour
{
    [Header("Данные")]
    [SerializeField] private GameData gameData = new GameData();

    [Header("Длительность смены")]
    [Tooltip("Сколько реальных секунд длится один рабочий день. 180 = 3 минуты.")]
    [SerializeField] private float sessionDuration = 180f;

    [Header("Игровое время суток")]
    [Tooltip("Во сколько начинается рабочий день (6 = 6 утра).")]
    [SerializeField] private int startHour = 6;
    [Tooltip("Во сколько заканчивается рабочий день (18 = 6 вечера).")]
    [SerializeField] private int endHour = 18;
    [Tooltip("Шаг отображения таймера в игровых минутах (30 = время меняется каждые полчаса).")]
    [SerializeField] private int timerStepMinutes = 30;

    [Header("UI")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text timerText;

    // Доступ к данным игры из других скриптов
    public GameData Data => gameData;
    public int Wallet => gameData.wallet;

    // Рабочий день закончился?
    public bool IsWorkdayOver => gameData.currentTime >= sessionDuration;

    // Чтобы не пересобирать строки UI каждый кадр — храним последнее показанное
    private int shownWallet = int.MinValue;
    private int shownHour = -1;
    private int shownMinute = -1;

    private string SavePath => Path.Combine(Application.persistentDataPath, "gamedata.json");

    void Awake()
    {
        // При старте игры — загружаем сохранение, если оно есть
        Load();
    }

    void Update()
    {
        // Накапливаем прошедшее реальное время, пока смена не закончилась
        if (!IsWorkdayOver)
            gameData.currentTime = Mathf.Min(gameData.currentTime + Time.deltaTime, sessionDuration);

        UpdateUI();
    }

    private void UpdateUI()
    {
        // Деньги — обновляем текст только когда значение изменилось
        if (moneyText && gameData.wallet != shownWallet)
        {
            moneyText.text = $"{gameData.wallet}$";
            shownWallet = gameData.wallet;
        }

        // Таймер — обновляем только когда сменилась минута (а не каждый кадр)
        GetClock(out int hours, out int minutes);
        if (timerText && (hours != shownHour || minutes != shownMinute))
        {
            timerText.text = $"{hours:00}:{minutes:00}";
            shownHour = hours;
            shownMinute = minutes;
        }
    }

    // Переводит currentTime (0..sessionDuration) в игровое время суток
    private void GetClock(out int hours, out int minutes)
    {
        float progress = sessionDuration > 0f ? Mathf.Clamp01(gameData.currentTime / sessionDuration) : 1f;
        float totalHours = Mathf.Lerp(startHour, endHour, progress); // например 6..18
        hours = Mathf.FloorToInt(totalHours);
        int rawMinutes = Mathf.FloorToInt((totalHours - hours) * 60f);
        // Округляем вниз до шага (по умолчанию 30 -> минуты становятся 00 или 30)
        int step = Mathf.Max(1, timerStepMinutes);
        minutes = (rawMinutes / step) * step;
    }

    void OnApplicationQuit()
    {
        // При окончании игры (выход/закрытие) — сохраняем
        Save();
    }

    // Сохранение: GameData сам сериализует себя, Game пишет результат в файл
    public void Save()
    {
        string json = gameData.ToJson();
        File.WriteAllText(SavePath, json);
        Debug.Log($"[Game] Сохранено в {SavePath}");
    }

    // Загрузка: если файл есть — читаем JSON и создаём из него GameData
    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[Game] Файл сохранения не найден — стартуем с новыми данными.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        gameData = new GameData(json);
        Debug.Log($"[Game] Загружено из {SavePath}");
    }

    // Удаляет файл сохранения и сбрасывает данные в памяти.
    // Вызывается кнопкой в инспекторе (см. GameEditor) или из контекстного меню
    // компонента (⋮ в правом верхнем углу компонента → Очистить сохранение).
    [ContextMenu("Очистить сохранение")]
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log($"[Game] Сохранение удалено: {SavePath}");
        }
        else
        {
            Debug.Log("[Game] Файл сохранения не найден — удалять нечего.");
        }

        // Сбрасываем данные в памяти и просим UI перерисоваться
        gameData = new GameData();
        shownWallet = int.MinValue;
        shownHour = -1;
        shownMinute = -1;
    }
}
