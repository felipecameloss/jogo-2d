using System.Collections.Generic;
using UnityEngine;

public enum Sfx { Shoot, Hit, EnemyDie, Pickup, LevelUp, PlayerHurt, Select }

// Toca efeitos sonoros. Se nenhum AudioClip for arrastado no Inspector,
// usa sons "8-bit" gerados por código (placeholder até vocês terem os sons finais).
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Range(0f, 1f)] public float sfxVolume = 0.5f;
    [Range(0f, 1f)] public float musicVolume = 0.4f;
    public AudioClip music;

    [Header("Opcional: substitui os sons gerados")]
    public AudioClip shoot;
    public AudioClip hit;
    public AudioClip enemyDie;
    public AudioClip pickup;
    public AudioClip levelUp;
    public AudioClip playerHurt;
    public AudioClip select;

    [Tooltip("Tempo mínimo entre dois sons iguais (evita estourar o áudio com 50 acertos no mesmo frame).")]
    public float minRepeatInterval = 0.04f;

    AudioSource sfxSource;
    readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();
    readonly Dictionary<Sfx, float> lastPlayed = new Dictionary<Sfx, float>();

    void Awake()
    {
        Instance = this;
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        clips[Sfx.Shoot] = shoot ? shoot : SfxGenerator.Shoot();
        clips[Sfx.Hit] = hit ? hit : SfxGenerator.Hit();
        clips[Sfx.EnemyDie] = enemyDie ? enemyDie : SfxGenerator.EnemyDie();
        clips[Sfx.Pickup] = pickup ? pickup : SfxGenerator.Pickup();
        clips[Sfx.LevelUp] = levelUp ? levelUp : SfxGenerator.LevelUp();
        clips[Sfx.PlayerHurt] = playerHurt ? playerHurt : SfxGenerator.PlayerHurt();
        clips[Sfx.Select] = select ? select : SfxGenerator.Select();

        if (music)
        {
            var musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    public static void Play(Sfx sfx, float volume = 1f, float pitchVariation = 0.1f)
    {
        if (Instance) Instance.PlayInternal(sfx, volume, pitchVariation);
    }

    void PlayInternal(Sfx sfx, float volume, float pitchVariation)
    {
        float now = Time.unscaledTime;
        if (lastPlayed.TryGetValue(sfx, out float last) && now - last < minRepeatInterval) return;
        lastPlayed[sfx] = now;

        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(clips[sfx], volume * sfxVolume);
    }
}
