using System.Collections.Generic;
using UnityEngine;

// Lâminas que giram em volta do jogador.
public class OrbitWeapon : Weapon
{
    public OrbitBlade bladePrefab;
    public int bladeCount = 2;
    public float radius = 1.8f;
    public float rotationSpeed = 180f;
    [Tooltip("Intervalo mínimo entre dois acertos no mesmo inimigo.")]
    public float hitInterval = 0.5f;
    public float knockback = 4f;

    readonly List<OrbitBlade> blades = new List<OrbitBlade>();
    float angle;

    public float BladeDamage => Damage;

    public override void Init(PlayerStats owner)
    {
        base.Init(owner);
        RebuildBlades();
    }

    void RebuildBlades()
    {
        foreach (var b in blades) if (b) Destroy(b.gameObject);
        blades.Clear();
        for (int i = 0; i < bladeCount; i++)
        {
            var blade = Instantiate(bladePrefab, transform);
            blade.owner = this;
            blades.Add(blade);
        }
    }

    void Update()
    {
        if (!stats) return;

        angle += rotationSpeed * Time.deltaTime;
        float r = radius * Area;
        for (int i = 0; i < blades.Count; i++)
        {
            float a = angle + 360f / blades.Count * i;
            var rot = Quaternion.Euler(0f, 0f, a);
            blades[i].transform.SetPositionAndRotation(transform.position + rot * Vector3.right * r, rot);
            blades[i].transform.localScale = Vector3.one * Area;
        }
    }

    protected override void ApplyLevel(int newLevel)
    {
        switch (newLevel)
        {
            case 2: bladeCount++; RebuildBlades(); break;
            case 3: damage *= 1.4f; break;
            case 4: bladeCount++; RebuildBlades(); break;
            case 5: rotationSpeed *= 1.5f; radius *= 1.2f; break;
        }
    }

    public override string NextLevelDescription()
    {
        switch (Level + 1)
        {
            case 2: return "+1 lâmina";
            case 3: return "+40% de dano";
            case 4: return "+1 lâmina";
            case 5: return "Gira 50% mais rápido e mais longe";
            default: return "";
        }
    }
}
