using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string prefID;
    private const float MIN_SLIDER_VALUE = 0f;
    private const float MAX_SLIDER_VALUE = 10f;

    void Awake()
    {
        // Добавляет слайдер, если его нет
        if (!slider) slider = GetComponent<Slider>();
        // Убеждаемся, что слайдер настроен на диапазон 0-10
        slider.minValue = MIN_SLIDER_VALUE;
        slider.maxValue = MAX_SLIDER_VALUE;
    }

    void OnEnable()
    {
        // Ставит значение на то, что в player prefs
        float savedVolume = PlayerPrefs.GetFloat(prefID, MAX_SLIDER_VALUE);
        slider.value = savedVolume;
        // Подписываемся на изменение слайдера (после установки value, чтобы не было лишнего вызова)
        slider.onValueChanged.AddListener(SetVolume);
        // Применяем к миксеру (без повторного сохранения)
        ApplyToMixer(savedVolume);
    }

    private void OnDisable()
    {
        // Отписываемся когда не используется
        slider.onValueChanged.RemoveListener(SetVolume);
    }

    // Вызывается слайдером при изменении: применяет звук И сохраняет настройку
    public void SetVolume(float sliderValue)
    {
        ApplyToMixer(sliderValue);
        // Сохраняем значение, чтобы настройки не сбросились при перезапуске игры
        PlayerPrefs.SetFloat(prefID, sliderValue);
        PlayerPrefs.Save();
    }

    // Применяет сохранённую настройку к миксеру без обращения к слайдеру.
    // Работает даже если объект выключен (панель настроек закрыта),
    // поэтому её можно вызывать на старте игры, чтобы громкость применилась сразу.
    public void ApplySavedVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(prefID, MAX_SLIDER_VALUE);
        ApplyToMixer(savedVolume);
    }

    // Перевод значения слайдера (0-10) в децибелы и установка в миксер
    private void ApplyToMixer(float sliderValue)
    {
        float normalizedValue = sliderValue / MAX_SLIDER_VALUE;
        float dbValue = Mathf.Log10(normalizedValue) * 20f;
        // Предотвращаем -Infinity/NaN и резкий звук при значении слайдера равном 0
        if (sliderValue <= 0f) dbValue = -80f;
        mixer.SetFloat(prefID, dbValue);
    }

    void OnApplicationQuit()
    {
        // Сохранение при выходе из игры
        PlayerPrefs.Save();
    }
}
