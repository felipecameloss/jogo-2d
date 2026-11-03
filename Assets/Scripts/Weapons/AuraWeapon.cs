using UnityEngine;

// Área em volta do jogador que causa dano periódico em quem estiver dentro.
public class AuraWeapon : Weapon
{
    public float radius = 1.6f;
    public float knockback = 0.5f;
    [Tooltip("Sprite do círculo (diâmetro de 1 unidade).")]
    public SpriteRenderer visual;

    float pulse;

    void Update()
    {
        if (!stats) return;

        float r = radius * Area;
        if (visual)
        {
            pulse += Time.deltaTime * 4f;
            visual.transform.localScale = Vector3.one * r * 2f * (1f + Mathf.Sin(pulse) * 0.03f);
        }

        if (!CanAct) return;
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = Cooldown;

        // De trás pra frente: um inimigo que morre sai da lista sem bagunçar o loop.
        float r2 = r * r;
        Vector2 center = transform.position;
        for (int i = Enemy.Active.Count - 1; i >= 0; i--)
        {
            if (i >= Enemy.Active.Count) continue;
            var e = Enemy.Active[i];
            Vector2 offset = (Vector2)e.transform.position - center;
            if (offset.sqrMagnitude <= r2) e.TakeDamage(Damage, offset.normalized * knockback);
        }
    }

    protected override void ApplyLevel(int newLevel)
    {
        switch (newLevel)
        {
            case 2: radius *= 1.2f; break;
            case 3: damage *= 1.4f; break;
            case 4: radius *= 1.2f; break;
            case 5: damage *= 1.4f; cooldown *= 0.8f; break;
        }
    }

    public override string NextLevelDescription()
    {
        switch (Level + 1)
        {
            case 2: return "+20% de raio";
            case 3: return "+40% de dano";
            case 4: return "+20% de raio";
            case 5: return "+40% de dano e pulsa mais rápido";
            default: return "";
        }
    }
}
