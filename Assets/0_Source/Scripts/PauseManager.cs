using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Tooltip("Объект с текстом \"Пауза\" по центру экрана. Должен быть выключен по умолчанию.")]
    [SerializeField] private GameObject pauseLabel;

    private bool isPaused;

    // Текущее состояние паузы (читают другие системы, напр. проверка билетов).
    public bool IsPaused => isPaused;

    // Срабатывает при каждом переключении паузы (true — пауза включена).
    public event System.Action<bool> PauseChanged;

    void Update()
    {
        // Update вызывается даже при timeScale = 0, поэтому Escape ловится и на паузе
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    // Переключает паузу: останавливает/возобновляет время и показывает/прячет надпись
    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pauseLabel) pauseLabel.SetActive(isPaused);
        PauseChanged?.Invoke(isPaused);
    }

    // Подстраховка: если объект выключат/уничтожат во время паузы (например, при выходе
    // из Play mode), вернём нормальную скорость времени, чтобы игра потом не «зависла».
    void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
