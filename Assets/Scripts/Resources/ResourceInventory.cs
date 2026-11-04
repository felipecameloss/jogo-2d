using System;
using UnityEngine;

public enum ResourceKind { Wood, Stone, Crystal }

// Materiais da run. Custos compostos são pagos integralmente ou não são pagos.
public class ResourceInventory : MonoBehaviour
{
    public int Wood { get; private set; }
    public int Stone { get; private set; }
    public int Crystal { get; private set; }
    public int TotalCollected { get; private set; }
    public event Action OnChanged;

    public int Count(ResourceKind kind)
    {
        switch (kind)
        {
            case ResourceKind.Wood: return Wood;
            case ResourceKind.Stone: return Stone;
            case ResourceKind.Crystal: return Crystal;
            default: throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    public void Add(ResourceKind kind, int amount)
    {
        if (amount <= 0) return;
        // Valida antes de alterar qualquer saldo.
        int balance = checked(Count(kind) + amount);
        int total = checked(TotalCollected + amount);
        switch (kind)
        {
            case ResourceKind.Wood: Wood = balance; break;
            case ResourceKind.Stone: Stone = balance; break;
            case ResourceKind.Crystal: Crystal = balance; break;
        }
        TotalCollected = total;
        OnChanged?.Invoke();
    }

    public bool CanAfford(int wood, int stone, int crystal) =>
        wood >= 0 && stone >= 0 && crystal >= 0 && Wood >= wood && Stone >= stone && Crystal >= crystal;

    public bool TrySpend(int wood, int stone, int crystal)
    {
        if (!CanAfford(wood, stone, crystal)) return false;
        Wood -= wood;
        Stone -= stone;
        Crystal -= crystal;
        OnChanged?.Invoke();
        return true;
    }
}
