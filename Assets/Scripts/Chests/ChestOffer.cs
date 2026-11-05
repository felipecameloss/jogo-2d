using UnityEngine;

public enum ChestReward { MaxHealth, Damage, Armor, Haste, Magnet, Regeneration, GoldBonus }

// Um bônus permanente que pode sair de um baú.
public class ChestOffer
{
    public string Title { get; }
    public string Description { get; }
    public ChestReward Reward { get; }
    public float Amount { get; }

    ChestOffer(string title, string description, ChestReward reward, float amount)
    {
        Title = title;
        Description = description;
        Reward = reward;
        Amount = amount;
    }

    // power 1..3: baús mais distantes dão bônus mais fortes.
    public static ChestOffer Create(ChestReward reward, int power)
    {
        power = Mathf.Clamp(power, 1, 3);
        switch (reward)
        {
            case ChestReward.MaxHealth: return new ChestOffer("Coração de âmbar", $"+{15 * power} de vida máxima e cura imediata desse valor.", reward, 15 * power);
            case ChestReward.Damage: return new ChestOffer("Pólvora solar", $"+{10 * power}% de dano em todas as armas.", reward, 0.1f * power);
            case ChestReward.Armor: return new ChestOffer("Escamas do guardião", $"+{power} de armadura contra cada golpe.", reward, power);
            case ChestReward.Haste: return new ChestOffer("Engrenagem de vento", $"-{5 * power}% no tempo de recarga das armas.", reward, 0.05f * power);
            case ChestReward.Magnet: return new ChestOffer("Bússola magnética", $"+{power} de alcance para atrair os drops.", reward, power);
            case ChestReward.Regeneration: return new ChestOffer("Musgo da aurora", $"Recupera +{0.3f * power:0.0} de vida por segundo.", reward, 0.3f * power);
            case ChestReward.GoldBonus: return new ChestOffer("Moeda da fortuna", $"+{15 * power}% de ouro dos inimigos.", reward, 0.15f * power);
            default: throw new System.ArgumentOutOfRangeException(nameof(reward));
        }
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
        }
    }
}
