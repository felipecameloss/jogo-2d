using System.Collections.Generic;
using UnityEngine;

// O acampamento: fogueira no centro, anel quadrado de muralhas com portões e pontos de estação.
// Tudo é criado por código, sem depender da cena.
public class CampSystem : MonoBehaviour
{
    public const float ReservedRadius = 8f; // cobre os cantos do quadrado, a ~7,5 do centro
    public const float HeartHealth = 300f;
    // Lado do quadrado, em células de 32 px: canto, 3 peças, portão, 3 peças, canto.
    public const int RingCells = 8;
    // Faixa de colisão das peças: a parede é fina e fica em volta do pé do poste.
    const float WallHalfThickness = 0.3f, WallLift = 0.15f;
    // Pessoas perdidas: longe o bastante para exigir uma expedição durante o dia.
    public const float PersonMinDistance = 25f, PersonMaxDistance = 40f;
    // O ferreiro usa o mesmo desenho do jogador: a cor diferencia.
    public static readonly Color SmithTint = new Color(1f, 0.72f, 0.55f); // ferreiro: quadros do jogador tingidos
    static readonly Color ArmorerTint = Color.white;
    const float CenterOffset = 1.5f; // O jogador nasce ao lado da fogueira, não em cima dela.
    const float HeartScale = 2f;
    const float HeartFrameTime = 0.1f;
    static readonly Vector2 HeartPivot = new Vector2(0.5f, 0.34f); // Centro do círculo de pedras.
    const float HeartPixelsPerUnit = 24f;

    public static CampSystem Instance { get; private set; }

    public float interactionRange = 2f;

    public Vector3 Center { get; private set; }
    public Structure Heart { get; private set; }
    public BuildSpot NearestSpot { get; private set; }
    public string Prompt { get; private set; } = "";
    public ResourceInventory Inventory { get; private set; }
    public LostPerson NearestPerson { get; private set; }
    public StationSystem Stations { get; private set; }
    // Quando há ação disponível, o F é do acampamento e não abre baús próximos.
    public bool HasTarget => NearestPerson || (NearestSpot && (NearestSpot.HasAction || NearestSpot.IsBuiltStation));
    public IReadOnlyList<BuildSpot> Spots => spots;
    public IReadOnlyList<LostPerson> Persons => persons;

    public int Residents
    {
        get
        {
            int count = 0;
            foreach (var person in persons) if (person && person.State == PersonState.Resident) count++;
            return count;
        }
    }

