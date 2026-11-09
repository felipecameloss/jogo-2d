using System.Collections.Generic;
using UnityEngine;

public enum PickupType { Experience, Heal, Magnet, Gold }

// Itens no chão: gemas de XP, cura, ímã e ouro. São atraídos quando o jogador chega perto.
public class Pickup : MonoBehaviour
{
    static readonly List<Pickup> All = new List<Pickup>();

    // Limite de gemas de XP no chão. Acima disso, o XP de novas gemas é somado a uma gema antiga
    // (como no Vampire Survivors), em vez de criar mais objetos com Update rodando.
    const int MaxXPGems = 250;
    static int xpGemCount;

    public PickupType type = PickupType.Experience;
    public float value = 1f;
    public float collectDistance = 0.4f;

    bool attracted;
    float speed;
    PlayerStats stats;
    PlayerExperience xp;
    PlayerWallet wallet;
    SpriteRenderer sr;

    void OnEnable()
    {
        All.Add(this);
        if (type == PickupType.Experience) xpGemCount++;
    }

    void OnDisable()
    {
        All.Remove(this);
        if (type == PickupType.Experience) xpGemCount--;
    }

    // Se já tem gemas demais, soma o valor numa gema antiga que ainda não está sendo puxada.
    // Retorna true se juntou (e então não é preciso criar uma gema nova).
    public static bool TryMergeXP(float value)
    {
        if (xpGemCount < MaxXPGems) return false;
        foreach (var p in All)
        {
            if (p.type != PickupType.Experience || p.attracted) continue;
            p.value += value;
            p.RefreshColor();
            return true;
        }
        return false;
    }

    void RefreshColor()
    {
        if (type != PickupType.Experience) return;
        if (!sr) sr = GetComponent<SpriteRenderer>();
        if (!sr) return;
        // Gemas mais valiosas têm outra cor.
        if (value >= 20f) sr.color = new Color(1f, 0.35f, 0.4f);
        else if (value >= 5f) sr.color = new Color(0.4f, 1f, 0.5f);
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        RefreshColor();
        // Montes de ouro maiores aparecem maiores.
        if (type == PickupType.Gold && value >= 5f) transform.localScale *= value >= 20f ? 1.6f : 1.25f;

        var gm = GameManager.Instance;
        if (gm && gm.Player)
        {
            stats = gm.Player;
            xp = stats.GetComponent<PlayerExperience>();
            wallet = stats.GetComponent<PlayerWallet>();
        }
    }

    void Update()
    {
        if (!stats || stats.IsDead || GameManager.Instance.State != GameState.Playing) return;

        Vector3 toPlayer = stats.transform.position - transform.position;
        float dist = toPlayer.magnitude;

        if (!attracted && dist <= stats.pickupRadius) attracted = true;
        if (attracted)
        {
            speed = Mathf.Min(speed + 30f * Time.deltaTime, 25f);
            transform.position += toPlayer.normalized * Mathf.Min(speed * Time.deltaTime, dist);
        }

        if (dist <= collectDistance) Collect();
    }

    void Collect()
    {
        switch (type)
        {
            case PickupType.Experience:
                if (xp) xp.AddXP(value);
                AudioManager.Play(Sfx.Pickup, 0.4f, 0.15f);
                break;
            case PickupType.Heal:
                stats.Heal(value);
                AudioManager.Play(Sfx.Pickup, 0.8f);
                break;
            case PickupType.Gold:
                int amount = Mathf.Max(1, Mathf.RoundToInt(value));
                if (wallet) wallet.Add(amount);
                FxManager.Popup(transform.position + Vector3.up * 0.4f, "+" + amount, new Color(1f, 0.85f, 0.25f), 0.9f);
                AudioManager.Play(Sfx.Pickup, 0.5f, 0.05f);
                break;
            case PickupType.Magnet:
                foreach (var p in All)
                    if (p.type == PickupType.Experience || p.type == PickupType.Gold) p.attracted = true;
                FxManager.Popup(transform.position + Vector3.up * 0.5f, "ÍMÃ!", new Color(0.6f, 0.8f, 1f), 1.3f);
                AudioManager.Play(Sfx.LevelUp, 0.5f);
                break;
        }
        Destroy(gameObject);
    }
}
