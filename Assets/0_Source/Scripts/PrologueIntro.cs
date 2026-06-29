using System.Collections;
using UnityEngine;

// Открытие пролога: сначала полная тьма, затем через паузу звонит телефон
// и включается направленный на него свет.
// Звук звонка вставляется ПРЯМО как AudioClip (mp3) — компонент AudioSource не нужен,
// скрипт создаёт его сам.
// Повесь на пустой объект-«режиссёр» в сцене Prologue.
public class PrologueIntro : MonoBehaviour
{
    [Tooltip("Сколько секунд полной темноты до звонка.")]
    [SerializeField] private float delayBeforeRing = 2f;
    [Tooltip("Звук звонящего телефона — перетащи сюда mp3 (AudioClip). Будет зациклен.")]
    [SerializeField] private AudioClip ringClip;
    [Range(0f, 1f)]
    [Tooltip("Громкость звонка.")]
    [SerializeField] private float ringVolume = 1f;
    [Tooltip("Прожектор на телефон (Spot Light). По умолчанию выключен — включится со звонком.")]
    [SerializeField] private GameObject phoneLight;

    private AudioSource _ringSource;

    private void Awake()
    {
        // Свой AudioSource для звонка (звук вставляется клипом — руками компонент создавать не надо).
        _ringSource = gameObject.AddComponent<AudioSource>();
        _ringSource.clip = ringClip;
        _ringSource.loop = true;
        _ringSource.playOnAwake = false;
        _ringSource.volume = ringVolume;

        if (phoneLight != null) phoneLight.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(Open());
    }

    private IEnumerator Open()
    {
        yield return new WaitForSeconds(delayBeforeRing);

        if (_ringSource.clip != null) _ringSource.Play();
        if (phoneLight != null) phoneLight.SetActive(true);
    }

    // Остановить звонок (вызывается, когда игрок «ответит» — кликнет по телефону).
    public void StopRing()
    {
        if (_ringSource != null) _ringSource.Stop();
    }
}
