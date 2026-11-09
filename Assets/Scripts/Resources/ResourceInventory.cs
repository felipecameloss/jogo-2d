using System;
using UnityEngine;

public enum ResourceKind { Wood, Stone }

// Materiais da run. Custos compostos são pagos integralmente ou não são pagos.
public class ResourceInventory : MonoBehaviour
{
    public int Wood { get; private set; }
    public int Stone { get; private set; }
    public int Keys { get; private set; }
    public int TotalCollected { get; private set; }
    public event Action OnChanged;

    public int Count(ResourceKind kind)
    {
        switch (kind)
        {
            case ResourceKind.Wood: return Wood;
            case ResourceKind.Stone: return Stone;
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
        }
        TotalCollected = total;
        OnChanged?.Invoke();
    }

    public void AddKey()
    {
        Keys++;
        OnChanged?.Invoke();
    }

    public bool UseKey()
    {
        if (Keys <= 0) return false;
        Keys--;
        OnChanged?.Invoke();
        return true;
    }

    public bool CanAfford(int wood, int stone) =>
        wood >= 0 && stone >= 0 && Wood >= wood && Stone >= stone;

    public bool TrySpend(int wood, int stone)
    {
        if (!CanAfford(wood, stone)) return false;
        Wood -= wood;
        Stone -= stone;
        OnChanged?.Invoke();
        return true;
    }
}
