using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Загрузить сцену по имени (для кнопок меню и переходов).
    // Имя сцены должно быть добавлено в File -> Build Settings.
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
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
