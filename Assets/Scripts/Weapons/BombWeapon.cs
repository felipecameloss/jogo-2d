using UnityEngine;

// Larga bombas no chão que explodem depois de um tempo. Combina com o dash: deixa a bomba e foge.
public class BombWeapon : Weapon
{
    public Sprite bombSprite;
    public Sprite explosionSprite;
    [Tooltip("Tira da explosão (explosao.png). Vazia, usa o explosionSprite crescendo e sumindo.")]
    public Sprite[] explosionFrames;
    public Color color = new Color(1f, 0.35f, 0.25f);
    public int count = 1;
    public float fuseTime = 1.2f;
    public float radius = 2f;
    public float knockback = 6f;

    void Update()
    {
        if (!TickCooldown()) return;

        int total = count + ExtraProjectiles;
        for (int i = 0; i < total; i++)
        {
            Vector2 pos = (Vector2)transform.position + (i == 0 ? Vector2.zero : Random.insideUnitCircle.normalized * 1.5f);
            var go = new GameObject("Bomb");
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = bombSprite;
            sr.color = color;
            sr.sortingOrder = -3;
            go.AddComponent<Bomb>().Init(fuseTime, radius * Area, Damage, knockback, explosionSprite, explosionFrames, color);
        }
    }

    protected override void ApplyLevel(int newLevel)
    {
        switch (newLevel)
        {
            case 2: count++; break;
            case 3: radius *= 1.25f; break;
            case 4: damage *= 1.5f; break;
            case 5: count++; cooldown *= 0.8f; break;
        }
    }

    public override string NextLevelDescription()
    {
        switch (Level + 1)
        {
            case 2: return "+1 bomba";
            case 3: return "+25% de raio da explosão";
            case 4: return "+50% de dano";
            case 5: return "+1 bomba e larga 20% mais rápido";
            default: return "";
        }
    }
}
