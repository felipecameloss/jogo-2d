using System.Collections.Generic;
using UnityEngine;

// Três baús perto do ponto de nascimento (só no começo da run, não voltam).
// Cada baú precisa de uma chave. As chaves ficam espalhadas pelo mapa e vão
// para o inventário quando o jogador passa por cima delas.
public class ChestSystem : MonoBehaviour
{
    public static ChestSystem Instance { get; private set; }

    [Header("Baús")]
    public float firstChestDistance = 10f;
    public float chestSpacing = 3.5f;
    public float interactionRange = 2.2f;

    [Header("Chaves")]
    [Tooltip("A primeira chave nasce garantida a essa distância do início.")]
    public float firstKeyDistance = 11f;
    public float keyCellSize = 14f;
    [Range(0f, 1f)] public float keyChancePerCell = 0.2f;
    public float keyPickupRange = 0.9f;

    [Header("Colisão do baú (sem colisão nas chaves)")]
    public Vector2 chestColliderSize = new Vector2(1.3f, 0.6f);
    public Vector2 chestColliderOffset = new Vector2(0f, 0.3f);

    public string Prompt { get; private set; }
    public string Heading => selected == null ? "" : selected.Name;
    public IReadOnlyList<ChestOffer> Offers => selected == null ? null : selected.Offers;
    public bool IsOpen => selected != null && GameManager.Instance && GameManager.Instance.State == GameState.Menu;
    // A janela espera a animação terminar; antes disso cobriria o baú abrindo.
    public bool ShowOffers => IsOpen && selected.Lid == Lid.Open;
    public ResourceInventory Inventory { get; private set; }
    public int ClosedChests { get { int n = 0; foreach (var c in chests) if (!c.Opened) n++; return n; } }

    // Quadros de chest.png: 0 fechado, 1 a 11 abrindo, 12 a 19 aberto em loop.
    const int FirstOpenFrame = 12;
    const float OpeningFrameTime = 0.07f;
    const float OpenFrameTime = 0.13f;

    enum Lid { Closed, Opening, Open, Closing }

    class Chest
    {
        public GameObject Object;
        public SpriteRenderer Renderer;
        public string Name;
        public bool Opened;
        public ChestOffer[] Offers;
        public Lid Lid;
        public int Frame;
        public float Timer;
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
    Sprite[] chestFrames;
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
        // Baús e chaves são medidos a partir do acampamento, para ficarem fora do anel.
        var camp = CampSystem.Ensure(gameObject);
        spawn = camp ? camp.Center : player.transform.position;
        runSeed = Random.Range(int.MinValue, int.MaxValue);
        var random = new System.Random(runSeed);
        keySprite = ChestArt.Key();
        ownedSprites.Add(keySprite);
        chestFrames = ChestArt.ChestFrames();

        // Baús em direções diferentes, cada vez mais longe (e melhores).
        baseAngle = (float)random.NextDouble() * 360f;
        string[] names = { "Baú do viajante", "Baú do cavaleiro", "Baú do rei" };
        var weapons = ChestWeapons(random);
        for (int tier = 0; tier < 3; tier++)
        {
            float angle = (baseAngle + tier * 120f + (float)random.NextDouble() * 30f - 15f) * Mathf.Deg2Rad;
            var position = spawn + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (firstChestDistance + tier * chestSpacing);
            // Cada baú: uma arma (diferente em cada baú, quando houver) e dois equipamentos.
            var offers = new List<ChestOffer>();
            if (weapons.Count > 0) offers.Add(ChestOffer.CreateWeapon(weapons[tier % weapons.Count], tier + 1));
            int offset = random.Next(ChestOffer.GearCount);
            for (int i = 0; offers.Count < 3; i++)
                offers.Add(ChestOffer.Create((ChestReward)((offset + i * 2) % ChestOffer.GearCount), tier + 1));
            chests.Add(SpawnChest(names[tier], position, tier, offers.ToArray()));
        }
        // Chave garantida no começo, entre o primeiro e o segundo baú.
        float keyAngle = (baseAngle + 60f) * Mathf.Deg2Rad;
        starterKeyPosition = spawn + new Vector3(Mathf.Cos(keyAngle), Mathf.Sin(keyAngle), 0) * firstKeyDistance;
    }

