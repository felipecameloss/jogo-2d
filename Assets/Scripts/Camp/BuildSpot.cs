using System.Collections.Generic;
using UnityEngine;

public enum SpotKind { Heart, Wall, Gate, WeaponStation, ArmorStation }

// Uma peça desenhada pelo ponto: posição local e quadro na folha.
// Em portões, o índice diz a orientação: 0 horizontal, 1 vertical.
public struct SpotPiece
{
    public Vector2 offset;
    public int index;
    public SpotPiece(Vector2 offset, int index) { this.offset = offset; this.index = index; }
}

// Um lugar fixo do acampamento. Sabe qual é a próxima ação disponível e quanto ela custa.
public class BuildSpot : MonoBehaviour
{
    public const int FenceWood = 6, WallStone = 6, GateWood = 8, GateStone = 8, StationWood = 30, StationStone = 20;
    public const float HealthPerMaterial = 10f;
    public const float FenceHealth = 60f, WallHealth = 150f, GateHealth = 100f, StoneGateHealth = 200f, StationHealth = 120f;

    // Quadros de 48x64: o pivô é o pé do poste, no nó da grade do anel.
    static readonly Vector2 PiecePivot = new Vector2(0.5f, 20f / 64f);
    // Quadros de 64x68: o pivô é a base da barraca.
    static readonly Vector2 StationPivot = new Vector2(0.5f, 9f / 68f);
    const float StationFrameTime = 0.11f;

    public SpotKind Kind { get; private set; }
    public Structure Current { get; private set; }
    public bool Locked { get; set; }
    public string LockedText { get; set; } = "";

    SpotPiece[] pieces;
    Rect[] areas; // colisores, em coordenadas locais
    readonly List<SpriteRenderer> marks = new List<SpriteRenderer>();
    readonly List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    GateVisual gate;
    SpriteLoop stationLoop;
    bool stationResident = true;

    public bool HasAction => !Locked && NextCost(out _, out _, out _);
    public bool IsStation => Kind == SpotKind.WeaponStation || Kind == SpotKind.ArmorStation;
    public bool IsBuiltStation => Current && Current.Kind == StructureKind.Station;
    public string StationName => Kind == SpotKind.WeaponStation ? "Ferraria" : "Armaria";

    public void Setup(SpotKind kind, SpotPiece[] spotPieces, Rect[] spotAreas)
    {
        Kind = kind;
        pieces = spotPieces;
        areas = spotAreas;
        foreach (var piece in pieces)
        {
            var go = new GameObject("Marca");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = piece.offset;
            var mark = go.AddComponent<SpriteRenderer>();
            mark.sprite = CampArt.Get("spot");
            mark.sortingOrder = 5;
            marks.Add(mark);
        }
    }

    // A fogueira já nasce pronta: o ponto só adota a estrutura.
    public void Adopt(Structure structure)
    {
        Current = structure;
        structure.OnDestroyed += StructureDestroyed;
        ShowMarks(false);
    }

    // Distância até a peça mais próxima: pontos compridos são alcançados por qualquer ponta.
    public float SqrDistance(Vector2 point)
    {
        float best = float.MaxValue;
        foreach (var piece in pieces)
            best = Mathf.Min(best, ((Vector2)transform.position + piece.offset - point).sqrMagnitude);
        return best;
    }

