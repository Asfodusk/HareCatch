using UnityEngine;

// Применяет сохранённые настройки громкости при запуске сцены —
// даже если панель настроек (со слайдерами) в этот момент выключена.
// Повесь этот компонент на любой ВКЛЮЧЁННЫЙ объект в сцене меню
// (например, на пустой GameObject "AudioSettingsLoader" или на корневой Canvas).
public class AudioSettingsLoader : MonoBehaviour
{
    void Start()
    {
        // Находим все VolumeSlider, включая те, что висят на выключенных объектах
        var sliders = FindObjectsByType<VolumeSlider>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var slider in sliders)
            slider.ApplySavedVolume();
    }
}
