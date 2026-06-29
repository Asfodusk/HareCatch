using UnityEngine;
using UnityEngine.Audio;

// Применяет сохранённую (PlayerPrefs) громкость к AudioMixer при старте сцены —
// даже без UI-слайдеров (для игровых сцен, где нет меню настроек).
// Формула совпадает с VolumeSlider, поэтому громкость, выставленная в меню, применяется и тут.
// Повесь на любой включённый объект в сцене и укажи миксер + имена параметров.
public class SavedVolumeApplier : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [Tooltip("Exposed-параметры громкости (как prefID у VolumeSlider). Напр. Music, Effects.")]
    [SerializeField] private string[] parameters = { "Music", "Effects" };

    private const float MAX_SLIDER_VALUE = 10f;

    private void Awake()
    {
        if (mixer == null) return;

        foreach (var p in parameters)
        {
            // Применяем только те параметры, что игрок реально сохранял.
            if (string.IsNullOrEmpty(p) || !PlayerPrefs.HasKey(p)) continue;

            float saved = PlayerPrefs.GetFloat(p, MAX_SLIDER_VALUE);
            float db = saved <= 0f ? -80f : Mathf.Log10(saved / MAX_SLIDER_VALUE) * 20f;
            mixer.SetFloat(p, db);
        }
    }
}
