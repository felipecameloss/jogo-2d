using UnityEngine;

// Weapon fica por último: as demais são equipamentos, sorteados por índice.
public enum ChestReward { MaxHealth, Damage, Armor, Haste, Magnet, Regeneration, GoldBonus, Weapon }

// Uma recompensa de baú: equipamento com bônus permanente ou uma arma.
public class ChestOffer
{
    public static readonly int GearCount = System.Enum.GetValues(typeof(ChestReward)).Length - 1;

    public string Title { get; }
    public string Description { get; }
    public ChestReward Reward { get; }
    public float Amount { get; }
    public Weapon WeaponPrefab { get; }

    ChestOffer(string title, string description, ChestReward reward, float amount, Weapon weapon = null)
    {
        Title = title;
        Description = description;
        Reward = reward;
        Amount = amount;
        WeaponPrefab = weapon;
    }

    // power 1..3: baús mais distantes dão bônus mais fortes.
    public static ChestOffer Create(ChestReward reward, int power)
    {
        power = Mathf.Clamp(power, 1, 3);
        switch (reward)
        {
            case ChestReward.MaxHealth: return new ChestOffer("Amuleto de carvalho", $"+{15 * power} de vida máxima e cura imediata desse valor.", reward, 15 * power);
            case ChestReward.Damage: return new ChestOffer("Pedra de amolar", $"+{10 * power}% de dano em todas as armas.", reward, 0.1f * power);
            case ChestReward.Armor: return new ChestOffer("Cota de malha", $"+{power} de armadura contra cada golpe.", reward, power);
            case ChestReward.Haste: return new ChestOffer("Luvas de couro", $"-{5 * power}% no tempo de recarga das armas.", reward, 0.05f * power);
            case ChestReward.Magnet: return new ChestOffer("Alforje", $"+{power} de alcance para atrair os drops.", reward, power);
            case ChestReward.Regeneration: return new ChestOffer("Ervas do curandeiro", $"Recupera +{0.3f * power:0.0} de vida por segundo.", reward, 0.3f * power);
            case ChestReward.GoldBonus: return new ChestOffer("Moeda da fortuna", $"+{15 * power}% de ouro dos inimigos.", reward, 0.15f * power);
            default: throw new System.ArgumentOutOfRangeException(nameof(reward));
        }
    }

    // Arma: entra no nível 1 e cada ponto de força além do primeiro sobe mais um nível.
    public static ChestOffer CreateWeapon(Weapon prefab, int power)
    {
        power = Mathf.Clamp(power, 1, 3);
        string extra = power > 1 ? $" e já sobe {power - 1} {(power == 2 ? "nível" : "níveis")}" : "";
        string description = $"Aprende {prefab.weaponName}{extra}. Se já tiver, +{power} {(power == 1 ? "nível" : "níveis")}.";
        return new ChestOffer(prefab.weaponName, description, ChestReward.Weapon, power, prefab);
    }

    public void Apply(PlayerStats player)
    {
        switch (Reward)
        {
            case ChestReward.MaxHealth: player.AddMaxHealth(Amount); break;
            case ChestReward.Damage: player.damageMultiplier += Amount; break;
            case ChestReward.Armor: player.armor += Amount; break;
            case ChestReward.Haste: player.cooldownMultiplier *= 1f - Amount; break;
            case ChestReward.Magnet: player.pickupRadius += Amount; break;
            case ChestReward.Regeneration: player.regenPerSecond += Amount; break;
            case ChestReward.GoldBonus: player.goldMultiplier += Amount; break;
            case ChestReward.Weapon:
                var holder = player.GetComponent<WeaponHolder>();
                for (int i = 0; i < Amount && holder; i++) holder.Unlock(WeaponPrefab);
                break;
        }
    }
}
