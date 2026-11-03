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
            if (Enemy.Active.Count < maxEnemies) Spawn(PickEntry(t), RandomOffscreenPosition(), t, false);
        }

        if (ringInterval > 0f && t >= nextRingTime)
        {
            nextRingTime += ringInterval;
            SpawnRing(t);
        }

        if (eliteInterval > 0f && t >= nextEliteTime)
        {
            nextEliteTime += eliteInterval;
            Spawn(PickEntry(t), RandomOffscreenPosition(), t, true);
        }
    }

    void Spawn(SpawnEntry entry, Vector2 position, float t, bool elite)
    {
        if (entry == null || !entry.prefab) return;
        var enemy = Instantiate(entry.prefab, position, Quaternion.identity);
        enemy.Init(healthMultiplier.Evaluate(t), elite);
    }

    void SpawnRing(float t)
    {
        var entry = PickEntry(t);
        Vector2 center = cam.transform.position;
        float radius = HalfWidth() + spawnMargin;
        for (int i = 0; i < ringCount; i++)
        {
            float a = i * Mathf.PI * 2f / ringCount;
            Spawn(entry, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius, t, false);
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
