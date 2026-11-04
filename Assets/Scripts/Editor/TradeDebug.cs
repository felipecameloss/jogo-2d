#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class TradeDebug
{
    [MenuItem("Survivors/Testes/Adicionar 100 de ouro")]
    static void AddTestGold()
    {
        var player = GameManager.Instance.Player;
        player.GetComponent<PlayerWallet>().Add(100);
        Debug.Log("Teste: +100 ouro. Use F perto de um baú ou mercador.");
    }

    [MenuItem("Survivors/Testes/Adicionar 100 de ouro", true)]
    static bool CanAddGold() => Application.isPlaying && GameManager.Instance && GameManager.Instance.Player &&
        GameManager.Instance.Player.GetComponent<PlayerWallet>();
}
#endif
