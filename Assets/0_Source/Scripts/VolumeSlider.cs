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
        if(!slider) slider = GetComponent<Slider>();
        // Убеждаемся, что слайдер настроен на диапазон 0-10
        slider.minValue = MIN_SLIDER_VALUE;
        slider.maxValue = MAX_SLIDER_VALUE;
    }

    void OnEnable()
    {
        // Ставит значение на то, что в player prefs
        float savedVolume = PlayerPrefs.GetFloat(prefID, MAX_SLIDER_VALUE);
        slider.value = savedVolume;
        // Подписываемся на изменение слайдера
        slider.onValueChanged.AddListener(SetVolume);
        // Устанавливаем значение
        SetVolume(savedVolume);
    }

    private void OnDisable()
    {
        // Отписываемся когда не используется
        slider.onValueChanged.RemoveListener(SetVolume);
    }

    public void SetVolume(float sliderValue)
    {
        float normalizedValue = sliderValue / MAX_SLIDER_VALUE;
        float dbValue = Mathf.Log10(normalizedValue) * 20;;
        // Предотвращаем резкий звук при значении слайдера равным 0
        if (sliderValue == 0) dbValue = -80f;
        mixer.SetFloat(prefID, dbValue);
        // Сохраняем значение, чтобы настройки не сбросились при перезапуске игры
        PlayerPrefs.SetFloat(prefID, sliderValue);
        PlayerPrefs.Save();
    }

    void OnApplicationQuit()
    {
        // Сохранение при выходе из игры
        PlayerPrefs.Save();
    }

}
