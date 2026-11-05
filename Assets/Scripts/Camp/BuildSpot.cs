using UnityEngine;

public enum SpotKind { Heart, Wall, Gate, WeaponStation, ArmorStation }

// Um lugar fixo do acampamento. Sabe qual é a próxima ação disponível e quanto ela custa.
public class BuildSpot : MonoBehaviour
{
    public const int FenceWood = 6, WallStone = 6, GateWood = 8, StationWood = 30, StationStone = 20;
    public const float FenceHealth = 60f, WallHealth = 150f, GateHealth = 100f, StationHealth = 120f;

    public SpotKind Kind { get; private set; }
    public Structure Current { get; private set; }
    public bool Locked { get; set; }
    public string LockedText { get; set; } = "";

    SpriteRenderer mark;
    Vector2 size;

    public bool HasAction => !Locked && NextCost(out _, out _, out _);

    public void Setup(SpotKind kind, Vector2 spotSize)
    {
        Kind = kind;
        size = spotSize;
        mark = gameObject.AddComponent<SpriteRenderer>();
        mark.sprite = CampArt.Get("spot");
        mark.sortingOrder = 5;
        transform.localScale = Vector3.one;
    }

    // A fogueira já nasce pronta: o ponto só adota a estrutura.
    public void Adopt(Structure structure)
    {
        Current = structure;
        structure.OnDestroyed += StructureDestroyed;
        mark.enabled = false;
    }

    bool NextCost(out int wood, out int stone, out string label)
    {
        wood = stone = 0;
        label = "";
        if (Current)
        {
            if (Current.Kind != StructureKind.Fence) return false;
            stone = WallStone;
            label = "Reforçar com muro";
            return true;
        }
        switch (Kind)
        {
            case SpotKind.Wall: wood = FenceWood; label = "Construir cerca"; return true;
            case SpotKind.Gate: wood = GateWood; label = "Construir portão"; return true;
            case SpotKind.WeaponStation: wood = StationWood; stone = StationStone; label = "Construir ferraria"; return true;
            case SpotKind.ArmorStation: wood = StationWood; stone = StationStone; label = "Construir armaria"; return true;
            default: return false;
        }
    }

    public string ActionText(ResourceInventory bag)
    {
        if (Locked) return LockedText;
        if (!NextCost(out int wood, out int stone, out string label)) return "";
        string missing = bag && bag.CanAfford(wood, stone) ? "" : "  •  materiais insuficientes";
        return $"[F / Y] {label}  •  {Cost(wood, stone)}{missing}";
    }

    public static string Cost(int wood, int stone)
    {
        if (wood > 0 && stone > 0) return $"{wood} madeira e {stone} pedra";
        return wood > 0 ? $"{wood} madeira" : $"{stone} pedra";
    }

    public bool TryBuild(ResourceInventory bag)
    {
        if (Locked || !bag || !NextCost(out int wood, out int stone, out _)) return false;
        if (!bag.CanAfford(wood, stone)) { Warn("Materiais insuficientes"); return false; }
        if (Kind != SpotKind.Gate && PlayerOnSpot()) { Warn("Saia do local para construir"); return false; }
        if (!bag.TrySpend(wood, stone)) return false;

        if (Current) Replace(StructureKind.Wall, "wall", WallHealth);
        else Create();
        FxManager.Burst(transform.position, new Color(0.9f, 0.75f, 0.45f), 10, 3f);
        AudioManager.Play(Sfx.Select, 0.8f);
        return true;
    }

    void Create()
    {
        var go = new GameObject("Structure");
        go.transform.SetParent(transform, false);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 10;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        go.AddComponent<BoxCollider2D>().size = size; // sem Rigidbody2D: colisor estático
        Current = go.AddComponent<Structure>();
        Current.OnDestroyed += StructureDestroyed;
        mark.enabled = false;
        switch (Kind)
        {
            case SpotKind.Wall: Replace(StructureKind.Fence, "fence", FenceHealth); break;
            case SpotKind.Gate: Replace(StructureKind.Gate, "gate", GateHealth); break;
            case SpotKind.WeaponStation: Replace(StructureKind.Station, "station_weapon", StationHealth); break;
            case SpotKind.ArmorStation: Replace(StructureKind.Station, "station_armor", StationHealth); break;
        }
    }

    void Replace(StructureKind kind, string sprite, float health)
    {
        Current.GetComponent<SpriteRenderer>().sprite = CampArt.Get(sprite);
        Current.Init(kind, health);
    }

    void StructureDestroyed(Structure structure)
    {
        if (Kind == SpotKind.Heart) return; // A derrota é tratada pelo CampSystem.
        structure.OnDestroyed -= StructureDestroyed;
        Destroy(structure.gameObject);
        Current = null;
        mark.enabled = true;
    }

    bool PlayerOnSpot()
    {
        foreach (var hit in Physics2D.OverlapBoxAll(transform.position, size, transform.eulerAngles.z))
            if (hit.GetComponent<PlayerStats>()) return true;
        return false;
    }

    void Warn(string text) => FxManager.Popup(transform.position + Vector3.up, text, new Color(1f, 0.55f, 0.4f));
}
