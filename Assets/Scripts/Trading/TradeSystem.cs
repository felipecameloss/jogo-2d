using System.Collections.Generic;
using UnityEngine;

// Um mercador e três baús por dia. Estado e recompensas persistem ao fechar a janela.
public class TradeSystem : MonoBehaviour
{
    public float interactionRange = 2.2f;
    public string NearbyPrompt { get; private set; }
    public string MerchantDirection { get; private set; }
    public string Heading { get; private set; }
    public string Notice { get; private set; }
    public IReadOnlyList<TradeOffer> Offers => selected == null ? null : selected.Offers;
    public bool IsOpen => selected != null && GameManager.Instance && GameManager.Instance.State == GameState.Trading;
    public PlayerWallet Wallet { get; private set; }
    public int ChestCount => chests.Count;

    class Station
    {
        public GameObject Object;
        public SpriteRenderer Renderer;
        public string Name;
        public bool Merchant, Opened;
        public TradeOffer[] Offers;
    }

    readonly List<Station> chests = new List<Station>();
    readonly List<Sprite> ownedSprites = new List<Sprite>();
    Station merchant, selected, nearest;
    PlayerStats player;
    PlayerController controller;
    DayNightCycle cycle;
    TradeUI view;
    Vector3 camp;

    void Start()
    {
        var gm = GameManager.Instance;
        cycle = DayNightCycle.Instance;
        if (!gm || !gm.Player || !cycle) { enabled = false; return; }
        player = gm.Player;
        controller = player.GetComponent<PlayerController>();
        Wallet = player.GetComponent<PlayerWallet>();
        if (!Wallet) Wallet = player.gameObject.AddComponent<PlayerWallet>();
        view = gameObject.AddComponent<TradeUI>();
        view.source = this;
        cycle.OnPeriodChanged += PeriodChanged;
        Replenish();
    }

    void PeriodChanged(DayPeriod period)
    {
        Close();
        if (period == DayPeriod.Day) Replenish();
        else if (merchant != null) merchant.Object.SetActive(false);
    }

    void Replenish()
    {
        if (merchant != null) Destroy(merchant.Object);
        foreach (var chest in chests) Destroy(chest.Object);
        chests.Clear();
        foreach (var sprite in ownedSprites) { Destroy(sprite.texture); Destroy(sprite); }
        ownedSprites.Clear();
        camp = player.transform.position;
        int day = Mathf.Max(1, cycle.DayNumber);
        var random = new System.Random(unchecked(day * 7919));
        var stock = new List<TradeOffer>
        {
            new TradeOffer("Sopa da alvorada", "Recupera 40 de vida. Só pode comprar se estiver ferido.", 10 + (day - 1) * 2, TradeReward.Heal, 40),
            CreateReward((TradeReward)(2 + random.Next(6)), 18 + (day - 1) * 4, 1),
            CreateReward((TradeReward)(1 + random.Next(7)), 25 + (day - 1) * 5, 2)
        };
        merchant = Spawn("Mercador da alvorada", camp + new Vector3(-4, 2, 0), true, 0, stock.ToArray());
        merchant.Object.SetActive(!cycle.IsNight);
        for (int tier = 0; tier < 3; tier++)
        {
            float angle = (tier * 120 + 15 + random.Next(30)) * Mathf.Deg2Rad;
            Vector3 position = camp + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (7 + tier * 4);
            int price = (15 + tier * 15) + (day - 1) * (4 + tier * 2);
            var choices = new TradeOffer[3];
            int offset = random.Next(7);
            for (int i = 0; i < choices.Length; i++)
                choices[i] = CreateReward((TradeReward)(1 + (offset + i * 2) % 7), price, tier + 1);
            chests.Add(Spawn(tier == 0 ? "Baú do viajante" : tier == 1 ? "Baú do guardião" : "Baú astral", position, false, tier, choices));
        }
    }

    public static TradeOffer CreateReward(TradeReward reward, int price, int power)
    {
        power = Mathf.Clamp(power, 1, 3);
        switch (reward)
        {
            case TradeReward.MaxHealth: return new TradeOffer("Coração de âmbar", $"+{15 * power} de vida máxima e cura imediata desse valor.", price, reward, 15 * power);
            case TradeReward.Damage: return new TradeOffer("Pólvora solar", $"+{10 * power}% ao multiplicador de dano de todas as armas.", price, reward, 0.1f * power);
            case TradeReward.Armor: return new TradeOffer("Escamas do guardião", $"+{power} de armadura contra cada golpe.", price, reward, power);
            case TradeReward.Haste: return new TradeOffer("Engrenagem de vento", $"-{5 * power}% no tempo de recarga das armas.", price, reward, 0.05f * power);
            case TradeReward.Magnet: return new TradeOffer("Bússola magnética", $"+{power} de alcance para atrair os drops dos inimigos.", price, reward, power);
            case TradeReward.Regeneration: return new TradeOffer("Musgo da aurora", $"Recupera +{0.3f * power:0.0} de vida por segundo.", price, reward, 0.3f * power);
            case TradeReward.GoldBonus: return new TradeOffer("Moeda da fortuna", $"+{15 * power}% ao multiplicador de ouro dos inimigos.", price, reward, 0.15f * power);
            default: throw new System.ArgumentOutOfRangeException(nameof(reward));
        }
    }

