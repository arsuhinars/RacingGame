using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioController : MonoBehaviour
{
    public enum AudioType
    {
        Sound, Music
    }

    public AudioSource AudioSource => audioSource;
    public float Volume
    {
        get => volume;
        set
        {
            volume = value;
        }
    }

    [SerializeField] private bool playOnEnable = false;
    [SerializeField] private bool returnToPoolAfterPlayed = false;
    [Tooltip("Leave empty if it is not needed to use AudioClip that is in AudioSource")]
    [SerializeField] private ChanceValueElector<AudioClip> audioClips;
    [Space]
    [SerializeField] private float volume = 1f;
    [SerializeField] private AudioType audioType = AudioType.Sound;
    [SerializeField] private bool ignoreSettings = false;
    [SerializeField] private float minPitch = 1f;
    [SerializeField] private float maxPitch = 1f;

    private AudioSource audioSource;
    private PoolObject poolObject;
    private bool didStartPlaying = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        poolObject = GetComponent<PoolObject>();
    }

    private void Start()
    {
        SettingsManager.Instance.OnSettingsRefresh += UpdateAudioSourceVolume;
        UpdateAudioSourceVolume();
    }

    private void OnEnable()
    {
        if (playOnEnable)
            PlaySound();
    }

    private void Update()
    {
        if (didStartPlaying && !audioSource.isPlaying)
        {
            didStartPlaying = false;

            if (poolObject != null && returnToPoolAfterPlayed)
            {
                poolObject.ReturnToPool();
            }
        }
    }

    public void PlaySound()
    {
        if (audioClips.Count > 0)
            audioSource.clip = audioClips.GetNextValueByChance();
        UpdateAudioSourceVolume();
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.Play();
        didStartPlaying = true;
    }

    private void UpdateAudioSourceVolume()
    {
        audioSource.volume = volume;
        if (!ignoreSettings)
        {
            audioSource.volume *= audioType switch
            {
                AudioType.Sound => SettingsManager.Instance.IsSoundEnabled ? 1f : 0f,
                AudioType.Music => SettingsManager.Instance.IsMusicEnabled ? 1f : 0f,
                _ => 1f
            };
        }
    }
}
