using System.Collections.Generic;
using UnityEngine;

// Mapa determinístico por dia: descarregar uma região não repõe o que foi coletado.
// Sem colliders: os pontos de coleta não prendem jogador/inimigos durante as waves.
public class DayResourceSystem : MonoBehaviour
{
    public float cellSize = 5.5f;
    public float collectRange = 1.9f;
    public float collectSeconds = 1.1f;
    public ResourceInventory Inventory { get; private set; }
    public string Prompt { get; private set; }
    public float Progress { get; private set; }
    public int ActiveNodeCount => nodes.Count;
    public int DepletedCount => depleted.Count;

    class Node
    {
        public Vector2Int Cell;
        public ResourceKind Kind;
        public bool Rich;
        public GameObject Object;
        public SpriteRenderer Renderer;
    }

    readonly Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();
    readonly HashSet<Vector2Int> depleted = new HashSet<Vector2Int>();
    readonly List<Vector2Int> removal = new List<Vector2Int>();
    readonly Sprite[] sprites = new Sprite[3];
    Node target;
    DayNightCycle cycle;
    PlayerController controller;
    Transform player;
    Vector2Int lastCell = new Vector2Int(int.MinValue, int.MinValue);
    int day;

    public static string Label(ResourceKind kind) => kind == ResourceKind.Wood ? "Madeira" : kind == ResourceKind.Stone ? "Pedra" : "Cristal";
    public static Color Tint(ResourceKind kind) => kind == ResourceKind.Wood ? new Color(0.55f, 0.88f, 0.35f) : kind == ResourceKind.Stone ? new Color(0.78f, 0.83f, 0.93f) : new Color(0.35f, 0.95f, 1f);

    void Start()
    {
        var gm = GameManager.Instance;
        cycle = DayNightCycle.Instance;
        if (!gm || !gm.Player || !cycle) { enabled = false; return; }
        player = gm.Player.transform;
        controller = gm.Player.GetComponent<PlayerController>();
        Inventory = gm.Player.GetComponent<ResourceInventory>();
        if (!Inventory) Inventory = gm.Player.gameObject.AddComponent<ResourceInventory>();
        for (int i = 0; i < sprites.Length; i++) sprites[i] = ResourceArt.Create((ResourceKind)i);
        day = cycle.DayNumber;
        cycle.OnPeriodChanged += PeriodChanged;
        gameObject.AddComponent<ResourceHUD>().source = this;
        RefreshRegion();
    }

    void PeriodChanged(DayPeriod period)
    {
        Progress = 0;
        target = null;
        if (period != DayPeriod.Day) return;
        day = cycle.DayNumber;
        depleted.Clear();
        foreach (var node in nodes.Values) Destroy(node.Object);
        nodes.Clear();
        lastCell = new Vector2Int(int.MinValue, int.MinValue);
        RefreshRegion();
    }

    void Update()
    {
        if (!player || !cycle) return;
        RefreshRegion();
        bool playing = GameManager.Instance && GameManager.Instance.State == GameState.Playing;
        Node nearest = null;
        float distance = collectRange * collectRange;
        foreach (var node in nodes.Values)
        {
            bool empty = depleted.Contains(node.Cell);
            node.Renderer.color = empty ? new Color(0.3f, 0.3f, 0.3f, 0.5f) : Color.white;
            node.Object.transform.localScale = Vector3.one * (empty ? 0.55f : node.Rich ? 1.25f : 1f);
            float d = ((Vector2)(node.Object.transform.position - player.position)).sqrMagnitude;
            if (!empty && d < distance) { nearest = node; distance = d; }
        }
        if (!playing || cycle.IsNight || (controller && controller.IsDashing))
        {
            Progress = 0;
            target = null;
            Prompt = cycle.IsNight ? "A coleta volta ao amanhecer. Sobreviva!" : "";
            return;
        }
        if (nearest != target) Progress = 0;
        target = nearest;
        if (target == null)
        {
            Prompt = "Explore: árvores, rochas e cristais • Segure E para coletar";
            return;
        }
        target.Renderer.color = Color.Lerp(Color.white, Tint(target.Kind), 0.3f + 0.2f * Mathf.Sin(Time.time * 5f));
        int amount = Yield(target.Kind) * (target.Rich ? 2 : 1);
        Prompt = $"Segure E / X (controle)  •  {Label(target.Kind)} +{amount}" + (target.Rich ? "  •  VEIO RICO!" : "");
        if (!GameInput.CollectHeld()) { Progress = 0; return; }
        float duration = Mathf.Max(0.1f, collectSeconds) * (target.Kind == ResourceKind.Crystal ? 1.5f : 1f);
        Progress = Mathf.Min(1f, Progress + Time.deltaTime / duration);
        target.Object.transform.localScale *= 1f + Mathf.Sin(Time.time * 30f) * 0.04f;
        if (Progress < 1f) return;
        depleted.Add(target.Cell);
        Inventory.Add(target.Kind, amount);
        FxManager.Burst(target.Object.transform.position, Tint(target.Kind), 10, 3f);
        FxManager.Popup(target.Object.transform.position + Vector3.up, $"+{amount} {Label(target.Kind)}", Tint(target.Kind));
        AudioManager.Play(Sfx.Pickup, 0.65f);
        Progress = 0;
        target = null;
    }

    static int Yield(ResourceKind kind) => kind == ResourceKind.Wood ? 5 : kind == ResourceKind.Stone ? 3 : 2;

    void RefreshRegion()
    {
        float size = Mathf.Max(3f, cellSize);
        var center = new Vector2Int(Mathf.FloorToInt(player.position.x / size), Mathf.FloorToInt(player.position.y / size));
        if (center == lastCell) return;
        lastCell = center;
        removal.Clear();
        foreach (var pair in nodes)
            if (Mathf.Abs(pair.Key.x - center.x) > 3 || Mathf.Abs(pair.Key.y - center.y) > 3) removal.Add(pair.Key);
        foreach (var key in removal) { Destroy(nodes[key].Object); nodes.Remove(key); }
        for (int y = center.y - 3; y <= center.y + 3; y++)
        for (int x = center.x - 3; x <= center.x + 3; x++)
        {
            var cell = new Vector2Int(x, y);
            if (nodes.ContainsKey(cell)) continue;
            int seed = unchecked(x * 73856093 ^ y * 19349663 ^ day * 83492791);
            var random = new System.Random(seed);
            int roll = random.Next(10);
            var kind = roll < 5 ? ResourceKind.Wood : roll < 8 ? ResourceKind.Stone : ResourceKind.Crystal;
            var go = new GameObject("Resource_" + kind);
            go.transform.SetParent(transform, false);
            go.transform.position = new Vector3((x + 0.25f + (float)random.NextDouble() * 0.5f) * size,
                (y + 0.25f + (float)random.NextDouble() * 0.5f) * size, 0);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites[(int)kind];
            renderer.sortingOrder = 2;
            nodes.Add(cell, new Node { Cell = cell, Kind = kind, Rich = random.Next(8) == 0, Object = go, Renderer = renderer });
        }
    }

    void OnDestroy()
    {
        if (cycle) cycle.OnPeriodChanged -= PeriodChanged;
        foreach (var sprite in sprites)
            if (sprite) { Destroy(sprite.texture); Destroy(sprite); }
    }
}
