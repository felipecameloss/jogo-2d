using System.Collections.Generic;
using UnityEngine;

// Três baús perto do ponto de nascimento (só no começo da run, não voltam).
// Cada baú precisa de uma chave. As chaves ficam espalhadas pelo mapa e vão
// para o inventário quando o jogador passa por cima delas.
public class ChestSystem : MonoBehaviour
{
    public static ChestSystem Instance { get; private set; }

    [Header("Baús")]
    public float firstChestDistance = 5f;
    public float chestSpacing = 3.5f;
    public float interactionRange = 2.2f;

    [Header("Chaves")]
    [Tooltip("A primeira chave nasce garantida a essa distância do início.")]
    public float firstKeyDistance = 7f;
    public float keyCellSize = 14f;
    [Range(0f, 1f)] public float keyChancePerCell = 0.2f;
    public float keyPickupRange = 0.9f;

    [Header("Colisão do baú (sem colisão nas chaves)")]
    public Vector2 chestColliderSize = new Vector2(1.3f, 0.6f);
    public Vector2 chestColliderOffset = new Vector2(0f, 0.3f);

    public string Prompt { get; private set; }
    public string Direction { get; private set; }
    public string Heading => selected == null ? "" : selected.Name;
    public IReadOnlyList<ChestOffer> Offers => selected == null ? null : selected.Offers;
    public bool IsOpen => selected != null && GameManager.Instance && GameManager.Instance.State == GameState.Chest;
    public ResourceInventory Inventory { get; private set; }
    public int ClosedChests { get { int n = 0; foreach (var c in chests) if (!c.Opened) n++; return n; } }

    class Chest
    {
        public GameObject Object;
        public SpriteRenderer Renderer;
        public string Name;
        public bool Opened;
        public ChestOffer[] Offers;
    }

    readonly List<Chest> chests = new List<Chest>();
    readonly Dictionary<Vector2Int, GameObject> keys = new Dictionary<Vector2Int, GameObject>();
    readonly HashSet<Vector2Int> pickedKeys = new HashSet<Vector2Int>();
    readonly List<Vector2Int> removal = new List<Vector2Int>();
    readonly List<Sprite> ownedSprites = new List<Sprite>();
    static readonly Vector2Int StarterKey = new Vector2Int(int.MinValue, int.MinValue);
    Chest selected, nearest;
    PlayerStats player;
    PlayerController controller;
    Sprite keySprite;
    Vector3 spawn, starterKeyPosition;
    Vector2Int lastCell = new Vector2Int(int.MaxValue, int.MaxValue);
    int runSeed;
    float baseAngle;

    void Awake()
    {
        Instance = this;
        player = FindAnyObjectByType<PlayerStats>();
        if (!player) { enabled = false; return; }
        controller = player.GetComponent<PlayerController>();
        spawn = player.transform.position;
        runSeed = Random.Range(int.MinValue, int.MaxValue);
        var random = new System.Random(runSeed);
        keySprite = ChestArt.Key();
        ownedSprites.Add(keySprite);

        // Baús em direções diferentes, cada vez mais longe (e melhores).
        baseAngle = (float)random.NextDouble() * 360f;
        string[] names = { "Baú do viajante", "Baú do guardião", "Baú astral" };
        for (int tier = 0; tier < 3; tier++)
        {
            float angle = (baseAngle + tier * 120f + (float)random.NextDouble() * 30f - 15f) * Mathf.Deg2Rad;
            var position = spawn + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (firstChestDistance + tier * chestSpacing);
            var offers = new ChestOffer[3];
            int offset = random.Next(7);
            for (int i = 0; i < offers.Length; i++)
                offers[i] = ChestOffer.Create((ChestReward)((offset + i * 2) % 7), tier + 1);
            chests.Add(SpawnChest(names[tier], position, tier, offers));
        }
        // Chave garantida no começo, entre o primeiro e o segundo baú.
        float keyAngle = (baseAngle + 60f) * Mathf.Deg2Rad;
        starterKeyPosition = spawn + new Vector3(Mathf.Cos(keyAngle), Mathf.Sin(keyAngle), 0) * firstKeyDistance;
    }

    void Start()
    {
        if (!player) return;
        Inventory = player.GetComponent<ResourceInventory>();
        if (!Inventory) Inventory = player.gameObject.AddComponent<ResourceInventory>();
        gameObject.AddComponent<ChestUI>().source = this;
        keys[StarterKey] = SpawnKey(starterKeyPosition);
    }

