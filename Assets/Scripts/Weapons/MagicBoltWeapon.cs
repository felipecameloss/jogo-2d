using UnityEngine;

// Atira projéteis no inimigo mais próximo.
public class MagicBoltWeapon : Weapon
{
    public Projectile projectilePrefab;
    public int projectileCount = 1;
    public float projectileSpeed = 10f;
    public int pierce = 0;
    public float range = 15f;
    public float spreadAngle = 12f;
    public float knockback = 2f;

    void Update()
    {
        if (!CanAct) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        var target = Enemy.FindNearest(transform.position, range);
        if (!target) return; // espera aparecer alguém no alcance

        timer = Cooldown;
        Fire(((Vector2)(target.transform.position - transform.position)).normalized);
    }

    void Fire(Vector2 baseDirection)
    {
        int count = projectileCount + stats.bonusProjectiles;
        for (int i = 0; i < count; i++)
        {
            float angle = (i - (count - 1) * 0.5f) * spreadAngle;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDirection;
            var p = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            p.Launch(dir, projectileSpeed, Damage, pierce, Area, knockback);
        }
        AudioManager.Play(Sfx.Shoot, 0.35f);
    }

    protected override void ApplyLevel(int newLevel)
    {
        switch (newLevel)
        {
            case 2: projectileCount++; break;
            case 3: damage *= 1.3f; break;
            case 4: projectileCount++; break;
            case 5: pierce += 2; cooldown *= 0.8f; break;
        }
    }

    public override string NextLevelDescription()
    {
        switch (Level + 1)
        {
            case 2: return "+1 virote";
            case 3: return "+30% de dano";
            case 4: return "+1 virote";
            case 5: return "Atravessa 2 inimigos e atira 20% mais rápido";
            default: return "";
        }
    }
}
