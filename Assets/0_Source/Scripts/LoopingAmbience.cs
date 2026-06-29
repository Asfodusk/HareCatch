using UnityEngine;
using UnityEngine.Audio;

// Зацикленный фоновый звук (напр. стук колёс поезда).
// Клип вставляется как обычный AudioClip — это поле в инспекторе всегда видно и принимает mp3.
// AudioSource настраивается в коде, так что не нужно искать слот клипа у AudioSource в Unity 6.
public class LoopingAmbience : MonoBehaviour
{
    [Tooltip("Звук — перетащи сюда mp3 (это поле AudioClip, оно показывается корректно).")]
    [SerializeField] private AudioClip clip;
    [Tooltip("Группа AudioMixer (напр. Effects) — чтобы громкость шла через миксер.")]
    [SerializeField] private AudioMixerGroup output;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool playOnStart = true;

    private AudioSource _source;

    private void Awake()
    {
        // Переиспользуем уже добавленный AudioSource, либо создаём свой.
        _source = GetComponent<AudioSource>();
        if (_source == null) _source = gameObject.AddComponent<AudioSource>();

        _source.clip = clip;
        _source.outputAudioMixerGroup = output;
        _source.loop = true;
        _source.playOnAwake = false;
        _source.volume = volume;
        _source.spatialBlend = 0f; // 2D — фоновый звук
    }

    private void Start()
    {
        if (playOnStart && clip != null) _source.Play();
    }

    public void Play() { if (clip != null) _source.Play(); }
    public void Stop() { if (_source != null) _source.Stop(); }
}
