using UnityEngine;

// Movimento top-down em 8 direções + dash (Left Shift).
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("Objeto filho com o sprite (para virar e 'balançar' sem mexer na física).")]
    public Transform visual;
    public float bobAmount = 0.08f;
    public float bobSpeed = 14f;

    [Header("Dash (recarga e cargas ficam no PlayerStats)")]
    public float dashSpeed = 17f;
    public float dashDuration = 0.16f;
    [Tooltip("Intervalo entre as 'sombras' deixadas pelo dash.")]
    public float afterimageInterval = 0.03f;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LastDirection { get; private set; } = Vector2.right;
    public bool IsDashing => dashTimer > 0f;
    public int DashCharges { get; private set; }
    // Tempo até recuperar a próxima carga (0 se estiver com todas).
    public float DashRechargeRemaining => DashCharges >= stats.dashCharges ? 0f : Mathf.Max(0f, stats.dashCooldown - rechargeTimer);

    Rigidbody2D rb;
    PlayerStats stats;
    Collider2D body;
    SpriteRenderer visualRenderer;
    float bobTime;

    float dashTimer;
    float rechargeTimer;
    float afterimageTimer;
    Vector2 dashDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        body = GetComponent<Collider2D>();
        if (visual) visualRenderer = visual.GetComponent<SpriteRenderer>();
        DashCharges = stats.dashCharges;
    }

    void Update()
    {
        bool canControl = !stats.IsDead && (!GameManager.Instance || GameManager.Instance.State == GameState.Playing);
        MoveInput = canControl ? GameInput.Move() : Vector2.zero;
        if (MoveInput.sqrMagnitude > 0.01f) LastDirection = MoveInput.normalized;

        UpdateDash(canControl);
        UpdateVisual();
    }

    void UpdateDash(bool canControl)
    {
        // Recarga: uma carga por vez.
        if (DashCharges < stats.dashCharges)
        {
            rechargeTimer += Time.deltaTime;
            if (rechargeTimer >= stats.dashCooldown)
            {
                rechargeTimer = 0f;
                DashCharges++;
            }
        }
        else rechargeTimer = 0f;

        if (canControl && !IsDashing && DashCharges > 0 && GameInput.DashPressed()) StartDash();

        if (!IsDashing) return;

        dashTimer -= Time.deltaTime;
        afterimageTimer -= Time.deltaTime;
        if (afterimageTimer <= 0f)
        {
            afterimageTimer = afterimageInterval;
            SpawnAfterimage();
        }
        if (dashTimer <= 0f) EndDash();
    }

    void StartDash()
    {
        DashCharges--;
        dashDirection = MoveInput.sqrMagnitude > 0.01f ? MoveInput.normalized : LastDirection;
        dashTimer = dashDuration;
        afterimageTimer = 0f;
        // Durante o dash o jogador atravessa os inimigos (e fica invencível, ver PlayerStats).
        if (body) body.isTrigger = true;
        AudioManager.Play(Sfx.Select, 0.5f, 0.2f);
    }

    void EndDash()
    {
        dashTimer = 0f;
        if (body) body.isTrigger = false;
    }

    void SpawnAfterimage()
    {
        if (!visualRenderer) return;
        var c = visualRenderer.color;
        c.a = 0.45f;
        var ghost = FxManager.Flash(visualRenderer.sprite, visual.position, visual.lossyScale, visual.lossyScale, c, 0.22f, 0f, visualRenderer.sortingOrder - 1);
        if (ghost) ghost.flipX = visualRenderer.flipX;
    }

    void UpdateVisual()
    {
        if (!visual) return;
        if (visualRenderer && Mathf.Abs(MoveInput.x) > 0.01f) visualRenderer.flipX = MoveInput.x < 0f;

        if (IsDashing)
        {
            // Estica o sprite na direção do dash.
            visual.localScale = new Vector3(1.25f, 0.8f, 1f);
            return;
        }

        // "Squash" simples ao andar: dá vida ao personagem sem precisar de animação.
        if (MoveInput.sqrMagnitude > 0.01f) bobTime += Time.deltaTime * bobSpeed;
        else bobTime = Mathf.MoveTowards(bobTime, Mathf.Round(bobTime / Mathf.PI) * Mathf.PI, Time.deltaTime * bobSpeed);
        float s = Mathf.Abs(Mathf.Sin(bobTime)) * bobAmount;
        visual.localScale = new Vector3(1f + s, 1f - s, 1f);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = IsDashing ? dashDirection * dashSpeed : MoveInput * stats.moveSpeed;
    }
}
