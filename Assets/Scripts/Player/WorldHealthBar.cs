using UnityEngine;

// Barrinha de vida embaixo do jogador (feedback dentro do mundo, sem depender do HUD).
public class WorldHealthBar : MonoBehaviour
{
    public PlayerStats stats;
    public Transform fill;
    public float width = 0.9f;

    void LateUpdate()
    {
        if (!stats || !fill) return;
        float r = Mathf.Clamp01(stats.CurrentHealth / stats.maxHealth);
        fill.localScale = new Vector3(width * r, fill.localScale.y, 1f);
        fill.localPosition = new Vector3(-width * (1f - r) * 0.5f, 0f, 0f);
    }
}
