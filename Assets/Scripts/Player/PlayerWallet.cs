using System;
using UnityEngine;

// Ouro do jogador. Por enquanto só acumula; depois vai ser usado em baús e na loja.
public class PlayerWallet : MonoBehaviour
{
    public int Gold { get; private set; }
    // Total ganho na run inteira (não diminui quando gastar).
    public int TotalEarned { get; private set; }

    // Chamado com o saldo novo sempre que o ouro muda.
    public event Action<int> OnGoldChanged;

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Gold += amount;
        TotalEarned += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool CanAfford(int cost) => Gold >= cost;

    // Tenta gastar. Retorna false (e não gasta nada) se não tiver ouro suficiente.
    public bool TrySpend(int cost)
    {
        if (cost < 0 || Gold < cost) return false;
        Gold -= cost;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
}