    // Armas do catálogo, menos a inicial, embaralhadas com a semente da run.
    List<Weapon> ChestWeapons(System.Random random)
    {
        var holder = player.GetComponent<WeaponHolder>();
        var list = new List<Weapon>();
        foreach (var weapon in UpgradeManager.Catalog())
            if (weapon && (!holder || weapon != holder.startingWeapon)) list.Add(weapon);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        return list;
    }

    void Start()
    {
        if (!player) return;
        Inventory = player.GetComponent<ResourceInventory>();
        if (!Inventory) Inventory = player.gameObject.AddComponent<ResourceInventory>();
        gameObject.AddComponent<ChestUI>().source = this;
        keys[StarterKey] = SpawnKey(starterKeyPosition);
    }

    // Baú fechado mais próximo de um ponto, para as missões.
    public bool TryGetClosest(Vector3 from, out string name, out Vector3 position)
    {
        name = "";
        position = Vector3.zero;
        float best = float.MaxValue;
        foreach (var chest in chests)
        {
            if (chest.Opened) continue;
            float sqr = ((Vector2)(chest.Object.transform.position - from)).sqrMagnitude;
            if (sqr >= best) continue;
            best = sqr;
            name = chest.Name;
            position = chest.Object.transform.position;
        }
        return best < float.MaxValue;
    }

    public void GetClosedChests(List<Vector3> positions)
    {
        foreach (var chest in chests)
            if (!chest.Opened) positions.Add(chest.Object.transform.position);
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
        renderer.sprite = chestFrames[0];
        renderer.color = ChestArt.Tint(tier);
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
        foreach (var chest in chests) Animate(chest);
        RefreshKeys();
        UpdateKeys();
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
        bool campAction = CampSystem.Instance && CampSystem.Instance.HasTarget;
        if (!campAction && GameInput.InteractPressed()) TryOpenNearest();
    }

    // Usa tempo não escalado: o jogo já está pausado enquanto o baú abre.
    void Animate(Chest chest)
    {
        if (chest.Lid == Lid.Closed) return;
        chest.Timer += Time.unscaledDeltaTime;
        float frameTime = chest.Lid == Lid.Open ? OpenFrameTime : OpeningFrameTime;
        if (chest.Timer < frameTime) return;
        chest.Timer -= frameTime;
        switch (chest.Lid)
        {
            case Lid.Opening:
                if (++chest.Frame >= FirstOpenFrame) chest.Lid = Lid.Open;
                break;
            case Lid.Open:
                if (++chest.Frame >= chestFrames.Length) chest.Frame = FirstOpenFrame;
                break;
            case Lid.Closing:
                chest.Frame = Mathf.Max(0, chest.Frame - 1);
                if (chest.Frame == 0) chest.Lid = Lid.Closed;
                break;
        }
        chest.Renderer.sprite = chestFrames[chest.Frame];
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

    public bool TryOpenNearest()
    {
        if (nearest == null || nearest.Opened || player.IsDead || Inventory.Keys <= 0) return false;
        if (!GameManager.Instance.EnterMenu(Close)) return false;
        selected = nearest;
        selected.Lid = Lid.Opening;
        selected.Timer = 0f;
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
        AudioManager.Play(Sfx.LevelUp, 0.6f);
        FxManager.Burst(selected.Object.transform.position, new Color(1f, 0.8f, 0.35f), 14, 4f);
        FxManager.Popup(player.transform.position + Vector3.up, offer.Title, new Color(1f, 0.85f, 0.35f));
        Close();
        return true;
    }

    public void Close()
    {
        // Desistiu da recompensa: o baú volta a fechar, sem passar pelo brilho do loop.
        if (selected != null && !selected.Opened)
        {
            selected.Lid = Lid.Closing;
            selected.Frame = Mathf.Min(selected.Frame, FirstOpenFrame - 1);
            selected.Timer = 0f;
        }
        selected = null;
        if (GameManager.Instance) GameManager.Instance.ExitMenu();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        foreach (var sprite in ownedSprites) if (sprite) { Destroy(sprite.texture); Destroy(sprite); }
    }
}
