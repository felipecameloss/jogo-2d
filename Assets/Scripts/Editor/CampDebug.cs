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

    [MenuItem("Survivors/Testes/Causar 50 de dano na fogueira")]
    static void DamageHeart()
    {
        if (Application.isPlaying && CampSystem.Instance && CampSystem.Instance.Heart)
            CampSystem.Instance.Heart.TakeDamage(50f);
    }
}
