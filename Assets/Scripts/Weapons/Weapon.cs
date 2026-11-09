using UnityEngine;

// Base de todas as armas. Cada arma é um prefab que vira filho do jogador.
public abstract class Weapon : MonoBehaviour
{
    [Header("Info")]
    public string weaponName = "Arma";
    [TextArea] public string description;
    [Tooltip("Ícone mostrado no baú e na ferraria (Assets/Art/Resources/Weapons/icone_*).")]
    public Sprite icon;
    public int maxLevel = 5;

    [Header("Base")]
    public float damage = 10f;
    public float cooldown = 1f;

    public int Level { get; private set; } = 1;
    public bool IsMaxLevel => Level >= maxLevel;

    protected PlayerStats stats;
    protected float timer;
    protected PlayerController controller;

    // Conta a recarga. Retorna true no frame em que a arma deve atacar.
    protected bool TickCooldown()
    {
        if (!CanAct) return false;
        timer -= Time.deltaTime;
        if (timer > 0f) return false;
        timer = Cooldown;
        return true;
    }

    protected float Damage => damage * (stats ? stats.damageMultiplier : 1f);
    protected float Cooldown => cooldown * (stats ? stats.cooldownMultiplier : 1f);
    protected float Area => stats ? stats.areaMultiplier : 1f;
    protected float Duration => stats ? stats.durationMultiplier : 1f;
    protected int ExtraProjectiles => stats ? stats.bonusProjectiles : 0;
    protected Vector2 FacingDirection => controller ? controller.LastDirection : Vector2.right;
    // De dia não há monstros: as armas descansam e só voltam a atacar quando anoitece.
    protected bool CanAct => stats && !stats.IsDead && IsNight
        && (!GameManager.Instance || GameManager.Instance.State == GameState.Playing);
    static bool IsNight => !DayNightCycle.Instance || DayNightCycle.Instance.IsNight;

    public virtual void Init(PlayerStats owner)
    {
        stats = owner;
        controller = owner ? owner.GetComponent<PlayerController>() : null;
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
