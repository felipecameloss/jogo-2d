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

    [Tooltip("Se ficar mais longe que isso do alvo, reaparece do outro lado.")]
    public float relocateDistance = 22f;

    [Header("Ataque à base")]
    [Tooltip("Persegue o jogador se ele estiver a essa distância; senão, vai até a fogueira.")]
    public float aggroRadius = 7f;
    [Tooltip("Segundos entre golpes em construções.")]
    public float structureHitInterval = 2f;
    [Tooltip("Fração do dano de contato aplicada em construções.")]
    [Range(0f, 1f)] public float structureDamageFactor = 0.5f;

    public bool IsDead { get; private set; }
    public bool IsElite { get; private set; }
    // Nasceu perto do jogador longe da base: persegue só ele, sem desviar para a fogueira.
    public bool HuntsPlayer { get; private set; }

    float health;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Color baseColor;
    Vector3 baseScale;
    float flashTimer;
    Vector2 knockbackVelocity;
    Transform player;
    Transform heart;
    Transform target;
    float retargetTimer;
    float nextStructureHit;
    float lastStructureContact;
    Vector2 stuckCheckPosition;
    float stuckCheckTimer = 1f;
    float sidestepTimer;
    float sidestepSign = 1f;
    float slowTimer;
    float slowFactor = 1f;

    static readonly Color SlowTint = new Color(0.55f, 0.85f, 1f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        int layer = LayerMask.NameToLayer("Enemy");
        if (layer >= 0) gameObject.layer = layer;
        sr = GetComponentInChildren<SpriteRenderer>();
        baseColor = sr.color;
        baseScale = transform.localScale;
        health = maxHealth;
    }

    void OnEnable() => Active.Add(this);
    void OnDisable() => Active.Remove(this);

    void Start()
    {
        if (GameManager.Instance && GameManager.Instance.Player) player = GameManager.Instance.Player.transform;
        var camp = CampSystem.Instance;
        if (camp && camp.Heart) heart = camp.Heart.transform;
        ChooseTarget();
        if (rb) stuckCheckPosition = rb.position;
    }

    void ChooseTarget()
    {
        target = player;
        if (HuntsPlayer || !heart) return;
        bool playerNear = player && ((Vector2)(player.position - transform.position)).sqrMagnitude <= aggroRadius * aggroRadius;
        if (!playerNear) target = heart;
    }

    // Chamado pelo spawner logo depois de instanciar.
    public void Init(float healthMultiplier, bool elite, bool huntsPlayer = false, float damageMultiplier = 1f)
    {
        HuntsPlayer = huntsPlayer;
        maxHealth *= healthMultiplier;
        contactDamage *= damageMultiplier;
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

    void FixedUpdate()
    {
        if (IsDead) return;
        retargetTimer -= Time.fixedDeltaTime;
        if (retargetTimer <= 0f || !target)
        {
            retargetTimer = 0.25f;
            ChooseTarget();
        }
        if (!target) return;

        Vector2 toTarget = (Vector2)target.position - rb.position;
        if (toTarget.sqrMagnitude > relocateDistance * relocateDistance)
        {
            rb.position = (Vector2)target.position + toTarget.normalized * (relocateDistance * 0.7f);
            return;
        }

        float speed = slowTimer > 0f ? moveSpeed * slowFactor : moveSpeed;
        rb.linearVelocity = MoveDirection(toTarget) * speed + knockbackVelocity;
        knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, 25f * Time.fixedDeltaTime);
    }

    // Sem pathfinding: anda reto e, se ficar parado contra árvore ou baú, desvia de lado por um instante.
    Vector2 MoveDirection(Vector2 toTarget)
    {
        Vector2 direction = toTarget.normalized;
        stuckCheckTimer -= Time.fixedDeltaTime;
        if (stuckCheckTimer <= 0f)
        {
            stuckCheckTimer = 1f;
            bool attacking = Time.time - lastStructureContact < 0.5f;
            bool arrived = toTarget.sqrMagnitude < 2.25f;
            if (!attacking && !arrived && (rb.position - stuckCheckPosition).sqrMagnitude < 0.04f)
            {
                sidestepTimer = 0.6f;
                sidestepSign = Random.value < 0.5f ? -1f : 1f;
            }
            stuckCheckPosition = rb.position;
        }
        if (sidestepTimer <= 0f) return direction;
        sidestepTimer -= Time.fixedDeltaTime;
        return new Vector2(-direction.y, direction.x) * sidestepSign;
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
        transform.localScale = baseScale * 1.15f;
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

        FxManager.Burst(transform.position, baseColor, IsElite ? 24 : 8);
        if (IsElite) FxManager.Shake(0.25f, 0.25f);
        AudioManager.Play(Sfx.EnemyDie, 0.5f);

        Vector3 pos = transform.position;
        if (xpGemPrefab) Instantiate(xpGemPrefab, pos, Quaternion.identity).value = xpValue;
        if (healPrefab && Random.value < healDropChance) Instantiate(healPrefab, pos + (Vector3)Random.insideUnitCircle * 0.4f, Quaternion.identity);
        if (magnetPrefab && Random.value < magnetDropChance) Instantiate(magnetPrefab, pos + (Vector3)Random.insideUnitCircle * 0.4f, Quaternion.identity);
        if (goldPrefab && Random.value < goldDropChance) DropGold(pos);

        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (IsDead) return;
        if (collision.gameObject.TryGetComponent(out PlayerStats stats))
        {
            stats.TakeDamage(contactDamage);
            return;
        }
        // Estruturas não têm Rigidbody2D: evita procurar componente a cada contato entre inimigos.
        if (collision.rigidbody || !collision.gameObject.TryGetComponent(out Structure structure)) return;
        lastStructureContact = Time.time;
        if (Time.time < nextStructureHit) return;
        nextStructureHit = Time.time + structureHitInterval;
        structure.TakeDamage(contactDamage * structureDamageFactor);
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
