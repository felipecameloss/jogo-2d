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
public class EnemySpawner : MonoBehaviour
{
    public SpawnEntry[] entries;

    [Header("Ritmo")]
    [Tooltip("Inimigos por segundo (eixo X = segundos de noite já passados na run).")]
    public AnimationCurve spawnsPerSecond = AnimationCurve.Linear(0f, 1f, 600f, 7f);
    [Tooltip("Multiplicador de vida dos inimigos ao longo do tempo.")]
    public AnimationCurve healthMultiplier = AnimationCurve.Linear(0f, 1f, 600f, 4f);
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

    float accumulator;
    float nextRingTime;
    float nextEliteTime;
    Camera cam;

    void Start()
    {
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

        accumulator += spawnsPerSecond.Evaluate(t) * Time.deltaTime;
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
        enemy.Init(healthMultiplier.Evaluate(t), elite, huntsPlayer);
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
