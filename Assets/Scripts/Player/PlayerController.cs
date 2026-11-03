using UnityEngine;

// Movimento top-down em 8 direções.
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("Objeto filho com o sprite (para virar e 'balançar' sem mexer na física).")]
    public Transform visual;
    public float bobAmount = 0.08f;
    public float bobSpeed = 14f;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LastDirection { get; private set; } = Vector2.right;

    Rigidbody2D rb;
    PlayerStats stats;
    SpriteRenderer visualRenderer;
    float bobTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        if (visual) visualRenderer = visual.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        MoveInput = stats.IsDead ? Vector2.zero : GameInput.Move();
        if (MoveInput.sqrMagnitude > 0.01f) LastDirection = MoveInput.normalized;

        if (!visual) return;
        if (visualRenderer && Mathf.Abs(MoveInput.x) > 0.01f) visualRenderer.flipX = MoveInput.x < 0f;

        // "Squash" simples ao andar: dá vida ao personagem sem precisar de animação.
        if (MoveInput.sqrMagnitude > 0.01f) bobTime += Time.deltaTime * bobSpeed;
        else bobTime = Mathf.MoveTowards(bobTime, Mathf.Round(bobTime / Mathf.PI) * Mathf.PI, Time.deltaTime * bobSpeed);
        float s = Mathf.Abs(Mathf.Sin(bobTime)) * bobAmount;
        visual.localScale = new Vector3(1f + s, 1f - s, 1f);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = MoveInput * stats.moveSpeed;
    }
}
