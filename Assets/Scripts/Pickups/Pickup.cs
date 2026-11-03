using System.Collections.Generic;
using UnityEngine;

public enum PickupType { Experience, Heal, Magnet }

// Itens no chão: gemas de XP, cura e ímã. São atraídos quando o jogador chega perto.
public class Pickup : MonoBehaviour
{
    static readonly List<Pickup> All = new List<Pickup>();

    public PickupType type = PickupType.Experience;
    public float value = 1f;
    public float collectDistance = 0.4f;

    bool attracted;
    float speed;
    PlayerStats stats;
    PlayerExperience xp;
    SpriteRenderer sr;

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (type == PickupType.Experience && sr)
        {
            // Gemas mais valiosas têm outra cor.
            if (value >= 20f) sr.color = new Color(1f, 0.35f, 0.4f);
            else if (value >= 5f) sr.color = new Color(0.4f, 1f, 0.5f);
        }

        var gm = GameManager.Instance;
        if (gm && gm.Player)
        {
            stats = gm.Player;
            xp = stats.GetComponent<PlayerExperience>();
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
            case PickupType.Magnet:
                foreach (var p in All)
                    if (p.type == PickupType.Experience) p.attracted = true;
                FxManager.Popup(transform.position + Vector3.up * 0.5f, "ÍMÃ!", new Color(0.6f, 0.8f, 1f), 1.3f);
                AudioManager.Play(Sfx.LevelUp, 0.5f);
                break;
        }
        Destroy(gameObject);
    }
}
