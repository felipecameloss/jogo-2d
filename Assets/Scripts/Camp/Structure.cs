using System;
using System.Collections.Generic;
using UnityEngine;

public enum StructureKind { Heart, Fence, Wall, Gate, Station }

// Qualquer construção do acampamento que tem vida e pode ser destruída.
public class Structure : MonoBehaviour
{
    public static readonly List<Structure> Active = new List<Structure>();

    public StructureKind Kind { get; private set; }
    public float MaxHealth { get; private set; }
    public float Health { get; private set; }
    public float Missing => MaxHealth - Health;
    public bool IsDamaged => Health < MaxHealth;
    public bool IsDestroyed { get; private set; }

    public event Action<Structure> OnDestroyed;

    static readonly Color DamagedTint = new Color(0.45f, 0.38f, 0.34f);

    SpriteRenderer sr;
    float flashTimer;

    void Awake() => sr = GetComponent<SpriteRenderer>();
    void OnEnable() => Active.Add(this);
    void OnDisable() => Active.Remove(this);

    // Também serve para melhorar a estrutura: troca o tipo e volta com a vida cheia.
    public void Init(StructureKind kind, float maxHealth)
    {
        Kind = kind;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDestroyed || amount <= 0f) return;
        Health = Mathf.Max(0f, Health - amount);
        flashTimer = 0.1f;
        AudioManager.Play(Sfx.Hit, 0.3f);
        if (Health > 0f) return;

        IsDestroyed = true;
        FxManager.Burst(transform.position, new Color(0.7f, 0.5f, 0.3f), 18, 5f);
        FxManager.Shake(0.2f, 0.2f);
        AudioManager.Play(Sfx.EnemyDie, 0.8f);
        OnDestroyed?.Invoke(this);
    }

    public void RepairFull()
    {
        if (IsDestroyed) return;
        Health = MaxHealth;
    }

    void Update()
    {
        if (!sr) return;
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            sr.color = Color.white * 1.5f;
            return;
        }
        // Quanto mais danificada, mais escura: o estado aparece no próprio mundo.
        sr.color = Color.Lerp(DamagedTint, Color.white, MaxHealth > 0f ? Health / MaxHealth : 1f);
    }
}
