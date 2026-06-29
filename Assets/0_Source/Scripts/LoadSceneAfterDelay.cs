using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Через delay секунд (или досрочно по клику/клавише) загружает указанную сцену.
// Для концовок: проиграть экран и вернуться в главное меню (sceneName = _Menu).
public class LoadSceneAfterDelay : MonoBehaviour
{
    [Tooltip("Какую сцену загрузить (для концовок — _Menu).")]
    [SerializeField] private string sceneName = "_Menu";
    [Tooltip("Через сколько секунд.")]
    [SerializeField] private float delay = 5f;
    [Tooltip("Разрешить пропустить ожидание кликом / любой клавишей.")]
    [SerializeField] private bool skippableByInput = true;

    private void Start()
    {
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        float t = 0f;
        while (t < delay)
        {
            if (skippableByInput && (Input.GetMouseButtonDown(0) || Input.anyKeyDown))
                break;
            t += Time.deltaTime;
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }
}
