using UnityEngine;

// Arremessa facas na direção em que o jogador está andando.
public class KnifeWeapon : Weapon
{
    public Projectile projectilePrefab;
    public int projectileCount = 1;
    public float projectileSpeed = 15f;
    public int pierce = 0;
    [Tooltip("Distância lateral entre facas lançadas juntas.")]
    public float spacing = 0.3f;
    public float knockback = 1.5f;

    void Update()
    {
        if (!TickCooldown()) return;

        Vector2 dir = FacingDirection;
        Vector2 side = new Vector2(-dir.y, dir.x);
        int count = projectileCount + ExtraProjectiles;
        for (int i = 0; i < count; i++)
        {
            float offset = (i - (count - 1) * 0.5f) * spacing;
            var p = Instantiate(projectilePrefab, transform.position + (Vector3)(side * offset), Quaternion.identity);
            p.Launch(dir, projectileSpeed, Damage, pierce, Area, knockback);
        }
        AudioManager.Play(Sfx.Shoot, 0.25f, 0.25f);
    }

    protected override void ApplyLevel(int newLevel)
    {
        switch (newLevel)
        {
            case 2: projectileCount++; break;
            case 3: pierce++; damage *= 1.2f; break;
            case 4: projectileCount++; break;
            case 5: damage *= 1.4f; cooldown *= 0.75f; break;
        }
    }

    public override string NextLevelDescription()
    {
        switch (Level + 1)
        {
            case 2: return "+1 faca";
            case 3: return "Atravessa +1 inimigo e +20% de dano";
            case 4: return "+1 faca";
            case 5: return "+40% de dano e arremessa 25% mais rápido";
            default: return "";
        }
    }
}