    bool NextCost(out int wood, out int stone, out string label)
    {
        wood = stone = 0;
        label = "";
        if (Current)
        {
            if (Current.IsDamaged)
            {
                int cost = Mathf.CeilToInt(Current.Missing / HealthPerMaterial);
                if (Current.Kind == StructureKind.Wall || Current.Kind == StructureKind.StoneGate) stone = cost;
                else wood = cost;
                label = "Reparar";
                return true;
            }
            switch (Current.Kind)
            {
                case StructureKind.Fence: stone = WallStone; label = "Reforçar com muro"; return true;
                case StructureKind.Gate: stone = GateStone; label = "Reforçar portão com pedra"; return true;
                default: return false;
            }
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
        if (!Current && Kind != SpotKind.Gate && PlayerOnSpot()) { Warn("Saia do local para construir"); return false; }
        if (!bag.TrySpend(wood, stone)) return false;

        if (!Current) Create();
        else if (Current.IsDamaged) Current.RepairFull();
        else if (Current.Kind == StructureKind.Fence) Become(StructureKind.Wall, WallHealth);
        else Become(StructureKind.StoneGate, StoneGateHealth);
        FxManager.Burst(transform.position, new Color(0.9f, 0.75f, 0.45f), 10, 3f);
        AudioManager.Play(Sfx.Select, 0.8f);
        return true;
    }

    void Create()
    {
        var go = new GameObject("Structure");
        go.transform.SetParent(transform, false);
        // Sem Rigidbody2D: colisores estáticos.
        foreach (var area in areas)
        {
            var box = go.AddComponent<BoxCollider2D>();
            box.offset = area.center;
            box.size = area.size;
            // Portão: bloqueia monstros e deixa o jogador passar, então ninguém fica preso na base.
            if (Kind == SpotKind.Gate && CampSystem.Instance && CampSystem.Instance.PlayerCollider)
                Physics2D.IgnoreCollision(box, CampSystem.Instance.PlayerCollider);
        }
        renderers.Clear();
        foreach (var piece in pieces)
        {
            var child = new GameObject("Peca");
            child.transform.SetParent(go.transform, false);
            child.transform.localPosition = piece.offset;
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 10;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderers.Add(renderer);
        }
        Current = go.AddComponent<Structure>();
        Current.OnDestroyed += StructureDestroyed;
        ShowMarks(false);
        switch (Kind)
        {
            case SpotKind.Wall: Become(StructureKind.Fence, FenceHealth); break;
            case SpotKind.Gate:
                gate = go.AddComponent<GateVisual>();
                gate.Setup(renderers[0], pieces[0].index == 0);
                Become(StructureKind.Gate, GateHealth);
                break;
            case SpotKind.WeaponStation:
            case SpotKind.ArmorStation:
                stationLoop = renderers[0].gameObject.AddComponent<SpriteLoop>();
                stationLoop.frameTime = StationFrameTime;
                StationArt();
                Current.Init(StructureKind.Station, StationHealth);
                break;
        }
    }

    // Também serve para melhorar: troca o tipo, a arte e volta com a vida cheia.
    void Become(StructureKind kind, float health)
    {
        switch (kind)
        {
            case StructureKind.Fence: Pieces("fence"); break;
            case StructureKind.Wall: Pieces("wall"); break;
            case StructureKind.Gate: gate.SetFrames(CampArt.Sheet("gate_wood", 48, 64, PiecePivot)); break;
            case StructureKind.StoneGate: gate.SetFrames(CampArt.Sheet("gate_stone", 48, 64, PiecePivot)); break;
        }
        Current.Init(kind, health);
    }

    void Pieces(string sheet)
    {
        var frames = CampArt.Sheet(sheet, 48, 64, PiecePivot);
        for (int i = 0; i < renderers.Count; i++) renderers[i].sprite = CampArt.Pick(frames, pieces[i].index);
    }

    // Com o NPC no acampamento, a barraca mostra ele trabalhando; sem ele, fica vazia e fechada.
    public void ShowResident(bool resident)
    {
        if (stationResident == resident) return;
        stationResident = resident;
        if (IsBuiltStation) StationArt();
    }

    void StationArt()
    {
        string sheet = (Kind == SpotKind.WeaponStation ? "station_weapon" : "station_armor") + (stationResident ? "_npc" : "");
        var frames = CampArt.Sheet(sheet, 64, 68, StationPivot);
        renderers[0].sprite = frames[0];
        stationLoop.frames = frames;
    }

    void StructureDestroyed(Structure structure)
    {
        if (Kind == SpotKind.Heart) return; // A derrota é tratada pelo CampSystem.
        structure.OnDestroyed -= StructureDestroyed;
        Destroy(structure.gameObject);
        Current = null;
        gate = null;
        stationLoop = null;
        renderers.Clear();
        ShowMarks(true);
    }

    void ShowMarks(bool show)
    {
        foreach (var mark in marks) mark.enabled = show;
    }

    bool PlayerOnSpot()
    {
        foreach (var area in areas)
        foreach (var hit in Physics2D.OverlapBoxAll(transform.TransformPoint(area.center), area.size, 0f))
            if (hit.GetComponent<PlayerStats>()) return true;
        return false;
    }

    void Warn(string text) => FxManager.Popup(transform.position + Vector3.up, text, new Color(1f, 0.55f, 0.4f));
}
