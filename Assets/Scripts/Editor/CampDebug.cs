using UnityEditor;
using UnityEngine;

// Atalhos de teste do acampamento. Não entram no build.
public static class CampDebug
{
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
