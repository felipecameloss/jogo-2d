using UnityEngine;

public enum TradeReward { Heal, MaxHealth, Damage, Armor, Haste, Magnet, Regeneration, GoldBonus }

public class TradeOffer
{
    public string Title { get; }
    public string Description { get; }
    public int Price { get; }
    public TradeReward Reward { get; }
    public float Amount { get; }
    public bool Sold { get; private set; }

    public TradeOffer(string title, string description, int price, TradeReward reward, float amount)
    {
        if (price < 0) throw new System.ArgumentOutOfRangeException(nameof(price));
        if (amount <= 0 || float.IsNaN(amount) || float.IsInfinity(amount)) throw new System.ArgumentOutOfRangeException(nameof(amount));
        if (!System.Enum.IsDefined(typeof(TradeReward), reward)) throw new System.ArgumentOutOfRangeException(nameof(reward));
        if (reward == TradeReward.Haste && amount >= 1) throw new System.ArgumentOutOfRangeException(nameof(amount));
        Title = title;
        Description = description;
        Price = Mathf.Max(0, price);
        Reward = reward;
        Amount = Mathf.Max(0, amount);
    }

    public bool CanUse(PlayerStats player) => player && !player.IsDead && !Sold &&
        (Reward != TradeReward.Heal || player.CurrentHealth < player.maxHealth);

    // Todos os requisitos são checados antes do débito. Não permite duas compras da mesma oferta.
    public bool TryPurchase(PlayerWallet wallet, PlayerStats player)
    {
        if (!wallet || !CanUse(player) || !wallet.TrySpend(Price)) return false;
        Sold = true;
        switch (Reward)
        {
            case TradeReward.Heal: player.Heal(Amount); break;
            case TradeReward.MaxHealth: player.AddMaxHealth(Amount); break;
            case TradeReward.Damage: player.damageMultiplier += Amount; break;
            case TradeReward.Armor: player.armor += Amount; break;
            case TradeReward.Haste: player.cooldownMultiplier *= 1f - Amount; break;
            case TradeReward.Magnet: player.pickupRadius += Amount; break;
            case TradeReward.Regeneration: player.regenPerSecond += Amount; break;
            case TradeReward.GoldBonus: player.goldMultiplier += Amount; break;
        }
        return true;
    }
}
