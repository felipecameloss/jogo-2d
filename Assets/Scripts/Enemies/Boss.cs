using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BossKind { Mid, Big }

// Padrões de ataque dos chefes. Fica junto do Enemy (que cuida de vida, dano, contato e drops).
// - Chefe intermediário: persegue, investe contra o jogador e solta anéis de tiros.
// - Chefe final: tudo isso mais forte, espiral de tiros, leque mirado e fúria abaixo de 50% de vida.
[RequireComponent(typeof(Enemy))]
public class Boss : MonoBehaviour
{
    public static Boss Current { get; private set; }
    public static event Action<Boss> Spawned;
    public static event Action<Boss> Defeated;

    public BossKind Kind { get; private set; }
    public string DisplayName { get; private set; }
    public Enemy Enemy { get; private set; }
    public bool Enraged { get; private set; }
    public float HealthFraction => Enemy ? Enemy.HealthFraction : 0f;

    Rigidbody2D rb;
    Transform target;
    float bulletDamage;
    Color baseTint;

    static readonly Color TelegraphColor = new Color(1f, 0.2f, 0.2f, 0.45f);

    public void Setup(BossKind kind, string displayName, float bulletDamage)
    {
        Kind = kind;
        DisplayName = displayName;
        this.bulletDamage = bulletDamage;
    }

    void Awake()
    {
        Enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Current = this;
        baseTint = GetComponentInChildren<SpriteRenderer>().color;
        var gm = GameManager.Instance;
        if (gm && gm.Player) target = gm.Player.transform;
        Enemy.Died += OnDied;
        Spawned?.Invoke(this);
        FxManager.Shake(0.35f, 0.5f);
        AudioManager.Play(Sfx.LevelUp, 1f, 0f);
        StartCoroutine(Kind == BossKind.Mid ? MidBossLoop() : BigBossLoop());
    }

    void OnDestroy()
    {
        if (Current == this) Current = null;
        if (Enemy) Enemy.Died -= OnDied;
    }

    void OnDied(Enemy e)
    {
        StopAllCoroutines();
        Defeated?.Invoke(this);
    }

    void Update()
    {
        // Chefe final entra em fúria com metade da vida: mais rápido e ataca com mais frequência.
        if (Kind == BossKind.Big && !Enraged && HealthFraction <= 0.5f)
        {
            Enraged = true;
            Enemy.moveSpeed *= 1.35f;
            FxManager.Popup(transform.position + Vector3.up * 1.5f, "FÚRIA!", new Color(1f, 0.3f, 0.3f), 2.2f);
            FxManager.Shake(0.4f, 0.4f);
            AudioManager.Play(Sfx.PlayerHurt, 1f, 0f);
        }
        if (Enraged)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 8f);
            Enemy.SetTint(Color.Lerp(baseTint, new Color(1f, 0.15f, 0.15f), 0.35f + 0.3f * pulse));
        }
    }

    // ------------------------------------------------------------------ Padrões

    IEnumerator MidBossLoop()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            yield return new WaitForSeconds(2.5f);
            yield return Charge(1.6f, 13f, 0.55f);
            yield return new WaitForSeconds(1.2f);
            RingBurst(12, 5f, Random.Range(0f, 360f));
            yield return new WaitForSeconds(2f);
            RingBurst(12, 5f, Random.Range(0f, 360f));
        }
    }

    IEnumerator BigBossLoop()
    {
        yield return new WaitForSeconds(2f);
        int step = 0;
        while (true)
        {
            yield return new WaitForSeconds(Enraged ? 1.1f : 1.8f);
            switch (step++ % 3)
            {
                case 0:
                    int charges = Enraged ? 3 : 2;
                    for (int i = 0; i < charges; i++)
                        yield return Charge(Enraged ? 0.55f : 0.8f, Enraged ? 17f : 15f, 0.5f);
                    break;
                case 1:
                    yield return Spiral(Enraged ? 6 : 4, 16, 5.5f);
                    break;
                case 2:
                    yield return AimedFans(Enraged ? 4 : 3, Enraged ? 7 : 5, 7f);
                    break;
            }
        }
    }

    // Para, mostra uma linha vermelha de aviso e dispara na direção do jogador.
    IEnumerator Charge(float telegraph, float speed, float duration)
    {
        if (!target) yield break;
        Enemy.externalMovement = true;
        rb.linearVelocity = Vector2.zero;

        Vector2 dir = ((Vector2)target.position - rb.position).normalized;
        float len = speed * duration;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        FxManager.Flash(null, rb.position + dir * len * 0.5f, new Vector2(len, 0.5f), new Vector2(len, 0.15f),
            TelegraphColor, telegraph, angle, 85);

        // Fica parado enquanto "carrega" a investida.
        float t = 0f;
        while (t < telegraph)
        {
            t += Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            yield return null;
        }

        t = 0f;
        AudioManager.Play(Sfx.Shoot, 1f, 0f);
        while (t < duration)
        {
            t += Time.fixedDeltaTime;
            rb.linearVelocity = dir * speed;
            yield return new WaitForFixedUpdate();
        }
        rb.linearVelocity = Vector2.zero;
        Enemy.externalMovement = false;
    }

    void RingBurst(int count, float speed, float offsetDegrees)
    {
        Color c = Kind == BossKind.Big ? new Color(0.85f, 0.4f, 1f) : new Color(1f, 0.55f, 0.2f);
        for (int i = 0; i < count; i++)
        {
            float a = (offsetDegrees + i * 360f / count) * Mathf.Deg2Rad;
            EnemyBullet.Fire(transform.position, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed, bulletDamage, c);
        }
        AudioManager.Play(Sfx.Shoot, 0.8f, 0.05f);
    }

    IEnumerator Spiral(int waves, int perWave, float speed)
    {
        float offset = Random.Range(0f, 360f);
        for (int w = 0; w < waves; w++)
        {
            RingBurst(perWave, speed, offset);
            offset += 360f / perWave * 0.5f; // cada onda gira meio espaço: forma uma espiral
            yield return new WaitForSeconds(0.3f);
        }
    }

    // Leque de tiros mirado no jogador, algumas vezes seguidas.
    IEnumerator AimedFans(int volleys, int bullets, float speed)
    {
        const float spread = 50f;
        for (int v = 0; v < volleys; v++)
        {
            if (!target) yield break;
            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            for (int i = 0; i < bullets; i++)
            {
                float a = (baseAngle - spread * 0.5f + spread * i / (bullets - 1)) * Mathf.Deg2Rad;
                EnemyBullet.Fire(transform.position, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speed, bulletDamage,
                    new Color(1f, 0.3f, 0.6f), 0.3f);
            }
            AudioManager.Play(Sfx.Shoot, 0.8f, 0.05f);
            yield return new WaitForSeconds(0.45f);
        }
    }
}
