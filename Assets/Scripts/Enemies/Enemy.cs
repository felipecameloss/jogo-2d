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

    [Tooltip("Se ficar mais longe que isso do jogador, reaparece do outro lado (como no Vampire Survivors).")]
    public float relocateDistance = 22f;

    public bool IsDead { get; private set; }
    public bool IsElite { get; private set; }

    float health;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Color baseColor;
    Vector3 baseScale;
    float flashTimer;
    Vector2 knockbackVelocity;
    Transform target;

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
        if (IsDead || !target) return;

        Vector2 toPlayer = (Vector2)target.position - rb.position;
        if (toPlayer.sqrMagnitude > relocateDistance * relocateDistance)
        {
            rb.position = (Vector2)target.position + toPlayer.normalized * (relocateDistance * 0.7f);
            return;
        }

        rb.linearVelocity = toPlayer.normalized * moveSpeed + knockbackVelocity;
        knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, 25f * Time.fixedDeltaTime);
    }

    void Update()
    {
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f) sr.color = baseColor;
        }
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, 15f * Time.deltaTime);
        if (target) sr.flipX = target.position.x < transform.position.x;
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (IsDead) return;

        health -= amount;
        flashTimer = 0.08f;
        sr.color = Color.white;
        transform.localScale = baseScale * 1.15f;
        knockbackVelocity += knockback * (1f - knockbackResistance);

        FxManager.Popup(transform.position + Vector3.up * 0.4f, Mathf.RoundToInt(amount).ToString(), Color.white);
        AudioManager.Play(Sfx.Hit, 0.35f);

        if (health <= 0f) Die();
    }

    void Die()
    {
        IsDead = true;
        Active.Remove(this);
        if (GameManager.Instance) GameManager.Instance.RegisterKill();

        FxManager.Burst(transform.position, baseColor, IsElite ? 24 : 8);
        if (IsElite) FxManager.Shake(0.25f, 0.25f);
        AudioManager.Play(Sfx.EnemyDie, 0.5f);

        Vector3 pos = transform.position;
        if (xpGemPrefab) Instantiate(xpGemPrefab, pos, Quaternion.identity).value = xpValue;
        if (healPrefab && Random.value < healDropChance) Instantiate(healPrefab, pos + (Vector3)Random.insideUnitCircle * 0.4f, Quaternion.identity);
        if (magnetPrefab && Random.value < magnetDropChance) Instantiate(magnetPrefab, pos + (Vector3)Random.insideUnitCircle * 0.4f, Quaternion.identity);

        Destroy(gameObject);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (IsDead) return;
        if (collision.gameObject.TryGetComponent(out PlayerStats player)) player.TakeDamage(contactDamage);
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