    public bool IsNearChest(Vector3 position, float radius)
    {
        foreach (var chest in chests)
            if (((Vector2)(chest.Object.transform.position - position)).sqrMagnitude < radius * radius) return true;
        return false;
    }

    // Lugares onde árvores/pedras não devem nascer: início, baús e chaves (inclusive as que
    // ainda não apareceram), para nenhuma chave ficar presa dentro de uma colisão.
    public bool IsReserved(Vector3 position, float radius)
    {
        if (IsNearChest(position, radius)) return true;
        // O jogador nasce num lugar livre.
        if (((Vector2)(spawn - position)).sqrMagnitude < radius * radius) return true;
        if (((Vector2)(starterKeyPosition - position)).sqrMagnitude < radius * radius) return true;
        float size = Mathf.Max(4f, keyCellSize);
        int cx = Mathf.FloorToInt(position.x / size), cy = Mathf.FloorToInt(position.y / size);
        for (int y = cy - 1; y <= cy + 1; y++)
        for (int x = cx - 1; x <= cx + 1; x++)
            if (KeyAt(new Vector2Int(x, y), out var key) && ((Vector2)(key - position)).sqrMagnitude < radius * radius) return true;
        return false;
    }

    // Posição determinística da chave de uma célula (se a célula tiver chave).
    bool KeyAt(Vector2Int cell, out Vector3 position)
    {
        float size = Mathf.Max(4f, keyCellSize);
        var random = new System.Random(unchecked(runSeed ^ cell.x * 73856093 ^ cell.y * 19349663));
        position = Vector3.zero;
        if (random.NextDouble() >= keyChancePerCell) return false;
        position = new Vector3((cell.x + 0.15f + (float)random.NextDouble() * 0.7f) * size, (cell.y + 0.15f + (float)random.NextDouble() * 0.7f) * size, 0);
        // Longe do início (lá já tem a chave garantida) e dos baús.
        return ((Vector2)(position - spawn)).magnitude >= firstKeyDistance + 3f && !IsNearChest(position, 3f);
    }

