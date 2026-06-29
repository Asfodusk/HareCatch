using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Логика главного меню.
// «Новая игра» — стирает сохранение и запускает пролог (игра начнётся с чистой GameData).
// «Загрузить» — если есть сохранение, открывает игровую сцену (она сама подгрузит данные).
// «Выход» — закрывает игру.
public class MainMenu : MonoBehaviour
{
    [Tooltip("Сцена пролога.")]
    [SerializeField] private string prologueScene = "Prologue";
    [Tooltip("Игровая сцена.")]
    [SerializeField] private string gameplayScene = "Walking demo scene";
    [Tooltip("Кнопка «Загрузить» — выключится, если сохранения нет. Можно оставить пустой.")]
    [SerializeField] private Button loadButton;

    private void Start()
    {
        // Нет сейва — кнопку «Загрузить» делаем неактивной.
        if (loadButton != null) loadButton.interactable = Game.HasSave;
    }

    // Новая игра: стираем сохранение и идём в пролог.
    public void NewGame()
    {
        Game.DeleteSaveFile();
        SceneManager.LoadScene(prologueScene);
    }

    // Загрузить: только если есть сохранение — открываем игровую сцену.
    public void LoadGame()
    {
        if (Game.HasSave)
            SceneManager.LoadScene(gameplayScene);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        // В редакторе Application.Quit() ничего не делает — останавливаем Play для теста.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
