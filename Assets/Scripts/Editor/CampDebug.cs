using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Atalhos de teste do acampamento. Não entram no build.
[InitializeOnLoad]
public static class CampDebug
{
    const string ReadyCampKey = "CampDebug.ReadyCamp";
    const string GameScene = "Assets/Scenes/Game.unity";

    static CampDebug() => EditorApplication.playModeStateChanged += OnPlayModeChanged;

    // Abre a cena do jogo e começa a run com tudo construído. Durante o Play, constrói na hora.
    // Lados de cima e da esquerda ficam de pedra; de baixo e da direita, de madeira; as duas estações prontas.
    [MenuItem("Survivors/Testes/Jogar com acampamento pronto")]
    static void PlayWithReadyCamp()
    {
        if (Application.isPlaying) { BuildReadyCamp(); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene(GameScene);
        SessionState.SetBool(ReadyCampKey, true);
        EditorApplication.EnterPlaymode();
    }

    static void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(ReadyCampKey, false)) return;
        SessionState.EraseBool(ReadyCampKey);
        EditorApplication.update += WaitForCamp;
    }

    // O inventário do acampamento só existe depois do Start.
    static void WaitForCamp()
    {
        if (!Application.isPlaying) { EditorApplication.update -= WaitForCamp; return; }
        if (!CampSystem.Instance || !CampSystem.Instance.Inventory) return;
        EditorApplication.update -= WaitForCamp;
        BuildReadyCamp();
    }

    static void BuildReadyCamp()
    {
        var camp = CampSystem.Instance;
        if (!camp || !camp.Inventory) return;
        var bag = camp.Inventory;
        bag.Add(ResourceKind.Wood, 1000);
        bag.Add(ResourceKind.Stone, 1000);
        float edge = CampSystem.RingCells / 2 * CampArt.Cell * 0.9f;
        foreach (var spot in camp.Spots)
        {
            if (spot.Kind == SpotKind.Heart) continue;
            spot.Locked = false;
            if (!spot.Current) spot.TryBuild(bag);
            var offset = spot.transform.position - camp.Center;
            bool stone = offset.y > edge || offset.x < -edge;
            if (stone && spot.Current && (spot.Current.Kind == StructureKind.Fence || spot.Current.Kind == StructureKind.Gate))
                spot.TryBuild(bag);
        }
    }

    [MenuItem("Survivors/Testes/Adicionar 100 de madeira e pedra")]
    static void AddMaterials()
    {
        var bag = Application.isPlaying ? Object.FindAnyObjectByType<ResourceInventory>() : null;
        if (!bag) return;
        bag.Add(ResourceKind.Wood, 100);
        bag.Add(ResourceKind.Stone, 100);
    }

    // O resgate ainda não existe: libera os pontos de estação para ver a arte em jogo.
    [MenuItem("Survivors/Testes/Liberar pontos de estação")]
    static void UnlockStations()
    {
        if (!Application.isPlaying || !CampSystem.Instance) return;
        foreach (var spot in CampSystem.Instance.Spots)
            if (spot.Kind == SpotKind.WeaponStation || spot.Kind == SpotKind.ArmorStation) spot.Locked = false;
    }

    [MenuItem("Survivors/Testes/Causar 50 de dano em tudo do acampamento")]
    static void DamageCamp()
    {
        if (!Application.isPlaying) return;
        foreach (var structure in Structure.Active.ToArray())
            if (structure.Kind != StructureKind.Heart) structure.TakeDamage(50f);
    }

    [MenuItem("Survivors/Testes/Causar 50 de dano na fogueira")]
    static void DamageHeart()
    {
        if (Application.isPlaying && CampSystem.Instance && CampSystem.Instance.Heart)
            CampSystem.Instance.Heart.TakeDamage(50f);
    }
}
