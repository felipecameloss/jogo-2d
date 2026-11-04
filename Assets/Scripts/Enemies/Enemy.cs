using System.Collections.Generic;
using UnityEngine;

// Inimigo básico: anda em direção ao jogador e causa dano por contato.
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    // Lista de inimigos vivos, usada pelas armas para achar alvos sem usar Find.
    public static readonly List<Enemy> Active = new List<Enemy>();

    [Header("Atributos")]
    public float maxHealth = 10f;
    public float moveSpeed = 2f;
    public float contactDamage = 5f;
    [Range(0f, 1f)] public float knockbackResistance = 0f;

    [Header("Drops")]
    public Pickup xpGemPrefab;
    public float xpValue = 1f;
    public Pickup healPrefab;
    [Range(0f, 1f)] public float healDropChance = 0.01f;
    public Pickup magnetPrefab;
    [Range(0f, 1f)] public float magnetDropChance = 0.004f;
    public Pickup goldPrefab;
    [Range(0f, 1f)] public float goldDropChance = 0.2f;
    public int goldMin = 1;
    public int goldMax = 2;
    [Tooltip("Ouro extra por noite: na noite N o valor é multiplicado por 1 + (N - 1) x isso.")]
    public float goldBonusPerNight = 0.2f;

    [Tooltip("Se ficar mais longe que isso do jogador, reaparece do outro lado (como no Vampire Survivors).")]
    public float relocateDistance = 22f;

    public bool IsDead { get; private set; }
    public bool IsElite { get; private set; }
    public bool IsBoss { get; private set; }
    public float Health => health;
    public float HealthFraction => maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f;

    // Quando true, quem controla a velocidade é outro script (ex.: o Boss durante uma investida).
    [HideInInspector] public bool externalMovement;

    // Disparado quando o inimigo morre por dano (não quando some ao amanhecer).
    public event System.Action<Enemy> Died;

    float health;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Color baseColor;
    Vector3 baseScale;
    float flashTimer;
    Vector2 knockbackVelocity;
    Transform target;
    float slowTimer;
    float slowFactor = 1f;
    PlayerStats touchingPlayer;

    static readonly Color SlowTint = new Color(0.55f, 0.85f, 1f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        baseColor = sr.color;
        baseScale = transform.localScale;
        health = maxHealth;
    }

    void OnEnable() => Active.Add(this);
    void OnDisable() => Active.Remove(this);

    void Start()
    {
        if (GameManager.Instance && GameManager.Instance.Player) target = GameManager.Instance.Player.transform;
    }

    // Chamado pelo spawner logo depois de instanciar.
    public void Init(float healthMultiplier, bool elite)
    {
        maxHealth *= healthMultiplier;
        if (elite)
        {
            IsElite = true;
            maxHealth *= 12f;
            xpValue *= 10f;
            goldDropChance = 1f;
            goldMin *= 8;
            goldMax *= 10;
            contactDamage *= 1.5f;
            knockbackResistance = Mathf.Max(knockbackResistance, 0.8f);
            healDropChance = 1f;
            transform.localScale *= 1.7f;
            baseScale = transform.localScale;
            baseColor = Color.Lerp(baseColor, new Color(1f, 0.8f, 0.2f), 0.55f);
            sr.color = baseColor;
        }
        health = maxHealth;
    }

    // Transforma este inimigo em chefe (chamado pelo spawner nas noites de chefe).
    public void InitBoss(float health, float scaleMultiplier, Color tint, float damageMultiplier, float speedMultiplier,
        float xpAmount, int goldAmount)
    {
        IsBoss = true;
        maxHealth = health;
        this.health = health;
        contactDamage *= damageMultiplier;
        moveSpeed *= speedMultiplier;
        knockbackResistance = 0.95f;
        relocateDistance = 30f;
        xpValue = xpAmount;
        goldDropChance = 1f;
        goldMin = goldMax = goldAmount;
        goldBonusPerNight = 0f;
        healDropChance = 1f;
        magnetDropChance = 1f; // ao morrer, puxa todas as gemas da tela
        transform.localScale *= scaleMultiplier;
        baseScale = transform.localScale;
        baseColor = tint;
        sr.color = baseColor;
        rb.mass = 50f; // empurra o jogador em vez de ser empurrado
    }

    public void SetTint(Color color)
    {
        baseColor = color;
        if (flashTimer <= 0f) sr.color = color;
    }

    void FixedUpdate()
    {
        if (IsDead || !target) return;

        if (touchingPlayer) touchingPlayer.TakeDamage(contactDamage);

        Vector2 toPlayer = (Vector2)target.position - rb.position;
        if (toPlayer.sqrMagnitude > relocateDistance * relocateDistance)
        {
            rb.position = (Vector2)target.position + toPlayer.normalized * (relocateDistance * 0.7f);
            return;
        }
        if (externalMovement) return;

        float speed = slowTimer > 0f ? moveSpeed * slowFactor : moveSpeed;
        rb.linearVelocity = toPlayer.normalized * speed + knockbackVelocity;
        knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, 25f * Time.fixedDeltaTime);
    }

    void Update()
    {
        if (slowTimer > 0f) slowTimer -= Time.deltaTime;
        if (flashTimer > 0f) flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f) sr.color = slowTimer > 0f ? Color.Lerp(baseColor, SlowTint, 0.6f) : baseColor;
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, 15f * Time.deltaTime);
        if (target) sr.flipX = target.position.x < transform.position.x;
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (IsDead) return;

        bool crit = false;
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        if (player) amount = player.RollDamage(amount, out crit);

        health -= amount;
        flashTimer = 0.08f;
        sr.color = Color.white;
        if (!IsBoss) transform.localScale = baseScale * 1.15f; // chefe não "pula" a cada golpe
        knockbackVelocity += knockback * (1f - knockbackResistance);

        if (crit) FxManager.Popup(transform.position + Vector3.up * 0.4f, Mathf.RoundToInt(amount) + "!", new Color(1f, 0.85f, 0.2f), 1.4f);
        else FxManager.Popup(transform.position + Vector3.up * 0.4f, Mathf.RoundToInt(amount).ToString(), Color.white);
        AudioManager.Play(Sfx.Hit, 0.35f);

        if (health <= 0f) Die();
    }

    void Die()
    {
        IsDead = true;
        Active.Remove(this);
        if (GameManager.Instance)
        {
            GameManager.Instance.RegisterKill();
            if (GameManager.Instance.Player) GameManager.Instance.Player.OnEnemyKilled();
        }

        FxManager.Burst(transform.position, baseColor, IsBoss ? 60 : IsElite ? 24 : 8, IsBoss ? 9f : 4f, IsBoss ? 0.3f : 0.18f);
        if (IsBoss) FxManager.Shake(0.6f, 0.6f);
        else if (IsElite) FxManager.Shake(0.25f, 0.25f);
        AudioManager.Play(Sfx.EnemyDie, 0.5f);

        Vector3 pos = transform.position;
        if (xpGemPrefab && !Pickup.TryMergeXP(xpValue)) Instantiate(xpGemPrefab, pos, Quaternion.identity).value = xpValue;
        if (healPrefab && Random.value < healDropChance) Instantiate(healPrefab, pos + (Vector3)Random.insideUnitCircle * 0.4f, Quaternion.identity);
        if (magnetPrefab && Random.value < magnetDropChance) Instantiate(magnetPrefab, pos + (Vector3)Random.insideUnitCircle * 0.4f, Quaternion.identity);
        if (goldPrefab && Random.value < goldDropChance) DropGold(pos);

        Died?.Invoke(this);
        Destroy(gameObject);
    }

    // Antes era OnCollisionStay2D, que o Unity chama a cada passo de física para TODO par de
    // colisores encostados — inclusive inimigo com inimigo. Com centenas de inimigos amontoados
    // à noite isso virava milhares de chamadas por frame. Agora só registramos quando o contato
    // com o jogador começa/termina e aplicamos o dano no FixedUpdate.
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (target && collision.gameObject == target.gameObject) touchingPlayer = collision.gameObject.GetComponent<PlayerStats>();
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (target && collision.gameObject == target.gameObject) touchingPlayer = null;
    }

    void DropGold(Vector3 pos)
    {
        var cycle = DayNightCycle.Instance;
        int night = cycle ? Mathf.Max(1, cycle.NightNumber) : 1;
        float multiplier = 1f + (night - 1) * goldBonusPerNight;
        if (GameManager.Instance && GameManager.Instance.Player) multiplier *= GameManager.Instance.Player.goldMultiplier;

        int amount = Mathf.Max(1, Mathf.RoundToInt(Random.Range(goldMin, goldMax + 1) * multiplier));
        var gold = Instantiate(goldPrefab, pos + (Vector3)Random.insideUnitCircle * 0.4f, Quaternion.identity);
        gold.value = amount;
    }

    // Some sem dropar nada (usado quando amanhece).
    public void Vanish()
    {
        if (IsDead) return;
        IsDead = true;
        Active.Remove(this);
        FxManager.Burst(transform.position, new Color(1f, 0.75f, 0.3f), 6, 3f);
        Destroy(gameObject);
    }

    public static void VanishAll()
    {
        for (int i = Active.Count - 1; i >= 0; i--)
            if (i < Active.Count) Active[i].Vanish();
    }

    // Deixa o inimigo mais lento por um tempo. Lentidões não somam: vale a mais forte.
    public void ApplySlow(float factor, float duration)
    {
        if (slowTimer <= 0f || factor < slowFactor) slowFactor = factor;
        slowTimer = Mathf.Max(slowTimer, duration);
    }

    // Causa dano em todos os inimigos dentro do círculo. Retorna quantos foram atingidos.
    public static int DamageInRadius(Vector2 center, float radius, float damage, float knockback, System.Action<Enemy> onHit = null)
    {
        int hits = 0;
        float r2 = radius * radius;
        // De trás pra frente: um inimigo que morre sai da lista sem bagunçar o loop.
        for (int i = Active.Count - 1; i >= 0; i--)
        {
            if (i >= Active.Count) continue;
            var e = Active[i];
            if (e.IsDead) continue;
            Vector2 offset = (Vector2)e.transform.position - center;
            if (offset.sqrMagnitude > r2) continue;
            hits++;
            onHit?.Invoke(e);
            e.TakeDamage(damage, offset.normalized * knockback);
        }
        return hits;
    }

    public static Enemy FindNearest(Vector2 from, float maxRange)
    {
        Enemy best = null;
        float bestSqr = maxRange * maxRange;
        foreach (var e in Active)
        {
            if (e.IsDead) continue;
            float d = ((Vector2)e.transform.position - from).sqrMagnitude;
            if (d < bestSqr)
            {
                bestSqr = d;
                best = e;
            }
        }
        return best;
    }
}
