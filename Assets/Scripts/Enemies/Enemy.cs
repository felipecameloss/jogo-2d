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

    [Header("Arte")]
    [Tooltip("Folha animada em Assets/Art/Resources/Enemies (sem extensão). Vazio ou sem arquivo: usa o sprite e a cor do prefab.")]
    public string art;

    public bool IsDead { get; private set; }
    public bool IsElite { get; private set; }
    public bool IsBoss { get; private set; }
    public float Health => health;
    public float HealthFraction => maxHealth > 0f ? Mathf.Clamp01(health / maxHealth) : 0f;
    // Nasceu perto do jogador longe da base: persegue só ele, sem desviar para a fogueira.
    public bool HuntsPlayer { get; private set; }

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
    PlayerStats touchingPlayer;
    // Construções encostadas agora: uma entrada por contato de colisor (ver OnCollisionEnter2D).
    readonly List<Structure> touchingStructures = new List<Structure>(2);

    // Animação da folha de arte (null sem arte).
    Sprite[] frames;
    float frameTime;
    float frameTimer;
    int frame;
    // Cor das partículas ao morrer. Sem arte é a própria cor do sprite; com arte, a cor que o prefab usava.
    Color fxColor;

    static readonly Color SlowTint = new Color(0.55f, 0.85f, 1f);
    // Com arte, o clarão branco não aparece: o golpe avermelha, como o jogador ao apanhar.
    static readonly Color HitTint = new Color(1f, 0.35f, 0.35f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        int layer = LayerMask.NameToLayer("Enemy");
        if (layer >= 0) gameObject.layer = layer;
        sr = GetComponentInChildren<SpriteRenderer>();
        fxColor = sr.color;
        ApplyArt(art, 1f);
        baseColor = sr.color;
        baseScale = transform.localScale;
        health = maxHealth;
    }

    // Troca o visual pela folha animada, desenhada a 24 px por unidade na escala pedida.
    // O colisor mantém o tamanho que tinha no mundo, para a arte não mudar o jogo.
    bool ApplyArt(string sheet, float scale)
    {
        var loaded = EnemyArt.Frames(sheet);
        if (loaded == null) return false;
        frames = loaded;
        frameTime = Mathf.Max(0.02f, EnemyArt.FrameTime(sheet));
        frame = Random.Range(0, frames.Length); // a horda não anima em sincronia
        frameTimer = 0f;
        sr.sprite = frames[frame];
        sr.color = Color.white;
        // Mesma camada do jogador, ordenado pela altura: passa atrás ou na frente de árvores e muros.
        sr.sortingOrder = 10;
        sr.spriteSortPoint = SpriteSortPoint.Pivot;
        var col = GetComponent<CircleCollider2D>();
        if (col) col.radius *= transform.localScale.x / scale;
        transform.localScale = Vector3.one * scale;
        return true;
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
            fxColor = Color.Lerp(fxColor, new Color(1f, 0.8f, 0.2f), 0.55f);
            sr.color = baseColor;
        }
        health = maxHealth;
    }

    // Transforma este inimigo em chefe (chamado pelo spawner nas noites de chefe).
    // O colisor cresce sizeMultiplier vezes, como antes; a arte do chefe (bossArt) é desenhada em artScale.
    // Sem essa arte, o próprio sprite cresce junto e recebe o tint.
    public void InitBoss(float health, float sizeMultiplier, string bossArt, float artScale, Color tint,
        float damageMultiplier, float speedMultiplier, float xpAmount, int goldAmount)
    {
        IsBoss = true;
        HuntsPlayer = true; // a luta é contra o jogador: o chefe não desvia para a fogueira
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
        transform.localScale *= sizeMultiplier;
        fxColor = tint;
        if (!ApplyArt(bossArt, artScale)) sr.color = tint;
        baseScale = transform.localScale;
        baseColor = sr.color;
        rb.mass = 50f; // empurra o jogador em vez de ser empurrado
    }

    public void SetTint(Color color)
    {
        baseColor = color;
        if (flashTimer <= 0f) sr.color = color;
    }

    void FixedUpdate()
    {
        if (IsDead) return;
        ApplyContactDamage();

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
        if (externalMovement) return;

        float speed = slowTimer > 0f ? moveSpeed * slowFactor : moveSpeed;
        rb.linearVelocity = MoveDirection(toTarget) * speed + knockbackVelocity;
        knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, 25f * Time.fixedDeltaTime);
    }

    // Dano em quem está encostado. O jogador apanha a cada passo de física (a invencibilidade dele
    // limita o ritmo); construções levam um golpe a cada structureHitInterval.
    void ApplyContactDamage()
    {
        if (touchingPlayer) touchingPlayer.TakeDamage(contactDamage);
        if (touchingStructures.Count == 0) return;

        Structure structure = null;
        for (int i = touchingStructures.Count - 1; i >= 0; i--)
        {
            if (touchingStructures[i]) structure = touchingStructures[i];
            else touchingStructures.RemoveAt(i); // construção destruída enquanto encostada
        }
        if (!structure) return;
        lastStructureContact = Time.time;
        if (Time.time < nextStructureHit) return;
        nextStructureHit = Time.time + structureHitInterval;
        structure.TakeDamage(contactDamage * structureDamageFactor);
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
        if (target) sr.flipX = target.position.x < transform.position.x; // a arte olha para a direita
        if (frames != null) Animate();
    }

    void Animate()
    {
        // Lento também anima mais devagar.
        frameTimer += Time.deltaTime * (slowTimer > 0f ? slowFactor : 1f);
        while (frameTimer >= frameTime)
        {
            frameTimer -= frameTime;
            frame = (frame + 1) % frames.Length;
        }
        sr.sprite = frames[frame];
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (IsDead) return;

        bool crit = false;
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        if (player) amount = player.RollDamage(amount, out crit);

        health -= amount;
        flashTimer = 0.08f;
        sr.color = frames != null ? HitTint : Color.white;
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

        FxManager.Burst(transform.position, fxColor, IsBoss ? 60 : IsElite ? 24 : 8, IsBoss ? 9f : 4f, IsBoss ? 0.3f : 0.18f);
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
    // com o jogador ou com uma construção começa/termina e aplicamos o dano no FixedUpdate.
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (player && collision.gameObject == player.gameObject)
        {
            touchingPlayer = collision.gameObject.GetComponent<PlayerStats>();
            return;
        }
        // Estruturas não têm Rigidbody2D: evita procurar componente a cada contato entre inimigos.
        if (!collision.rigidbody && collision.gameObject.TryGetComponent(out Structure structure))
            touchingStructures.Add(structure);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (player && collision.gameObject == player.gameObject)
        {
            touchingPlayer = null;
            return;
        }
        if (!collision.rigidbody && collision.gameObject.TryGetComponent(out Structure structure))
            touchingStructures.Remove(structure);
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
