using UnityEngine;
using System.Collections.Generic;

namespace FadingSuns.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        public AudioSource musicSource;
        public float musicFadeDuration = 1.5f;

        [Header("SFX")]
        public AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private List<AudioEntry> clips = new();
        private Dictionary<string, AudioClip> clipById = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (var e in clips)
                clipById[e.id] = e.clip;
        }

        void Start()
        {
            GameManager.Instance.OnRegionChanged += OnRegionChanged;
        }

        private void OnRegionChanged(string regionId)
        {
            // Each region has its own ambient music track
            string trackId = regionId switch
            {
                "City" => "music_city",
                "Spaceport" => "music_spaceport",
                "Palace" => "music_palace",
                "Wilderness" => "music_wilderness",
                _ => "music_city"
            };
            PlayMusic(trackId);
        }

        public void PlayMusic(string id)
        {
            if (!clipById.TryGetValue(id, out var clip)) return;
            if (musicSource.clip == clip) return;
            StartCoroutine(FadeToMusic(clip));
        }

        public void PlaySFX(string id)
        {
            if (!clipById.TryGetValue(id, out var clip)) return;
            sfxSource.PlayOneShot(clip);
        }

        private System.Collections.IEnumerator FadeToMusic(AudioClip next)
        {
            float t = 0;
            float startVol = musicSource.volume;
            while (t < musicFadeDuration)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0, t / musicFadeDuration);
                yield return null;
            }
            musicSource.Stop();
            musicSource.clip = next;
            musicSource.Play();
            t = 0;
            while (t < musicFadeDuration)
            {
                t += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0, startVol, t / musicFadeDuration);
                yield return null;
            }
        }
    }

    [System.Serializable]
    public class AudioEntry
    {
        public string id;
        public AudioClip clip;
    }
}
