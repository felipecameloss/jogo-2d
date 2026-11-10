using System;
using System.Collections.Generic;
using UnityEngine;

// Uma compra possível numa estação.
public class StationOffer
{
    public string Title;
    public string Description;
    public int Gold;
    public Sprite Icon;
    public Action Apply;
}

// Ferraria e armaria: liberadas pelo resgate do NPC, fechadas sem ele, com compras pagas em ouro
// que continuam valendo mesmo se a estação cair.
public class StationSystem : MonoBehaviour
{
    public const int WeaponGold = 15, WeaponStep = 10;
    public const int ArmorGold = 20, ArmorStep = 10, ArmorLevels = 5;
    public const float ArmorPerLevel = 1f, HealthPerLevel = 10f;

    public CampSystem camp;

    public BuildSpot Open { get; private set; }
    public string Heading => Open ? $"{Open.StationName} • {PersonAt(Open).Title}" : "";
    public string Message { get; private set; } = "";
    public IReadOnlyList<StationOffer> Offers => offers;
    public PlayerWallet Wallet { get; private set; }

    readonly List<StationOffer> offers = new List<StationOffer>();
    PlayerStats player;
    WeaponHolder holder;
    Weapon[] catalog;
    int armorLevel;

    void Start()
    {
        player = GameManager.Instance ? GameManager.Instance.Player : FindAnyObjectByType<PlayerStats>();
        holder = player ? player.GetComponent<WeaponHolder>() : null;
        Wallet = player ? player.GetComponent<PlayerWallet>() : null;
        catalog = UpgradeManager.Catalog();
        gameObject.AddComponent<StationUI>().source = this;
    }

    LostPerson PersonAt(BuildSpot spot) =>
        camp.Person(spot.Kind == SpotKind.WeaponStation ? PersonRole.WeaponSmith : PersonRole.ArmorSmith);

    bool IsResident(BuildSpot spot)
    {
        var person = PersonAt(spot);
        return person && person.State == PersonState.Resident;
    }

    // Mantém ponto, arte da barraca e NPC de acordo com quem está no acampamento.
    void Update()
    {
        if (!camp) return;
        foreach (var spot in camp.Spots)
        {
            if (!spot.IsStation) continue;
            var person = PersonAt(spot);
            if (!person) continue;
            bool resident = person.State == PersonState.Resident;
            spot.Locked = !spot.Current && !resident;
            spot.LockedText = $"{spot.StationName}: resgate {person.Name} para construir";
            spot.ShowResident(resident);
            if (resident) person.Hidden = spot.IsBuiltStation;
        }
    }

    public string PromptFor(BuildSpot spot)
    {
        var person = PersonAt(spot);
        return IsResident(spot)
            ? $"[F / Y] Abrir a {spot.StationName.ToLower()}"
            : $"{spot.StationName} fechada • resgate {person.Name}";
    }

    public bool TryOpen(BuildSpot spot)
    {
        if (!spot || !spot.IsBuiltStation || !IsResident(spot) || !GameManager.Instance) return false;
        if (!GameManager.Instance.EnterMenu(Close)) return false;
        Open = spot;
        Message = "";
        Refresh();
        AudioManager.Play(Sfx.Select, 0.5f);
        return true;
    }

    public void Close()
    {
        Open = null;
        if (GameManager.Instance) GameManager.Instance.ExitMenu();
    }

    public void Buy(int index)
    {
        if (!Open || index < 0 || index >= offers.Count || !Wallet) return;
        var offer = offers[index];
        if (!Wallet.TrySpend(offer.Gold))
        {
            Message = $"Ouro insuficiente: {offer.Title} custa {offer.Gold} de ouro";
            AudioManager.Play(Sfx.PlayerHurt, 0.4f);
            return;
        }
        offer.Apply();
        Message = $"Comprado: {offer.Title}";
        AudioManager.Play(Sfx.LevelUp, 0.6f);
        FxManager.Burst(Open.transform.position + Vector3.up, new Color(1f, 0.8f, 0.35f), 14, 4f);
        Refresh();
    }

    void Refresh()
    {
        offers.Clear();
        if (!Open) return;
        if (Open.Kind == SpotKind.WeaponStation) WeaponOffers();
        else ArmorOffers();
    }

    // Besta sempre; Espada e Facas com 1 residente; Machado e Bombas com os 2.
    static int ResidentsNeeded(Weapon weapon)
    {
        switch (weapon.weaponName)
        {
            case "Besta": return 0;
            case "Espada":
            case "Facas": return 1;
            default: return 2;
        }
    }

    void WeaponOffers()
    {
        if (!holder) return;
        int residents = camp.Residents;
        foreach (var prefab in catalog)
        {
            if (!prefab || ResidentsNeeded(prefab) > residents) continue;
            var owned = holder.Get(prefab);
            if (owned && owned.IsMaxLevel) continue;
            int level = owned ? owned.Level + 1 : 1;
            var weapon = prefab;
            offers.Add(new StationOffer
            {
                Title = owned ? $"{weapon.weaponName} Nv {level}" : $"Aprender {weapon.weaponName}",
                Description = owned ? owned.NextLevelDescription() : weapon.description,
                Gold = WeaponGold + WeaponStep * (level - 1),
                Icon = weapon.icon,
                Apply = () => holder.Unlock(weapon),
            });
        }
    }

    void ArmorOffers()
    {
        if (!player || armorLevel >= ArmorLevels) return;
        offers.Add(new StationOffer
        {
            Title = $"Armadura Nv {armorLevel + 1}/{ArmorLevels}",
            Description = $"+{ArmorPerLevel:0} de armadura contra cada golpe e +{HealthPerLevel:0} de vida máxima.",
            Gold = ArmorGold + ArmorStep * armorLevel,
            Icon = ChestOffer.GearIcon(ChestReward.Armor),
            Apply = () =>
            {
                armorLevel++;
                player.armor += ArmorPerLevel;
                player.AddMaxHealth(HealthPerLevel);
            },
        });
    }
}
