using UnityEngine;

// Golpe horizontal na frente do jogador (e atrás, depois de upar).
public class WhipWeapon : Weapon
{
    public Sprite slashSprite;
    public Color color = new Color(1f, 1f, 1f, 0.85f);
    public float length = 3.2f;
    public float height = 0.9f;
    public float knockback = 3f;
    public bool hitsBehind;

    float facing = 1f;

    void Update()
    {
        if (controller && Mathf.Abs(controller.LastDirection.x) > 0.1f) facing = Mathf.Sign(controller.LastDirection.x);
        if (!TickCooldown()) return;

        Slash(facing);
        if (hitsBehind) Slash(-facing);
        AudioManager.Play(Sfx.Shoot, 0.3f, 0.3f);
    }

    void Slash(float side)
    {
        float len = length * Area;
        float h = height * Area;
        Vector2 center = (Vector2)transform.position + new Vector2(side * len * 0.5f, 0f);
        FxManager.Flash(slashSprite, center, new Vector2(len * 0.6f, h * 1.2f), new Vector2(len, h * 0.3f), color, 0.15f);

        const float enemyRadius = 0.3f;
        float hx = len * 0.5f + enemyRadius, hy = h * 0.5f + enemyRadius;
        for (int i = Enemy.Active.Count - 1; i >= 0; i--)
        {
            if (i >= Enemy.Active.Count) continue;
            var e = Enemy.Active[i];
            if (e.IsDead) continue;
            Vector2 offset = (Vector2)e.transform.position - center;
            if (Mathf.Abs(offset.x) <= hx && Mathf.Abs(offset.y) <= hy)
                e.TakeDamage(Damage, new Vector2(side, 0f) * knockback);
        }
    }

    protected override void ApplyLevel(int newLevel)
    {
        switch (newLevel)
        {
            case 2: hitsBehind = true; break;
            case 3: damage *= 1.3f; break;
            case 4: length *= 1.25f; break;
            case 5: damage *= 1.3f; cooldown *= 0.85f; break;
        }
    }

    public override string NextLevelDescription()
    {
        switch (Level + 1)
        {
            case 2: return "Também acerta atrás de você";
            case 3: return "+30% de dano";
            case 4: return "+25% de alcance";
            case 5: return "+30% de dano e ataca 15% mais rápido";
            default: return "";
        }
    }
}