    Station Spawn(string name, Vector3 position, bool isMerchant, int tier, TradeOffer[] offers)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.position = position;
        var renderer = go.AddComponent<SpriteRenderer>();
        var sprite = TradeArt.Create(isMerchant, tier);
        ownedSprites.Add(sprite);
        renderer.sprite = sprite;
        renderer.sortingOrder = 3;
        return new Station { Object = go, Renderer = renderer, Merchant = isMerchant, Name = name, Offers = offers };
    }

    void Update()
    {
        if (!player || !cycle || !Wallet) return;
        float distance = merchant == null ? 0 : Vector2.Distance(player.transform.position, merchant.Object.transform.position);
        Vector3 direction = merchant == null ? Vector3.zero : merchant.Object.transform.position - player.transform.position;
        string heading = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? (direction.x > 0 ? "leste >" : "< oeste") : (direction.y > 0 ? "norte ^" : "sul v");
        MerchantDirection = cycle.IsNight ? "Mercador retorna ao amanhecer" : $"Mercador: {distance:0} m • {heading}";
        if (IsOpen) return;
        nearest = null;
        float range = interactionRange * interactionRange;
        if (merchant != null && !cycle.IsNight) Consider(merchant, ref range);
        foreach (var chest in chests) if (!chest.Opened) Consider(chest, ref range);
        NearbyPrompt = nearest == null ? "" : nearest.Merchant ? "[F / Y] Conversar com o mercador" : $"[F / Y] {nearest.Name} • {nearest.Offers[0].Price} ouro • escolher recompensa";
        if (GameManager.Instance.State != GameState.Playing || (controller && controller.IsDashing)) return;
        if (GameInput.TradePressed() && nearest != null) OpenNearest();
    }

    void Consider(Station station, ref float range)
    {
        float sqr = ((Vector2)(station.Object.transform.position - player.transform.position)).sqrMagnitude;
        if (sqr > range) return;
        range = sqr;
        nearest = station;
    }

    public bool OpenNearest()
    {
        if (nearest == null || nearest.Opened || !player || player.IsDead ||
            (nearest.Merchant && cycle.IsNight) || (controller && controller.IsDashing) ||
            Vector2.Distance(nearest.Object.transform.position, player.transform.position) > interactionRange ||
            !GameManager.Instance.EnterTrade()) return false;
        selected = nearest;
        Heading = selected.Name;
        Notice = selected.Merchant ? "Estoque diário • uma unidade de cada item" : "Escolha UMA recompensa. Você só paga ao confirmar.";
        AudioManager.Play(Sfx.Select, 0.5f);
        return true;
    }

    public bool Buy(int index)
    {
        if (!IsOpen || index < 0 || index >= selected.Offers.Length || selected.Opened) return false;
        var offer = selected.Offers[index];
        if (!offer.CanUse(player))
        {
            Notice = offer.Sold ? "Este item já foi comprado hoje." : "Sua vida já está cheia.";
            return false;
        }
        if (!offer.TryPurchase(Wallet, player))
        {
            Notice = $"Faltam {Mathf.Max(0, offer.Price - Wallet.Gold)} moedas. Derrote inimigos à noite para ganhar ouro.";
            return false;
        }
        Notice = "Comprado: " + offer.Title;
        AudioManager.Play(Sfx.LevelUp, 0.6f);
        FxManager.Burst(selected.Object.transform.position, new Color(1f, 0.8f, 0.35f), 14, 4f);
        FxManager.Popup(player.transform.position + Vector3.up, offer.Title, new Color(1f, 0.85f, 0.35f));
        if (!selected.Merchant)
        {
            selected.Opened = true;
            selected.Renderer.color = new Color(0.45f, 0.45f, 0.45f, 0.7f);
            selected.Object.transform.localScale = new Vector3(1, 0.6f, 1);
            Close();
        }
        return true;
    }

    public void Close()
    {
        selected = null;
        if (GameManager.Instance) GameManager.Instance.ExitTrade();
    }

    void OnDestroy()
    {
        if (cycle) cycle.OnPeriodChanged -= PeriodChanged;
        foreach (var sprite in ownedSprites) if (sprite) { Destroy(sprite.texture); Destroy(sprite); }
    }
}