    readonly List<BuildSpot> spots = new List<BuildSpot>();
    readonly List<LostPerson> persons = new List<LostPerson>();
    bool personsSpawned;
    PlayerStats player;
    PlayerController controller;
    public Collider2D PlayerCollider { get; private set; }

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
        Stations = gameObject.AddComponent<StationSystem>();
        Stations.camp = this;
    }

    public LostPerson Person(PersonRole role)
    {
        foreach (var person in persons) if (person && person.Role == role) return person;
        return null;
    }

    BuildSpot StationSpot(PersonRole role)
    {
        var kind = role == PersonRole.WeaponSmith ? SpotKind.WeaponStation : SpotKind.ArmorStation;
        foreach (var spot in spots) if (spot.Kind == kind) return spot;
        return null;
    }

    // Criadas no primeiro Update, quando os baús já existem e podem ser evitados.
    void SpawnPersons()
    {
        personsSpawned = true;
        var animator = player.GetComponent<CharacterAnimator>();
        var manFrames = animator ? animator.frames : null;
        var womanFrames = LoadFrames("Characters/character_women");
        float angle = Random.Range(0f, 360f);
        persons.Add(NewPerson(PersonRole.WeaponSmith, manFrames, SmithTint, angle));
        persons.Add(NewPerson(PersonRole.ArmorSmith, womanFrames ?? manFrames, ArmorerTint, angle + 180f + Random.Range(-40f, 40f)));
    }

    LostPerson NewPerson(PersonRole role, Sprite[] frames, Color tint, float angle)
    {
        var go = new GameObject(role == PersonRole.WeaponSmith ? "Ferreiro" : "Ferreira");
        go.transform.SetParent(transform, false);
        go.transform.position = FindLostPosition(angle);
        var person = go.AddComponent<LostPerson>();
        person.Setup(role, frames, tint);
        var home = StationSpot(role);
        person.Home = (home ? home.transform.position : Center) + Vector3.down * 0.5f;
        return person;
    }

    // Quadros da fogueira. CampArt guarda o cache só pelo nome: quem mais usar a arte (como a
    // introdução) passa por aqui para não fatiar com outro pivô.
    public static Sprite[] FireFrames() => CampArt.Frames("heart", HeartPivot, HeartPixelsPerUnit);

    // Spritesheet fatiada no import (Multiple): ordena os quadros pelo número do nome.
    public static Sprite[] LoadFrames(string path)
    {
        var frames = Resources.LoadAll<Sprite>(path);
        if (frames == null || frames.Length == 0) return null;
        System.Array.Sort(frames, (a, b) => Index(a.name).CompareTo(Index(b.name)));
        return frames;
    }

    static int Index(string name)
    {
        int underscore = name.LastIndexOf('_');
        return underscore >= 0 && int.TryParse(name.Substring(underscore + 1), out int value) ? value : 0;
    }

    // Ponto livre a 25-40 unidades da fogueira, perto do ângulo pedido.
    Vector3 FindLostPosition(float angle)
    {
        Vector3 fallback = Center;
        for (int attempt = 0; attempt < 40; attempt++)
        {
            float radians = (angle + Random.Range(-30f, 30f)) * Mathf.Deg2Rad;
            var position = Center + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * Random.Range(PersonMinDistance, PersonMaxDistance);
            if (attempt == 0) fallback = position;
            if (Physics2D.OverlapCircle(position, 0.8f)) continue;
            if (ChestSystem.Instance && ChestSystem.Instance.IsReserved(position, 2f)) continue;
            return position;
        }
        return fallback;
    }

    // Residente derrotado volta a ficar perdido em outro ponto do mapa.
    public void DefeatPerson(LostPerson person)
    {
        if (person) person.Defeat(FindLostPosition(Random.Range(0f, 360f)));
    }

    // Recursos e decoração não nascem em cima de quem está perdido.
    public bool IsNearPerson(Vector3 position, float radius)
    {
        foreach (var person in persons)
            if (person && person.State == PersonState.Lost && ((Vector2)(person.transform.position - position)).sqrMagnitude < radius * radius) return true;
        return false;
    }

    BuildSpot NewSpot(SpotKind kind, Vector3 position, SpotPiece[] pieces, params Rect[] areas)
    {
        var go = new GameObject("BuildSpot_" + kind);
        go.transform.SetParent(transform, false);
        go.transform.position = position;
        var spot = go.AddComponent<BuildSpot>();
        spot.Setup(kind, pieces, areas);
        spots.Add(spot);
        return spot;
    }

    static SpotPiece[] One(int index) => new[] { new SpotPiece(Vector2.zero, index) };

    void BuildHeart()
    {
        var spot = NewSpot(SpotKind.Heart, Center, One(0), new Rect(-0.65f, -0.35f, 1.3f, 0.7f));
        var go = new GameObject("Fogueira");
        go.transform.SetParent(spot.transform, false);
        var renderer = go.AddComponent<SpriteRenderer>();
        var frames = FireFrames();
        renderer.sprite = frames[0];
        renderer.sortingOrder = 10;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        go.transform.localScale = Vector3.one * HeartScale;
        if (frames.Length > 1)
        {
            var loop = go.AddComponent<SpriteLoop>();
            loop.frames = frames;
            loop.frameTime = HeartFrameTime;
        }
        go.AddComponent<BoxCollider2D>().size = new Vector2(1.7f, 0.9f) / HeartScale;
        Heart = go.AddComponent<Structure>();
        Heart.Init(StructureKind.Heart, HeartHealth);
        Heart.OnDestroyed += HeartDestroyed;
        spot.Adopt(Heart);
    }

    // Nó (i, j) da grade do anel, de 0 a RingCells, com a fogueira no meio.
    Vector3 Node(int i, int j) => Center + new Vector3(i - RingCells / 2, j - RingCells / 2, 0f) * CampArt.Cell;

    // 12 pontos de muralha (4 cantos e 8 trechos de 3 peças) e 4 portões, no meio de cada lado.
    void BuildRing()
    {
        int n = RingCells, half = RingCells / 2;
        Corner(0, n, 11);  // ┌
        Corner(n, n, 10);  // ┐
        Corner(0, 0, 9);   // └
        Corner(n, 0, 8);   // ┘
        foreach (int j in new[] { 0, n })
        {
            HorizontalRun(2, j);
            HorizontalRun(half + 2, j);
            NewSpot(SpotKind.Gate, Node(half, j), One(0), HorizontalArea(0.5f));
        }
        foreach (int i in new[] { 0, n })
        {
            VerticalRun(i, 2);
            VerticalRun(i, half + 2);
            NewSpot(SpotKind.Gate, Node(i, half), One(1), VerticalArea(-0.5f, 0.5f));
        }
    }

    // Três peças com centro no nó (i, j). Cada lado tem o seu quadro, que casa com o braço dos cantos:
    // topo 0, base 1, esquerda 3, direita 2.
    void HorizontalRun(int i, int j)
    {
        int index = j == RingCells ? 0 : 1;
        var pieces = new SpotPiece[3];
        for (int k = -1; k <= 1; k++) pieces[k + 1] = new SpotPiece(new Vector2(k * CampArt.Cell, 0f), index);
        NewSpot(SpotKind.Wall, Node(i, j), pieces, HorizontalArea(1.5f));
    }

    void VerticalRun(int i, int j)
    {
        int index = i == 0 ? 3 : 2;
        var pieces = new SpotPiece[3];
        for (int k = -1; k <= 1; k++) pieces[k + 1] = new SpotPiece(new Vector2(0f, k * CampArt.Cell), index);
        NewSpot(SpotKind.Wall, Node(i, j), pieces, VerticalArea(-1.5f, 1.5f));
    }

    // Canto: um braço horizontal e um vertical, ambos apontando para dentro do quadrado.
    void Corner(int i, int j, int index)
    {
        float arm = 0.5f * CampArt.Cell;
        float x = i == 0 ? arm : -arm, y = j == 0 ? 0.5f : -0.5f;
        var horizontal = Rect.MinMaxRect(Mathf.Min(-WallHalfThickness, x), WallLift - WallHalfThickness,
            Mathf.Max(WallHalfThickness, x), WallLift + WallHalfThickness);
        NewSpot(SpotKind.Wall, Node(i, j), One(index), horizontal, VerticalArea(Mathf.Min(0f, y), Mathf.Max(0f, y)));
    }

    // Faixas de colisão; os limites ao longo da parede são medidos em células a partir do nó.
    static Rect HorizontalArea(float cells) => new Rect(-cells * CampArt.Cell, WallLift - WallHalfThickness,
        2f * cells * CampArt.Cell, 2f * WallHalfThickness);

    static Rect VerticalArea(float fromCells, float toCells) => Rect.MinMaxRect(-WallHalfThickness,
        fromCells * CampArt.Cell + WallLift, WallHalfThickness, toCells * CampArt.Cell + WallLift);

    void BuildStations()
    {
        var area = new Rect(-1.1f, 0f, 2.2f, 0.9f);
        var weapon = NewSpot(SpotKind.WeaponStation, Center + new Vector3(-2.7f, 1.3f, 0f), One(0), area);
        var armor = NewSpot(SpotKind.ArmorStation, Center + new Vector3(2.7f, 1.3f, 0f), One(0), area);
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

        if (!personsSpawned) SpawnPersons();
        bool playing = GameManager.Instance && GameManager.Instance.State == GameState.Playing;
        NearestSpot = null;
        NearestPerson = null;
        Prompt = "";
        if (!playing || player.IsDead) return;
        bool pressed = !(controller && controller.IsDashing) && GameInput.InteractPressed();

        float range = interactionRange * interactionRange;
        foreach (var person in persons)
        {
            if (!person || person.State != PersonState.Lost) continue;
            float sqr = ((Vector2)(person.transform.position - player.transform.position)).sqrMagnitude;
            if (sqr < range) { range = sqr; NearestPerson = person; }
        }
        if (NearestPerson)
        {
            Prompt = $"[F / Y] Resgatar {NearestPerson.Name}";
            if (pressed) NearestPerson.Rescue();
            return;
        }

        float best = interactionRange * interactionRange;
        foreach (var spot in spots)
        {
            if (!spot.Locked && !spot.HasAction && !spot.IsBuiltStation) continue;
            float distance = spot.SqrDistance(player.transform.position);
            if (distance < best) { best = distance; NearestSpot = spot; }
        }
        if (!NearestSpot) return;
        // Estação inteira: F abre a janela de compras. Danificada, F repara primeiro.
        if (!NearestSpot.HasAction && NearestSpot.IsBuiltStation && Stations)
        {
            Prompt = Stations.PromptFor(NearestSpot);
            if (pressed) Stations.TryOpen(NearestSpot);
            return;
        }
        Prompt = NearestSpot.ActionText(Inventory);
        if (NearestSpot.HasAction && pressed) NearestSpot.TryBuild(Inventory);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
