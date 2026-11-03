using System.Collections.Generic;
using UnityEngine;

// Raios caem em inimigos aleatórios perto do jogador.
public class LightningWeapon : Weapon
{
    public int strikes = 2;
    public float range = 9f;
    [Tooltip("Raio da explosão de cada raio.")]
    public float strikeRadius = 0.8f;
    public float knockback = 1f;
    [Tooltip("Sprite do brilho no ponto de impacto (opcional).")]
    public Sprite impactSprite;
    public Color color = new Color(0.65f, 0.85f, 1f);

    static readonly List<Enemy> candidates = new List<Enemy>();

    void Update()
    {
        if (!TickCooldown()) return;

        candidates.Clear();
        float r2 = range * range;
        foreach (var e in Enemy.Active)
            if (!e.IsDead && ((Vector2)(e.transform.position - transform.position)).sqrMagnitude <= r2) candidates.Add(e);

        if (candidates.Count == 0)
        {
            timer = 0.2f; // ninguém por perto: tenta de novo logo
            return;
        }

        int count = strikes + ExtraProjectiles;
        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int index = Random.Range(0, candidates.Count);
            Vector2 pos = candidates[index].transform.position;
            candidates.RemoveAt(index);
            Strike(pos);
        }
        AudioManager.Play(Sfx.Hit, 0.6f, 0.3f);
    }

    void Strike(Vector2 pos)
    {
        float r = strikeRadius * Area;
        // Desenha o raio "caindo do céu" e o brilho no chão.
        FxManager.Flash(null, pos + Vector2.up * 4f, new Vector2(0.22f, 8f), new Vector2(0.05f, 8f), color, 0.15f, 0f, 45);
        FxManager.Flash(impactSprite, pos, Vector2.one * r, Vector2.one * r * 2.4f, new Color(color.r, color.g, color.b, 0.8f), 0.25f);
        FxManager.Burst(pos, color, 5, 4f, 0.12f);
        Enemy.DamageInRadius(pos, r, Damage, knockback);
    }

    protected override void ApplyLevel(int newLevel)
    {
        switch (newLevel)
        {
            case 2: strikes++; break;
            case 3: damage *= 1.3f; strikeRadius *= 1.25f; break;
            case 4: strikes++; break;
            case 5: strikes++; cooldown *= 0.8f; break;
        }
    }

    public override string NextLevelDescription()
    {
        switch (Level + 1)
        {
            case 2: return "+1 raio";
            case 3: return "+30% de dano e explosão maior";
            case 4: return "+1 raio";
            case 5: return "+1 raio e cai 20% mais rápido";
            default: return "";
        }
    }
}
