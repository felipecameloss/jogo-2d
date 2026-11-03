using UnityEngine;

// Explosão de gelo em volta do jogador: dano, empurrão forte e deixa os inimigos lentos.
public class FrostNovaWeapon : Weapon
{
    public Sprite ringSprite;
    public Color color = new Color(0.6f, 0.9f, 1f, 0.8f);
    public float radius = 2.8f;
    public float knockback = 7f;
    [Range(0.1f, 1f)] public float slowFactor = 0.5f;
    public float slowDuration = 2f;

    void Update()
    {
        if (!TickCooldown()) return;

        float r = radius * Area;
        float slow = slowFactor, time = slowDuration * Duration;
        int hits = Enemy.DamageInRadius(transform.position, r, Damage, knockback, e => e.ApplySlow(slow, time));

        FxManager.Flash(ringSprite, transform.position, Vector2.one * 0.5f, Vector2.one * r * 2f, color, 0.35f);
        FxManager.Burst(transform.position, color, 14, 7f, 0.14f);
        if (hits > 0) FxManager.Shake(0.08f, 0.1f);
        AudioManager.Play(Sfx.EnemyDie, 0.5f, 0.1f);
    }

    protected override void ApplyLevel(int newLevel)
    {
        switch (newLevel)
        {
            case 2: radius *= 1.2f; break;
            case 3: slowDuration += 1f; slowFactor *= 0.8f; break;
            case 4: damage *= 1.5f; break;
            case 5: cooldown *= 0.75f; radius *= 1.2f; break;
        }
    }

    public override string NextLevelDescription()
    {
        switch (Level + 1)
        {
            case 2: return "+20% de raio";
            case 3: return "Lentidão mais forte e dura +1s";
            case 4: return "+50% de dano";
            case 5: return "Explode 25% mais vezes e +20% de raio";
            default: return "";
        }
    }
}
