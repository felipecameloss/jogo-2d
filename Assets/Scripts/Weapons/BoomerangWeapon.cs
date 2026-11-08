using UnityEngine;

// Lança bumerangues que vão até o inimigo mais próximo e voltam, acertando nas duas passadas.
public class BoomerangWeapon : Weapon
{
    public Sprite sprite;
    public Color color = new Color(1f, 0.75f, 0.35f);
    public int count = 1;
    public float speed = 11f;
    [Tooltip("Tempo indo antes de começar a voltar.")]
    public float outTime = 0.55f;
    public float size = 0.7f;
    public float spreadAngle = 30f;
    public float knockback = 3f;

    void Update()
    {
        if (!TickCooldown()) return;

        var target = Enemy.FindNearest(transform.position, 12f);
        Vector2 baseDir = target ? ((Vector2)(target.transform.position - transform.position)).normalized : FacingDirection;

        int total = count + ExtraProjectiles;
        for (int i = 0; i < total; i++)
        {
            float angle = (i - (total - 1) * 0.5f) * spreadAngle;
            Vector2 dir = Quaternion.Euler(0f, 0f, angle) * baseDir;

            var go = new GameObject("Boomerang");
            go.transform.position = transform.position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 6;
            go.AddComponent<Boomerang>().Launch(transform, dir, speed, outTime, Damage, size * Area, knockback);
        }
        AudioManager.Play(Sfx.Shoot, 0.3f, 0.2f);
    }

    protected override void ApplyLevel(int newLevel)
    {
        switch (newLevel)
        {
            case 2: count++; break;
            case 3: damage *= 1.3f; break;
            case 4: count++; break;
            case 5: damage *= 1.3f; speed *= 1.3f; size *= 1.3f; break;
        }
    }

    public override string NextLevelDescription()
    {
        switch (Level + 1)
        {
            case 2: return "+1 machado";
            case 3: return "+30% de dano";
            case 4: return "+1 machado";
            case 5: return "+30% de dano, maior e mais rápido";
            default: return "";
        }
    }
}
