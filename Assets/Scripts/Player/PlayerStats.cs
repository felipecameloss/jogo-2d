using UnityEngine;

// Atributos do jogador. Os upgrades de level up mexem nestes valores.
public class PlayerStats : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 100f;
    public float regenPerSecond = 0f;
    [Tooltip("Tempo invencível depois de levar dano.")]
    public float invincibilityTime = 0.5f;

    [Header("Movimento")]
    public float moveSpeed = 5f;

    [Header("Ataque (multiplicadores aplicados em todas as armas)")]
    public float damageMultiplier = 1f;
    public float cooldownMultiplier = 1f;
    public float areaMultiplier = 1f;
    public int bonusProjectiles = 0;

    [Header("Coleta")]
    public float pickupRadius = 2f;

    [Header("Feedback")]
    public SpriteRenderer visual;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    float invincibleTimer;
    Color baseColor;

    void Awake()
    {
        CurrentHealth = maxHealth;
        if (!visual) visual = GetComponentInChildren<SpriteRenderer>();
        if (visual) baseColor = visual.color;
    }

    void Update()
    {
        if (IsDead) return;

        if (regenPerSecond > 0f) Heal(regenPerSecond * Time.deltaTime, false);

        if (invincibleTimer > 0f)
        {
            invincibleTimer -= Time.deltaTime;
            // Pisca enquanto está invencível.
            bool on = Mathf.FloorToInt(invincibleTimer * 20f) % 2 == 0;
            if (visual) visual.color = invincibleTimer > 0f && on ? new Color(1f, 0.35f, 0.35f) : baseColor;
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || invincibleTimer > 0f) return;

        CurrentHealth -= amount;
        invincibleTimer = invincibilityTime;

        FxManager.Popup(transform.position + Vector3.up * 0.6f, Mathf.RoundToInt(amount).ToString(), new Color(1f, 0.3f, 0.3f), 1.3f);
        FxManager.Shake(0.18f, 0.2f);
        AudioManager.Play(Sfx.PlayerHurt, 0.8f);

        if (CurrentHealth <= 0f) Die();
    }

    public void Heal(float amount, bool showFeedback = true)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        if (showFeedback)
            FxManager.Popup(transform.position + Vector3.up * 0.6f, "+" + Mathf.RoundToInt(amount), new Color(0.4f, 1f, 0.4f), 1.2f);
    }

    public void AddMaxHealth(float amount)
    {
        maxHealth += amount;
        Heal(amount);
    }

    void Die()
    {
        CurrentHealth = 0f;
        IsDead = true;
        if (visual) visual.color = new Color(0.4f, 0.4f, 0.4f);
        FxManager.Burst(transform.position, baseColor, 25, 6f, 0.25f);
        FxManager.Shake(0.4f, 0.4f);
        if (GameManager.Instance) GameManager.Instance.EndRun(false);
    }
}
