using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Финал пролога: игрок дошёл до телефона и кликнул по нему — звонок прекращается,
// экран чернеет, и проигрывается КАТСЦЕНА из шагов:
//   шаг = текст (+ опц. озвучка) → держим, пока звучит озвучка (или Display Duration) →
//         текст исчезает → пауза Pause After → следующий шаг.
// Последним шагом удобно сделать название игры (крупный шрифт, без озвучки, на N секунд).
// После всех шагов — переход на сцену Next Scene.
public class ProloguePhone : MonoBehaviour
{
    [Header("Связи")]
    [Tooltip("Управление игроком в прологе (блокируем ввод; узнаём, дошёл ли он до телефона).")]
    [SerializeField] private PlayerPrologue player;
    [Tooltip("Скрипт вступления — чтобы остановить звонок при ответе.")]
    [SerializeField] private PrologueIntro intro;
    [Tooltip("Камера для клика. Пусто = Camera.main.")]
    [SerializeField] private Camera cam;
    [Tooltip("Телефон, по которому кликают (на нём или его детях нужен Collider).")]
    [SerializeField] private Transform phone;

    [Header("Экран и текст")]
    [Tooltip("Полноэкранная чёрная панель (CanvasGroup, alpha 0).")]
    [SerializeField] private CanvasGroup blackScreen;
    [Tooltip("Текст катсцены (TMP), лежит на чёрной панели.")]
    [SerializeField] private TMP_Text instructionText;
    [Tooltip("Длительность затемнения, сек. 0 = мгновенно.")]
    [SerializeField] private float fadeDuration = 0f;
    [Range(0f, 1f)]
    [SerializeField] private float voiceoverVolume = 1f;

    [Header("Шаги катсцены (по порядку)")]
    [SerializeField] private CutsceneStep[] steps;

    [Header("Переход")]
    [Tooltip("Имя сцены, которая загрузится после катсцены. Должна быть в Build Settings. Пусто = не грузить.")]
    [SerializeField] private string nextScene;

    [System.Serializable]
    public class CutsceneStep
    {
        [TextArea]
        [Tooltip("Текст шага. Можно с тегами <i>, <b>, <color=#RRGGBB>, <size> и т.п.")]
        public string text;
        [Tooltip("Озвучка шага (mp3). Если задана — текст держится, пока она звучит. Если пусто — Display Duration.")]
        public AudioClip voiceover;
        [Tooltip("Сколько секунд показывать шаг БЕЗ озвучки (напр. название игры).")]
        public float displayDuration = 3f;
        [Tooltip("Пауза (пустой чёрный экран) перед следующим шагом, сек.")]
        public float pauseAfter = 2f;
        [Tooltip("Размер шрифта для этого шага. 0 = по умолчанию; больше = крупнее (для названия игры).")]
        public float fontSize = 0f;
    }

    private AudioSource _voiceSource;
    private float _defaultFontSize;
    private bool _triggered;

    private void Awake()
    {
        _voiceSource = gameObject.AddComponent<AudioSource>();
        _voiceSource.playOnAwake = false;
        _voiceSource.volume = voiceoverVolume;

        if (cam == null) cam = Camera.main;
        if (blackScreen != null) { blackScreen.alpha = 0f; blackScreen.blocksRaycasts = false; }
        if (instructionText != null)
        {
            _defaultFontSize = instructionText.fontSize;
            instructionText.text = "";
        }
    }

    private void Update()
    {
        if (_triggered) return;
        // Кликнуть по телефону можно, только дойдя до него.
        if (player != null && !player.AtPhone) return;
        if (Input.GetMouseButtonDown(0)) TryClickPhone();
    }

    private void TryClickPhone()
    {
        if (cam == null || phone == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == phone || hit.transform.IsChildOf(phone))
                StartCoroutine(PlayCutscene());
        }
    }

    private IEnumerator PlayCutscene()
    {
        _triggered = true;
        if (intro != null) intro.StopRing();               // ответили — звонок смолк
        if (player != null) player.SetInputEnabled(false); // блокируем управление

        // Затемнение (0 = мгновенно).
        if (blackScreen != null)
        {
            blackScreen.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                blackScreen.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            blackScreen.alpha = 1f;
        }

        // Шаги по порядку.
        if (steps != null)
        {
            foreach (var step in steps)
            {
                if (step == null) continue;

                if (instructionText != null)
                {
                    instructionText.fontSize = step.fontSize > 0f ? step.fontSize : _defaultFontSize;
                    instructionText.text = step.text;
                }

                // Держим шаг: пока звучит озвучка, иначе — заданное время.
                if (step.voiceover != null)
                {
                    _voiceSource.clip = step.voiceover;
                    _voiceSource.Play();
                    yield return new WaitForSeconds(step.voiceover.length);
                }
                else
                {
                    yield return new WaitForSeconds(step.displayDuration);
                }

                // Текст исчезает, затем пауза перед следующим шагом.
                if (instructionText != null) instructionText.text = "";
                if (step.pauseAfter > 0f) yield return new WaitForSeconds(step.pauseAfter);
            }
        }

        // Переход на следующую сцену.
        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
    }
}
