using UnityEngine;

// Base de todas as armas. Cada arma é um prefab que vira filho do jogador.
public abstract class Weapon : MonoBehaviour
{
    [Header("Info")]
    public string weaponName = "Arma";
    [TextArea] public string description;
    public int maxLevel = 5;

    [Header("Base")]
    public float damage = 10f;
    public float cooldown = 1f;

    public int Level { get; private set; } = 1;
    public bool IsMaxLevel => Level >= maxLevel;

    protected PlayerStats stats;
    protected float timer;

    protected float Damage => damage * (stats ? stats.damageMultiplier : 1f);
    protected float Cooldown => cooldown * (stats ? stats.cooldownMultiplier : 1f);
    protected float Area => stats ? stats.areaMultiplier : 1f;
    protected bool CanAct => stats && !stats.IsDead && (!GameManager.Instance || GameManager.Instance.State == GameState.Playing);

    public virtual void Init(PlayerStats owner)
    {
        stats = owner;
    }

    public void LevelUp()
    {
        if (IsMaxLevel) return;
        Level++;
        ApplyLevel(Level);
    }

    // O que muda ao chegar no nível "newLevel".
    protected abstract void ApplyLevel(int newLevel);

    // Texto mostrado na tela de level up para o próximo nível.
    public abstract string NextLevelDescription();
}
