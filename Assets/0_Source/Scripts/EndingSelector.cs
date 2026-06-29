using UnityEngine;
using UnityEngine.SceneManagement;

// Завершает рабочий день и грузит концовку.
// Конец дня = таймер дошёл до конца (Game.IsWorkdayOver) ИЛИ опрошены все пассажиры.
// Выбор концовки по карме/деньгам. При переходе сохранение стирается.
// Повесь на объект в игровой сцене (напр. на Game).
public class EndingSelector : MonoBehaviour
{
    [Tooltip("Менеджер игры. Если пусто — найдётся сам.")]
    [SerializeField] private Game game;

    [Header("Сцены концовок (должны быть в Build Settings)")]
    [SerializeField] private string goodEndScene = "GoodEnd";
    [SerializeField] private string neutralEndScene = "NeutralEnd";
    [SerializeField] private string badEndScene = "BadEnd";

    private bool _ended;

    private void Awake()
    {
        if (game == null) game = FindFirstObjectByType<Game>();
    }

    private void Update()
    {
        if (_ended || game == null) return;

        // День заканчивается по таймеру ИЛИ когда опрошены все пассажиры.
        if (!game.IsWorkdayOver && !AllPassengersSurveyed()) return;

        _ended = true;

        int karma = game.Data.karma;
        int money = game.Data.wallet;

        string scene;
        if (karma == 0)                                       // никого не выгнал
            scene = goodEndScene;
        else if (karma > 0 && karma < 100 && money >= 180)
            scene = neutralEndScene;
        else
            scene = badEndScene;

        Game.DeleteSaveFile();          // при переходе к концовке сейв стирается
        SceneManager.LoadScene(scene);
    }

    // Опрошены ли все: непроверенных пассажиров не осталось
    // (выгнанные удалены со сцены, одобренные помечены IsChecked).
    private bool AllPassengersSurveyed()
    {
        var npcs = FindObjectsByType<PassengerTicket>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var n in npcs)
            if (!n.IsChecked) return false;

        return true;
    }
}
