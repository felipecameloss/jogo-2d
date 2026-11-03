using UnityEngine;

// Joga frascos que criam poças venenosas embaixo dos inimigos.
public class FlaskWeapon : Weapon
{
    public Sprite zoneSprite;
    public Color color = new Color(0.45f, 1f, 0.35f, 0.45f);
    public int count = 1;
    public float range = 8f;
    public float zoneRadius = 1.3f;
    public float zoneDuration = 3f;
    public float tickInterval = 0.4f;

    void Update()
    {
        if (!TickCooldown()) return;

        int total = count + ExtraProjectiles;
        for (int i = 0; i < total; i++)
        {
            // Cai em um inimigo aleatório no alcance; se não tiver ninguém, perto do jogador.
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * range * 0.5f;
            if (Enemy.Active.Count > 0)
            {
                var e = Enemy.Active[Random.Range(0, Enemy.Active.Count)];
                if (((Vector2)(e.transform.position - transform.position)).sqrMagnitude <= range * range) pos = e.transform.position;
            }

            var go = new GameObject("PoisonZone");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = zoneSprite;
            sr.color = color;
            sr.sortingOrder = -4;
            go.AddComponent<DamageZone>().Init(zoneRadius * Area, Damage, zoneDuration * Duration, tickInterval);
            FxManager.Burst(pos, color, 6, 3f, 0.14f);
        }
    }

    protected override void ApplyLevel(int newLevel)
    {
        switch (newLevel)
        {
            case 2: count++; break;
            case 3: zoneRadius *= 1.25f; break;
            case 4: damage *= 1.4f; break;
            case 5: count++; zoneDuration *= 1.3f; break;
        }
    }

    public override string NextLevelDescription()
    {
        switch (Level + 1)
        {
            case 2: return "+1 frasco";
            case 3: return "+25% de tamanho da poça";
            case 4: return "+40% de dano";
            case 5: return "+1 frasco e poças duram 30% mais";
            default: return "";
        }
    }
}
