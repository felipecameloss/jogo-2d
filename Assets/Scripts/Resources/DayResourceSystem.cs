using System.Collections.Generic;
using UnityEngine;

// Mapa determinístico por dia: descarregar uma região não repõe o que foi coletado.
// Árvores e pedras têm colisão só na base (tronco/pé da pedra): dá para passar atrás da copa.
public class DayResourceSystem : MonoBehaviour
{
    [Tooltip("Árvore animada (idle) usada como recurso de madeira.")]
    public GameObject treePrefab;
    public float treeScale = 1.6f;
    [Tooltip("Variações de pedra (Aseprite); cada pedra sorteia uma.")]
    public Sprite[] rockSprites;
    public float rockScale = 1.6f;
    [Header("Colisão (tamanho local, antes da escala)")]
    public Vector2 trunkColliderSize = new Vector2(0.6f, 0.35f);
    public Vector2 trunkColliderOffset = new Vector2(0f, 0.1f);
    [Tooltip("Fração da largura/altura do sprite da pedra usada na colisão.")]
    public Vector2 rockColliderFraction = new Vector2(0.8f, 0.5f);
    public float cellSize = 6.5f;
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
        public Vector3 BaseScale;
    }

    readonly Dictionary<Vector2Int, Node> nodes = new Dictionary<Vector2Int, Node>();
    readonly HashSet<Vector2Int> depleted = new HashSet<Vector2Int>();
    readonly List<Vector2Int> removal = new List<Vector2Int>();
    Node target;
    DayNightCycle cycle;
    PlayerController controller;
    Transform player;
    Vector2Int lastCell = new Vector2Int(int.MinValue, int.MinValue);
    int day;

    public static string Label(ResourceKind kind) => kind == ResourceKind.Wood ? "Madeira" : "Pedra";
    public static Color Tint(ResourceKind kind) => kind == ResourceKind.Wood ? new Color(0.55f, 0.88f, 0.35f) : new Color(0.78f, 0.83f, 0.93f);

    void Start()
    {
        var gm = GameManager.Instance;
        cycle = DayNightCycle.Instance;
        if (!gm || !gm.Player || !cycle) { enabled = false; return; }
        player = gm.Player.transform;
        controller = gm.Player.GetComponent<PlayerController>();
        Inventory = gm.Player.GetComponent<ResourceInventory>();
        if (!Inventory) Inventory = gm.Player.gameObject.AddComponent<ResourceInventory>();
        if (!treePrefab || rockSprites == null || rockSprites.Length == 0)
            Debug.LogError("[DayResourceSystem] treePrefab/rockSprites não definidos. Regere a cena pelo menu Survivors.");
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
            node.Object.transform.localScale = node.BaseScale * (empty ? 0.55f : node.Rich ? 1.25f : 1f);
            float d = ((Vector2)(node.Object.transform.position - player.position)).sqrMagnitude;
            // O pivô da árvore fica no pé do tronco: dá um pouco mais de alcance.
            if (node.Kind == ResourceKind.Wood) d /= 1.44f;
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
            Prompt = "Explore: árvores e rochas • Segure E para coletar";
            return;
        }
        target.Renderer.color = Color.Lerp(Color.white, Tint(target.Kind), 0.3f + 0.2f * Mathf.Sin(Time.time * 5f));
        int amount = Yield(target.Kind) * (target.Rich ? 2 : 1);
        Prompt = $"Segure E / X (controle)  •  {Label(target.Kind)} +{amount}" + (target.Rich ? "  •  VEIO RICO!" : "");
        if (!GameInput.CollectHeld()) { Progress = 0; return; }
        float duration = Mathf.Max(0.1f, collectSeconds);
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

    static int Yield(ResourceKind kind) => kind == ResourceKind.Wood ? 5 : 3;

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
            // ~45% das células ficam vazias; árvores em ~25% e pedras em ~30%.
            int roll = random.Next(20);
            if (roll >= 11) continue;
            var kind = roll < 5 ? ResourceKind.Wood : ResourceKind.Stone;
            if (kind == ResourceKind.Wood && !treePrefab) continue;
            if (kind == ResourceKind.Stone && (rockSprites == null || rockSprites.Length == 0)) continue;
            var position = new Vector3((x + 0.25f + (float)random.NextDouble() * 0.5f) * size,
                (y + 0.25f + (float)random.NextDouble() * 0.5f) * size, 0);
            if (ChestSystem.Instance && ChestSystem.Instance.IsReserved(position, 3f)) continue;
            if (CampSystem.Instance && CampSystem.Instance.IsReserved(position, 1f)) continue;
            bool rich = random.Next(8) == 0;
            bool flip = random.Next(2) == 0;
            int variant = random.Next(rockSprites != null && rockSprites.Length > 0 ? rockSprites.Length : 1);
            GameObject go;
            SpriteRenderer renderer;
            Vector3 baseScale;
            if (kind == ResourceKind.Wood)
            {
                go = Instantiate(treePrefab, transform);
                go.name = "Resource_Wood";
                renderer = go.GetComponent<SpriteRenderer>();
                baseScale = Vector3.one * treeScale;
            }
            else
            {
                go = new GameObject("Resource_" + kind);
                go.transform.SetParent(transform, false);
                renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = rockSprites[variant];
                baseScale = Vector3.one * rockScale;
            }
            go.transform.position = position;
            go.transform.localScale = baseScale;
            renderer.flipX = flip;
            AddCollider(go, renderer, kind, flip);
            // Mesma camada do jogador, ordenado pela altura (eixo Y).
            renderer.sortingOrder = 10;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            nodes.Add(cell, new Node { Cell = cell, Kind = kind, Rich = rich, Object = go, Renderer = renderer, BaseScale = baseScale });
        }
    }

    // Colisor estático (sem Rigidbody2D) na base do recurso.
    void AddCollider(GameObject go, SpriteRenderer renderer, ResourceKind kind, bool flip)
    {
        var collider = go.GetComponent<CapsuleCollider2D>();
        if (!collider) collider = go.AddComponent<CapsuleCollider2D>();
        collider.direction = CapsuleDirection2D.Horizontal;
        if (kind == ResourceKind.Wood)
        {
            collider.size = trunkColliderSize;
            collider.offset = trunkColliderOffset;
            return;
        }
        var bounds = renderer.sprite.bounds; // local, relativo ao pivô
        var size = new Vector2(bounds.size.x * rockColliderFraction.x, bounds.size.y * rockColliderFraction.y);
        collider.size = size;
        collider.offset = new Vector2(flip ? -bounds.center.x : bounds.center.x, bounds.min.y + size.y * 0.5f + 0.05f);
    }

    void OnDestroy()
    {
        if (cycle) cycle.OnPeriodChanged -= PeriodChanged;
    }
}