    Chest SpawnChest(string name, Vector3 position, int tier, ChestOffer[] offers)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.position = position;
        var renderer = go.AddComponent<SpriteRenderer>();
        var sprite = ChestArt.Chest(tier);
        ownedSprites.Add(sprite);
        renderer.sprite = sprite;
        renderer.sortingOrder = 10;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        // Sem Rigidbody2D: colisor estático, bloqueia jogador e inimigos.
        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = chestColliderSize;
        collider.offset = chestColliderOffset;
        return new Chest { Object = go, Renderer = renderer, Name = name, Offers = offers };
    }

    GameObject SpawnKey(Vector3 position)
    {
        var go = new GameObject("Key");
        go.transform.SetParent(transform, false);
        go.transform.position = position;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = keySprite;
        renderer.sortingOrder = 10;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        return go;
    }

    void Update()
    {
        if (!player || Inventory == null) return;
        RefreshKeys();
        UpdateKeys();
        UpdateDirection();
        if (IsOpen) return;

        nearest = null;
        float range = interactionRange * interactionRange;
        foreach (var chest in chests)
        {
            if (chest.Opened) continue;
            float sqr = ((Vector2)(chest.Object.transform.position - player.transform.position)).sqrMagnitude;
            if (sqr < range) { range = sqr; nearest = chest; }
        }
        Prompt = nearest == null ? "" : Inventory.Keys > 0
            ? $"[F / Y] Abrir {nearest.Name} • usa 1 chave"
            : $"{nearest.Name} trancado • encontre uma chave pelo mapa";
        if (GameManager.Instance.State != GameState.Playing || (controller && controller.IsDashing)) return;
        if (GameInput.InteractPressed()) TryOpenNearest();
    }

    // Chaves só aparecem enquanto ainda houver baú fechado sem chave guardada para ele.
    bool KeysNeeded => Inventory.Keys < ClosedChests;

    void RefreshKeys()
    {
        float size = Mathf.Max(4f, keyCellSize);
        var center = new Vector2Int(Mathf.FloorToInt(player.transform.position.x / size), Mathf.FloorToInt(player.transform.position.y / size));
        if (center == lastCell) return;
        lastCell = center;
        removal.Clear();
        foreach (var pair in keys)
            if (pair.Key != StarterKey && (Mathf.Abs(pair.Key.x - center.x) > 2 || Mathf.Abs(pair.Key.y - center.y) > 2)) removal.Add(pair.Key);
        foreach (var cell in removal) { Destroy(keys[cell]); keys.Remove(cell); }
        for (int y = center.y - 2; y <= center.y + 2; y++)
        for (int x = center.x - 2; x <= center.x + 2; x++)
        {
            var cell = new Vector2Int(x, y);
            if (keys.ContainsKey(cell) || pickedKeys.Contains(cell)) continue;
            if (KeyAt(cell, out var position)) keys[cell] = SpawnKey(position);
        }
    }

    void UpdateKeys()
    {
        bool needed = KeysNeeded;
        Vector2Int? picked = null;
        foreach (var pair in keys)
        {
            var go = pair.Value;
            go.SetActive(needed);
            if (!needed) continue;
            // Flutua e brilha para chamar atenção.
            float t = Time.time * 3f + pair.Key.x * 1.7f + pair.Key.y;
            go.transform.localScale = Vector3.one * (1.6f + Mathf.Sin(t * 2f) * 0.12f);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.color = Color.Lerp(Color.white, new Color(1f, 0.95f, 0.6f), 0.5f + 0.5f * Mathf.Sin(t * 2f));
            if (picked == null && ((Vector2)(go.transform.position - player.transform.position)).sqrMagnitude < keyPickupRange * keyPickupRange)
                picked = pair.Key;
        }
        if (picked == null || player.IsDead) return;
        var key = keys[picked.Value];
        Inventory.AddKey();
        FxManager.Burst(key.transform.position, new Color(1f, 0.85f, 0.3f), 10, 3f);
        FxManager.Popup(key.transform.position + Vector3.up, "+1 Chave", new Color(1f, 0.85f, 0.3f));
        AudioManager.Play(Sfx.Pickup, 0.8f);
        Destroy(key);
        keys.Remove(picked.Value);
        pickedKeys.Add(picked.Value);
    }

    void UpdateDirection()
    {
        Chest closest = null;
        float best = float.MaxValue;
        foreach (var chest in chests)
        {
            if (chest.Opened) continue;
            float sqr = ((Vector2)(chest.Object.transform.position - player.transform.position)).sqrMagnitude;
            if (sqr < best) { best = sqr; closest = chest; }
        }
        if (closest == null) { Direction = ""; return; }
        var d = closest.Object.transform.position - player.transform.position;
        string heading = Mathf.Abs(d.x) > Mathf.Abs(d.y) ? (d.x > 0 ? "leste >" : "< oeste") : (d.y > 0 ? "norte ^" : "sul v");
        Direction = $"Baú mais próximo: {Mathf.Sqrt(best):0} m • {heading}";
    }

    public bool TryOpenNearest()
    {
        if (nearest == null || nearest.Opened || player.IsDead || Inventory.Keys <= 0) return false;
        if (!GameManager.Instance.EnterChest()) return false;
        selected = nearest;
        AudioManager.Play(Sfx.Select, 0.5f);
        return true;
    }

    // A chave só é gasta quando o jogador escolhe a recompensa.
    public bool Choose(int index)
    {
        if (!IsOpen || index < 0 || index >= selected.Offers.Length || !Inventory.UseKey()) return false;
        var offer = selected.Offers[index];
        offer.Apply(player);
        selected.Opened = true;
        selected.Renderer.color = new Color(0.45f, 0.45f, 0.45f, 0.7f);
        selected.Object.transform.localScale = new Vector3(1, 0.6f, 1);
        AudioManager.Play(Sfx.LevelUp, 0.6f);
        FxManager.Burst(selected.Object.transform.position, new Color(1f, 0.8f, 0.35f), 14, 4f);
        FxManager.Popup(player.transform.position + Vector3.up, offer.Title, new Color(1f, 0.85f, 0.35f));
        Close();
        return true;
    }

    public void Close()
    {
        selected = null;
        if (GameManager.Instance) GameManager.Instance.ExitChest();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        foreach (var sprite in ownedSprites) if (sprite) { Destroy(sprite.texture); Destroy(sprite); }
    }
}
