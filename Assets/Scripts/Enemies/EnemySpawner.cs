using System;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class SpawnEntry
{
    public Enemy prefab;
    [Tooltip("A partir de quantos segundos esse inimigo começa a aparecer.")]
    public float startTime = 0f;
    [Tooltip("Depois de quantos segundos ele para de aparecer.")]
    public float endTime = 99999f;
    public float weight = 1f;
}

// Spawna inimigos fora da tela, só durante a noite (se tiver DayNightCycle na cena).
// A dificuldade cresce com o tempo de noite acumulado, pelas curvas abaixo.
// Em algumas noites aparece só um chefe: a noite não acaba até ele ser derrotado.
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    public SpawnEntry[] entries;

    [Header("Ritmo")]
    [Tooltip("Inimigos por segundo (eixo X = segundos de noite já passados na run).")]
    public AnimationCurve spawnsPerSecond = AnimationCurve.Linear(0f, 1f, 600f, 7f);
    [Tooltip("Multiplicador de vida dos inimigos ao longo do tempo.")]
    public AnimationCurve healthMultiplier = AnimationCurve.Linear(0f, 1f, 600f, 4f);
    [Tooltip("Depois do fim da curva: inimigos/s a mais por minuto de noite.")]
    public float spawnsGrowthPerMinute = 0.5f;
    [Tooltip("Depois do fim da curva: multiplicador de vida a mais por minuto de noite.")]
    public float healthGrowthPerMinute = 0.4f;
    [Tooltip("A partir desse tempo de noite (início da Noite 4), o dano de contato também cresce.")]
    public float damageGrowthStart = 540f;
    [Tooltip("Dano de contato a mais por minuto de noite depois de damageGrowthStart (0,06 = +6%).")]
    public float damageGrowthPerMinute = 0.06f;
    public int maxEnemies = 350;
    public float spawnMargin = 1.5f;

    [Header("Acampamento")]
    [Tooltip("Longe assim da fogueira, metade dos inimigos nasce em volta dela e o resto caça o jogador.")]
    public float playerAwayDistance = 15f;
    public float campSpawnRadius = 16f;

    [Header("Eventos")]
    [Tooltip("A cada X segundos, um anel de inimigos cerca o jogador.")]
    public float ringInterval = 60f;
    public int ringCount = 24;
    [Tooltip("A cada X segundos aparece um inimigo elite (mais forte, dropa muito XP e cura).")]
    public float eliteInterval = 90f;

    [Header("Chefes")]
    [Tooltip("Noite do chefe intermediário (4 = logo depois da terceira noite). 0 desliga.")]
    public int midBossNight = 4;
    [Tooltip("Noite do chefe final (7 = logo depois da sexta noite). 0 desliga.")]
    public int bigBossNight = 7;
    [Tooltip("Segundos depois do anoitecer até o chefe aparecer.")]
    public float bossDelay = 4f;
    [Tooltip("Inimigo usado como base dos chefes. Se vazio, usa o inimigo com mais vida da lista.")]
    public Enemy bossBasePrefab;
    [Tooltip("Segundos até amanhecer depois que o chefe morre.")]
    public float dawnAfterBoss = 6f;
    [Tooltip("Escala da arte dos chefes (desenhada a 24 px por unidade). 1,5 mantém o tamanho que tinham como formas; o colisor não muda.")]
    public float bossArtScale = 1.5f;

    public string midBossName = "Golem Ancião";
    [Tooltip("Folha do chefe intermediário em Assets/Art/Resources/Enemies.")]
    public string midBossArt = "golem_anciao";
    [Tooltip("Vida base do chefe intermediário (multiplicada pela curva de vida).")]
    public float midBossHealth = 2000f;
    public float midBossBulletDamage = 10f;

    public string bigBossName = "Rei da Noite";
    [Tooltip("Folha do chefe final em Assets/Art/Resources/Enemies.")]
    public string bigBossArt = "rei_da_noite";
    [Tooltip("Vida base do chefe final (multiplicada pela curva de vida).")]
    public float bigBossHealth = 5000f;
    public float bigBossBulletDamage = 15f;

    float accumulator;
    int bossSpawnedForNight = -1;
    float nextRingTime;
    float nextEliteTime;
    Camera cam;

    void Awake() => Instance = this;

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        Boss.Defeated -= OnBossDefeated;
    }

    void Start()
    {
        Boss.Defeated += OnBossDefeated;
        cam = Camera.main;
        nextRingTime = ringInterval;
        nextEliteTime = eliteInterval;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (!gm || gm.State != GameState.Playing || !cam || entries == null || entries.Length == 0) return;

        var cycle = DayNightCycle.Instance;
        if (cycle && !cycle.IsNight) return;
        float t = cycle ? cycle.NightElapsed : gm.ElapsedTime;

        if (cycle && IsBossNight(cycle.NightNumber))
        {
            UpdateBossNight(cycle, t);
            return;
        }

        accumulator += Extended(spawnsPerSecond, t, spawnsGrowthPerMinute) * Time.deltaTime;
        while (accumulator >= 1f)
        {
            accumulator -= 1f;
            if (Enemy.Active.Count < maxEnemies) SpawnRegular(t);
        }

        if (ringInterval > 0f && t >= nextRingTime)
        {
            nextRingTime += ringInterval;
            SpawnRing(t);
        }

        if (eliteInterval > 0f && t >= nextEliteTime)
        {
            nextEliteTime += eliteInterval;
            Spawn(PickEntry(t), RandomOffscreenPosition(), t, true, PlayerAway());
        }
    }

    // ------------------------------------------------------------------ Chefes

    public bool IsBossNight(int night) => night > 0 && (night == midBossNight || night == bigBossNight);
    public BossKind BossKindForNight(int night) => night == bigBossNight ? BossKind.Big : BossKind.Mid;

    void UpdateBossNight(DayNightCycle cycle, float t)
    {
        // Noite de chefe não conta para os eventos normais (senão eles "acumulariam" para a próxima noite).
        nextRingTime = Mathf.Max(nextRingTime, t + ringInterval);
        nextEliteTime = Mathf.Max(nextEliteTime, t + eliteInterval);
        accumulator = 0f;

        if (bossSpawnedForNight == cycle.NightNumber)
        {
#if UNITY_EDITOR
            // Atalho de teste: N mata o chefe na hora.
            if (Boss.Current && GameInput.DebugSkipPeriodPressed()) Boss.Current.Enemy.TakeDamage(1e9f, Vector2.zero);
#endif
            return;
        }

        cycle.HoldNight = true; // não amanhece enquanto o chefe não aparecer e morrer
        if (cycle.PeriodTime < bossDelay) return;
        bossSpawnedForNight = cycle.NightNumber;
        SpawnBoss(BossKindForNight(cycle.NightNumber), t);
    }

    void SpawnBoss(BossKind kind, float t)
    {
        var prefab = bossBasePrefab ? bossBasePrefab : ToughestEntry();
        if (!prefab)
        {
            if (DayNightCycle.Instance) DayNightCycle.Instance.HoldNight = false;
            return;
        }

        var enemy = Instantiate(prefab, OutsideCamp(RandomOffscreenPosition()), Quaternion.identity);
        enemy.name = kind == BossKind.Big ? "BigBoss" : "MidBoss";
        // Mesma escala de vida dos inimigos comuns, inclusive depois do fim da curva.
        float hp = (kind == BossKind.Big ? bigBossHealth : midBossHealth) * Extended(healthMultiplier, t, healthGrowthPerMinute);
        if (kind == BossKind.Big)
            enemy.InitBoss(hp, 3f, bigBossArt, bossArtScale, new Color(0.45f, 0.2f, 0.7f), 3f, 1.4f, 400f, 300);
        else
            enemy.InitBoss(hp, 2.2f, midBossArt, bossArtScale, new Color(0.75f, 0.3f, 0.15f), 2f, 1.15f, 150f, 100);

        var boss = enemy.gameObject.AddComponent<Boss>();
        boss.Setup(kind, kind == BossKind.Big ? bigBossName : midBossName,
            kind == BossKind.Big ? bigBossBulletDamage : midBossBulletDamage);
    }

    void OnBossDefeated(Boss boss)
    {
        if (DayNightCycle.Instance) DayNightCycle.Instance.FinishNightIn(dawnAfterBoss);
    }

    Enemy ToughestEntry()
    {
        Enemy best = null;
        if (entries == null) return null;
        foreach (var e in entries)
            if (e.prefab && (!best || e.prefab.maxHealth > best.maxHealth)) best = e.prefab;
        return best;
    }

    // ------------------------------------------------------------------ Inimigos comuns

    // A curva vale até a última chave; depois, cresce em linha reta para a run nunca estabilizar.
    static float Extended(AnimationCurve curve, float t, float perMinute)
    {
        if (curve == null || curve.length == 0) return 1f;
        var last = curve[curve.length - 1];
        return t <= last.time ? curve.Evaluate(t) : last.value + (t - last.time) / 60f * perMinute;
    }

    bool PlayerAway()
    {
        var camp = CampSystem.Instance;
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        return camp && camp.Heart && player
            && ((Vector2)(player.transform.position - camp.Center)).sqrMagnitude > playerAwayDistance * playerAwayDistance;
    }

    void SpawnRegular(float t)
    {
        bool away = PlayerAway();
        if (away && Random.value < 0.5f)
        {
            Vector2 position = (Vector2)CampSystem.Instance.Center + Random.insideUnitCircle.normalized * campSpawnRadius;
            Spawn(PickEntry(t), position, t, false, false);
            return;
        }
        Spawn(PickEntry(t), RandomOffscreenPosition(), t, false, away);
    }

    void Spawn(SpawnEntry entry, Vector2 position, float t, bool elite, bool huntsPlayer)
    {
        if (entry == null || !entry.prefab) return;
        var enemy = Instantiate(entry.prefab, OutsideCamp(position), Quaternion.identity);
        float damage = 1f + Mathf.Max(0f, t - damageGrowthStart) / 60f * damageGrowthPerMinute;
        enemy.Init(Extended(healthMultiplier, t, healthGrowthPerMinute), elite, huntsPlayer, damage);
    }

    // Nenhum inimigo nasce dentro do anel de muralhas.
    static Vector2 OutsideCamp(Vector2 position)
    {
        var camp = CampSystem.Instance;
        if (!camp || !camp.IsReserved(position, 0f)) return position;
        Vector2 center = camp.Center;
        Vector2 offset = position - center;
        if (offset == Vector2.zero) offset = Vector2.up;
        return center + offset.normalized * (CampSystem.ReservedRadius + 1f);
    }

    void SpawnRing(float t)
    {
        var entry = PickEntry(t);
        Vector2 center = cam.transform.position;
        float radius = HalfWidth() + spawnMargin;
        bool away = PlayerAway();
        for (int i = 0; i < ringCount; i++)
        {
            float a = i * Mathf.PI * 2f / ringCount;
            Spawn(entry, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, t, false, away);
        }
    }

    SpawnEntry PickEntry(float t)
    {
        float total = 0f;
        foreach (var e in entries)
            if (e.prefab && t >= e.startTime && t < e.endTime) total += e.weight;
        if (total <= 0f) return null;

        float r = Random.value * total;
        foreach (var e in entries)
        {
            if (!e.prefab || t < e.startTime || t >= e.endTime) continue;
            r -= e.weight;
            if (r <= 0f) return e;
        }
        return null;
    }

    float HalfWidth() => cam.orthographicSize * cam.aspect;

    Vector2 RandomOffscreenPosition()
    {
        float h = cam.orthographicSize + spawnMargin;
        float w = HalfWidth() + spawnMargin;
        Vector2 c = cam.transform.position;
        switch (Random.Range(0, 4))
        {
            case 0: return c + new Vector2(Random.Range(-w, w), h);
            case 1: return c + new Vector2(Random.Range(-w, w), -h);
            case 2: return c + new Vector2(-w, Random.Range(-h, h));
            default: return c + new Vector2(w, Random.Range(-h, h));
        }
    }
}
