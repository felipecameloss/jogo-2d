using System;
using UnityEngine;

public enum DayPeriod { Day, Night }

// Ciclo de dia e noite. De dia não nascem inimigos; de noite eles vêm.
// Também cuida do visual: escurece a tela à noite, com uma luz em volta do jogador.
public class DayNightCycle : MonoBehaviour
{
    public static DayNightCycle Instance { get; private set; }

    [Tooltip("Duração de cada período (dia ou noite) em segundos.")]
    public float periodDuration = 180f;
    public DayPeriod startPeriod = DayPeriod.Day;

    [Header("Visual")]
    public SpriteRenderer ground;
    [Tooltip("Cor do chão de acordo com a luz (esquerda = noite, meio = pôr do sol, direita = dia).")]
    public Gradient groundTint = DefaultGroundTint();
    [Tooltip("Cor da camada de escuridão (esquerda = noite, direita = dia).")]
    public Gradient overlayTint = DefaultOverlayTint();
    [Tooltip("Opacidade da escuridão no meio da noite.")]
    [Range(0f, 1f)] public float nightDarkness = 0.82f;
    [Tooltip("Raio (em unidades) totalmente iluminado em volta do jogador à noite.")]
    public float lightInnerRadius = 2.6f;
    [Tooltip("Raio onde a escuridão fica total.")]
    public float lightOuterRadius = 6.5f;

    public DayPeriod Period { get; private set; }
    public bool IsNight => Period == DayPeriod.Night;
    public int DayNumber { get; private set; }
    public int NightNumber { get; private set; }
    // Tempo total já passado de noite. O spawner usa isso para a dificuldade.
    public float NightElapsed { get; private set; }
    public float PeriodTime { get; private set; }
    public float TimeLeftInPeriod => Mathf.Max(0f, periodDuration - PeriodTime);
    // Hora do relógio do jogo (0 a 24). O dia vai de 06:00 às 18:00 e a noite de 18:00 às 06:00.
    public float Hour => Mathf.Repeat((IsNight ? 18f : 6f) + 12f * PeriodTime / periodDuration, 24f);
    public string ClockText
    {
        get
        {
            float h = Hour;
            int hours = Mathf.FloorToInt(h);
            int minutes = Mathf.FloorToInt((h - hours) * 60f / 15f) * 15; // relógio "pula" de 15 em 15 min
            return $"{hours:00}:{minutes:00}";
        }
    }

    public event Action<DayPeriod> OnPeriodChanged;

    // Noite de chefe: enquanto HoldNight estiver ligado, o relógio para na meia-noite e não amanhece.
    public bool HoldNight { get; set; }
    const float HoldAtFraction = 0.5f; // metade da noite = 00:00
    float fastForwardRate = 1f;

    // Faz o resto da noite passar rápido, amanhecendo em "seconds" segundos (usado quando o chefe morre).
    public void FinishNightIn(float seconds)
    {
        HoldNight = false;
        if (!IsNight) return;
        fastForwardRate = Mathf.Max(1f, TimeLeftInPeriod / Mathf.Max(0.1f, seconds));
    }

    const float OverlaySize = 44f; // em unidades: cobre a tela inteira com folga
    SpriteRenderer overlay;
    Transform player;
    float flicker;

    void Awake()
    {
        Instance = this;
        EnterPeriod(startPeriod, false);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        if (GameManager.Instance && GameManager.Instance.Player) player = GameManager.Instance.Player.transform;
        CreateOverlay();
        UpdateVisuals();
    }

    void Update()
    {
        var gm = GameManager.Instance;
        bool playing = !gm || gm.State == GameState.Playing;
        if (playing)
        {
            float dt = Time.deltaTime;
            if (IsNight) NightElapsed += dt;
            if (IsNight && HoldNight) PeriodTime = Mathf.Min(PeriodTime + dt, periodDuration * HoldAtFraction);
            else PeriodTime += dt * fastForwardRate;
            if (GameInput.DebugSkipPeriodPressed() && !(IsNight && HoldNight)) PeriodTime = periodDuration;
            if (PeriodTime >= periodDuration) EnterPeriod(IsNight ? DayPeriod.Day : DayPeriod.Night, true);
        }
        UpdateVisuals();
    }

    void EnterPeriod(DayPeriod period, bool notify)
    {
        Period = period;
        PeriodTime = 0f;
        fastForwardRate = 1f;
        HoldNight = false;
        if (period == DayPeriod.Day) DayNumber++;
        else NightNumber++;

        if (!notify) return;
        if (period == DayPeriod.Day) Enemy.VanishAll(); // o sol "queima" quem sobrou da noite
        AudioManager.Play(Sfx.LevelUp, 0.6f, 0f);
        OnPeriodChanged?.Invoke(period);
    }

    // ------------------------------------------------------------------ Visual

    // Luz de 0 (noite) a 1 (dia), com pôr do sol (17h-18h30) e amanhecer (5h-6h30).
    public float LightLevel
    {
        get
        {
            float h = Hour;
            if (h >= 6.5f && h < 17f) return 1f;
            if (h >= 17f && h < 18.5f) return Mathf.SmoothStep(1f, 0f, (h - 17f) / 1.5f);
            if (h >= 5f && h < 6.5f) return Mathf.SmoothStep(0f, 1f, (h - 5f) / 1.5f);
            return 0f;
        }
    }

    void UpdateVisuals()
    {
        float light = LightLevel;
        if (ground) ground.color = groundTint.Evaluate(light);

        if (!overlay) return;
        var c = overlayTint.Evaluate(light);
        c.a = nightDarkness * (1f - light);
        overlay.color = c;
        overlay.enabled = c.a > 0.005f;

        // A "tocha" do jogador tremula um pouco.
        flicker += Time.deltaTime;
        float s = 1f + Mathf.Sin(flicker * 7f) * 0.015f + Mathf.Sin(flicker * 13f) * 0.01f;
        overlay.transform.localScale = Vector3.one * s;
        if (player) overlay.transform.position = new Vector3(player.position.x, player.position.y, 0f);
    }

    void CreateOverlay()
    {
        const int size = 256;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color[size * size];
        float unitsPerPixel = OverlaySize / size;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x + 0.5f - size * 0.5f) * unitsPerPixel;
            float dy = (y + 0.5f - size * 0.5f) * unitsPerPixel;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(lightInnerRadius, lightOuterRadius, d));
            pixels[y * size + x] = new Color(1f, 1f, 1f, a);
        }
        tex.SetPixels(pixels);
        tex.Apply();

        var go = new GameObject("NightOverlay");
        overlay = go.AddComponent<SpriteRenderer>();
        overlay.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / OverlaySize);
        overlay.sortingOrder = 80; // acima do mundo, abaixo dos números de dano
    }

    static Gradient DefaultGroundTint()
    {
        var g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.45f, 0.52f, 0.85f), 0f),  // noite: azulado
                new GradientColorKey(new Color(1f, 0.68f, 0.5f), 0.5f),    // pôr do sol: alaranjado
                new GradientColorKey(new Color(1.0f, 1.0f, 0.92f), 1f)     // dia: claro e quente
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return g;
    }

    static Gradient DefaultOverlayTint()
    {
        var g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.02f, 0.03f, 0.12f), 0f),  // noite: azul bem escuro
                new GradientColorKey(new Color(0.35f, 0.12f, 0.08f), 0.6f), // pôr do sol: vermelho escuro
                new GradientColorKey(new Color(0.5f, 0.3f, 0.1f), 1f)
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return g;
    }
}
