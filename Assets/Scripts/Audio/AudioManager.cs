using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Sfx { Shoot, Hit, EnemyDie, Pickup, LevelUp, PlayerHurt, Select, Step, Chop, Mine, Key }

// Toca efeitos e música.
// Efeitos: AudioClip arrastado no Inspector; senão o WAV em Assets/Art/Resources/Audio
// (sfx_<nome>.wav, ou variações sfx_<nome>_1, _2... sorteadas a cada vez); senão o som 8-bit gerado
// por código. Para trocar um som, basta substituir o arquivo mantendo o nome.
// Música: faixas de dia, noite e chefe, trocadas com fade pelo DayNightCycle e pelos eventos do Boss.
// Em cenas sem ciclo (o menu), toca `music` se houver.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Música (vazio: usa Resources/Audio/musica_dia, musica_noite e musica_chefe)")]
    public AudioClip dayMusic;
    public AudioClip nightMusic;
    public AudioClip bossMusic;
    [Tooltip("Faixa fixa para cenas sem dia e noite, como o menu. Vazio: sem música.")]
    public AudioClip music;
    [Tooltip("Segundos para uma faixa sumir e a outra entrar.")]
    public float musicFade = 2.5f;

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

    // Várias fontes em rodízio: cada som tem o próprio tom sem desafinar os que ainda tocam.
    const int SfxVoices = 8;
    readonly AudioSource[] voices = new AudioSource[SfxVoices];
    int nextVoice;
    readonly Dictionary<Sfx, AudioClip[]> clips = new Dictionary<Sfx, AudioClip[]>();
    readonly Dictionary<Sfx, float> lastPlayed = new Dictionary<Sfx, float>();

    class Track
    {
        public AudioSource Source;
        public float Level;
    }

    readonly Dictionary<AudioClip, Track> tracks = new Dictionary<AudioClip, Track>();
    AudioClip day, night, boss, current;
    DayNightCycle cycle;
    float musicVolume;

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < SfxVoices; i++)
        {
            voices[i] = gameObject.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
        }

        Register(Sfx.Shoot, shoot, "sfx_tiro", SfxGenerator.Shoot);
        Register(Sfx.Hit, hit, "sfx_acerto", SfxGenerator.Hit);
        Register(Sfx.EnemyDie, enemyDie, "sfx_morte_inimigo", SfxGenerator.EnemyDie);
        Register(Sfx.Pickup, pickup, "sfx_coleta", SfxGenerator.Pickup);
        Register(Sfx.LevelUp, levelUp, "sfx_nivel", SfxGenerator.LevelUp);
        Register(Sfx.PlayerHurt, playerHurt, "sfx_dano", SfxGenerator.PlayerHurt);
        Register(Sfx.Select, select, "sfx_selecao", SfxGenerator.Select);
        Register(Sfx.Step, null, "sfx_passo", null);
        Register(Sfx.Chop, null, "sfx_machado", SfxGenerator.Hit);
        Register(Sfx.Mine, null, "sfx_picareta", SfxGenerator.Hit);
        Register(Sfx.Key, null, "sfx_chave", SfxGenerator.Pickup);

        musicVolume = GameSettings.MusicVolume;
        GameSettings.Changed += ApplyVolumes;
    }

    void Start()
    {
        // No Start o DayNightCycle da cena já existe (ele se registra no Awake).
        cycle = DayNightCycle.Instance;
        if (cycle)
        {
            day = dayMusic ? dayMusic : Resources.Load<AudioClip>("Audio/musica_dia");
            night = nightMusic ? nightMusic : Resources.Load<AudioClip>("Audio/musica_noite");
            boss = bossMusic ? bossMusic : Resources.Load<AudioClip>("Audio/musica_chefe");
            cycle.OnPeriodChanged += OnPeriodChanged;
            Boss.Spawned += OnBossSpawned;
            Boss.Defeated += OnBossDefeated;
            PlayMusic(cycle.IsNight ? night : day);
        }
        else PlayMusic(music);
    }

    void OnDestroy()
    {
        GameSettings.Changed -= ApplyVolumes;
        Boss.Spawned -= OnBossSpawned;
        Boss.Defeated -= OnBossDefeated;
        if (cycle) cycle.OnPeriodChanged -= OnPeriodChanged;
        if (Instance == this) Instance = null;
    }

    void ApplyVolumes() => musicVolume = GameSettings.MusicVolume;

    // ---------------------------------------------------------------- música

    // Na noite de chefe, a faixa do chefe manda até ele morrer.
    void OnPeriodChanged(DayPeriod period)
    {
        if (Boss.Current) return;
        PlayMusic(period == DayPeriod.Night ? night : day);
    }

    void OnBossSpawned(Boss b) => PlayMusic(boss);

    // Silêncio de vitória até o sol nascer; o amanhecer traz a música do dia.
    void OnBossDefeated(Boss b) => PlayMusic(null);

    void PlayMusic(AudioClip clip)
    {
        current = clip;
        if (!clip) return;
        if (!tracks.TryGetValue(clip, out var track))
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
            track = new Track { Source = source };
            tracks[clip] = track;
        }
        if (!track.Source.isPlaying) track.Source.Play(); // recomeça do início se estava parada
    }

    // Fade em tempo real: continua na pausa e nas janelas de jogo.
    void Update()
    {
        if (tracks.Count == 0) return;
        float step = Time.unscaledDeltaTime / Mathf.Max(0.01f, musicFade);
        foreach (var pair in tracks)
        {
            var track = pair.Value;
            track.Level = Mathf.MoveTowards(track.Level, pair.Key == current ? 1f : 0f, step);
            track.Source.volume = track.Level * musicVolume;
            if (track.Level <= 0f && track.Source.isPlaying) track.Source.Stop();
        }
    }

    // ---------------------------------------------------------------- efeitos

    void Register(Sfx sfx, AudioClip custom, string file, Func<AudioClip> generated)
    {
        if (custom) clips[sfx] = new[] { custom };
        else clips[sfx] = Load(file) ?? (generated != null ? new[] { generated() } : null);
    }

    static AudioClip[] Load(string name)
    {
        var single = Resources.Load<AudioClip>("Audio/" + name);
        if (single) return new[] { single };
        var list = new List<AudioClip>();
        for (int i = 1; i < 16; i++)
        {
            var variant = Resources.Load<AudioClip>($"Audio/{name}_{i}");
            if (!variant) break;
            list.Add(variant);
        }
        return list.Count > 0 ? list.ToArray() : null;
    }

    public static void Play(Sfx sfx, float volume = 1f, float pitchVariation = 0.1f)
    {
        if (Instance) Instance.PlayInternal(sfx, volume, pitchVariation);
    }

    void PlayInternal(Sfx sfx, float volume, float pitchVariation)
    {
        if (!clips.TryGetValue(sfx, out var set) || set == null || set.Length == 0) return;
        float now = Time.unscaledTime;
        if (lastPlayed.TryGetValue(sfx, out float last) && now - last < minRepeatInterval) return;
        lastPlayed[sfx] = now;

        var voice = voices[nextVoice];
        nextVoice = (nextVoice + 1) % SfxVoices;
        voice.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        voice.PlayOneShot(set[Random.Range(0, set.Length)], volume * GameSettings.SfxVolume);
    }
}
