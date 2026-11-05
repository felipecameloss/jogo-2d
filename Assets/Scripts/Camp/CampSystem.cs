using System.Collections.Generic;
using UnityEngine;

// O acampamento: fogueira no centro, anel de muralhas com portões e pontos de estação.
// Tudo é criado por código, sem depender da cena.
public class CampSystem : MonoBehaviour
{
    public const float ReservedRadius = 7f;
    public const float HeartHealth = 300f;
    public const int RingSpots = 16;
    public const float RingRadius = 5.5f;
    const float CenterOffset = 1.5f; // O jogador nasce ao lado da fogueira, não em cima dela.

    public static CampSystem Instance { get; private set; }

    public float interactionRange = 2f;

    public Vector3 Center { get; private set; }
    public Structure Heart { get; private set; }
    public BuildSpot NearestSpot { get; private set; }
    public string Prompt { get; private set; } = "";
    public ResourceInventory Inventory { get; private set; }
    // Quando há ação disponível, o F é do acampamento e não abre baús próximos.
    public bool HasTarget => NearestSpot && NearestSpot.HasAction;
    public IReadOnlyList<BuildSpot> Spots => spots;

    readonly List<BuildSpot> spots = new List<BuildSpot>();
    PlayerStats player;
    PlayerController controller;
    public Collider2D PlayerCollider { get; private set; }
    float baseHeartScale = 1f;

    // Baús e recursos precisam do centro já no Awake, seja qual for a ordem dos componentes.
    public static CampSystem Ensure(GameObject host)
    {
        if (Instance) return Instance;
        var existing = host.GetComponent<CampSystem>();
        return existing ? existing : host.AddComponent<CampSystem>(); // O Awake define Instance.
    }

    void Awake()
    {
        Instance = this;
        player = FindAnyObjectByType<PlayerStats>();
        if (!player) { enabled = false; return; }
        controller = player.GetComponent<PlayerController>();
        PlayerCollider = player.GetComponent<Collider2D>();
        Center = player.transform.position + Vector3.up * CenterOffset;
        BuildHeart();
        BuildRing();
        BuildStations();
    }

    void Start()
    {
        if (!player) return;
        Inventory = player.GetComponent<ResourceInventory>();
        if (!Inventory) Inventory = player.gameObject.AddComponent<ResourceInventory>();
        gameObject.AddComponent<CampHUD>().source = this;
    }

    BuildSpot NewSpot(SpotKind kind, Vector3 position, float rotation, Vector2 size)
    {
        var go = new GameObject("BuildSpot_" + kind);
        go.transform.SetParent(transform, false);
        go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, rotation));
        var spot = go.AddComponent<BuildSpot>();
        spot.Setup(kind, size);
        spots.Add(spot);
        return spot;
    }

    void BuildHeart()
    {
        var spot = NewSpot(SpotKind.Heart, Center, 0f, new Vector2(1.3f, 0.7f));
        var go = new GameObject("Fogueira");
        go.transform.SetParent(spot.transform, false);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = CampArt.Get("heart");
        renderer.sortingOrder = 10;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        go.AddComponent<BoxCollider2D>().size = new Vector2(1.3f, 0.7f);
        Heart = go.AddComponent<Structure>();
        Heart.Init(StructureKind.Heart, HeartHealth);
        Heart.OnDestroyed += HeartDestroyed;
        baseHeartScale = go.transform.localScale.x;
        spot.Adopt(Heart);
    }

    void BuildRing()
    {
        var size = new Vector2(2.2f, 0.6f);
        for (int i = 0; i < RingSpots; i++)
        {
            float degrees = i * 360f / RingSpots;
            float radians = degrees * Mathf.Deg2Rad;
            var position = Center + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * RingRadius;
            NewSpot(i % 4 == 0 ? SpotKind.Gate : SpotKind.Wall, position, degrees + 90f, size);
        }
    }

    void BuildStations()
    {
        var size = new Vector2(1.7f, 1f);
        var weapon = NewSpot(SpotKind.WeaponStation, Center + new Vector3(-2.7f, 1.3f, 0f), 0f, size);
        var armor = NewSpot(SpotKind.ArmorStation, Center + new Vector3(2.7f, 1.3f, 0f), 0f, size);
        weapon.Locked = armor.Locked = true;
        weapon.LockedText = "Ferraria: resgate o ferreiro para construir";
        armor.LockedText = "Armaria: resgate a ferreira para construir";
    }

    void HeartDestroyed(Structure heart)
    {
        var cycle = DayNightCycle.Instance;
        string reason = cycle && cycle.IsNight ? $"A FOGUEIRA SE APAGOU NA NOITE {cycle.NightNumber}" : "A FOGUEIRA SE APAGOU";
        if (GameManager.Instance) GameManager.Instance.EndRun(reason);
    }

    public bool IsReserved(Vector3 position, float radius)
    {
        float limit = ReservedRadius + radius;
        return ((Vector2)(position - Center)).sqrMagnitude < limit * limit;
    }

    void Update()
    {
        if (!player || !Inventory) return;
        if (Heart) Heart.transform.localScale = Vector3.one * (baseHeartScale + Mathf.Sin(Time.time * 9f) * 0.04f);

        bool playing = GameManager.Instance && GameManager.Instance.State == GameState.Playing;
        NearestSpot = null;
        Prompt = "";
        if (!playing || player.IsDead) return;

        float best = interactionRange * interactionRange;
        foreach (var spot in spots)
        {
            if (!spot.Locked && !spot.HasAction) continue;
            float distance = ((Vector2)(spot.transform.position - player.transform.position)).sqrMagnitude;
            if (distance < best) { best = distance; NearestSpot = spot; }
        }
        if (!NearestSpot) return;
        Prompt = NearestSpot.ActionText(Inventory);
        if (NearestSpot.HasAction && !(controller && controller.IsDashing) && GameInput.InteractPressed())
            NearestSpot.TryBuild(Inventory);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
