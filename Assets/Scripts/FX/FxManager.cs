using UnityEngine;

// Efeitos visuais simples: partículas, números de dano e tremida de câmera.
public class FxManager : MonoBehaviour
{
    public static FxManager Instance { get; private set; }

    public Sprite particleSprite;
    public Font popupFont;

    void Awake()
    {
        Instance = this;
        if (!popupFont) popupFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    public static void Burst(Vector3 position, Color color, int count = 8, float speed = 4f, float size = 0.18f)
    {
        if (!Instance || !Instance.particleSprite) return;
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("FxParticle");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * size * Random.Range(0.6f, 1.2f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Instance.particleSprite;
            sr.color = color;
            sr.sortingOrder = 50;
            var p = go.AddComponent<FxParticle>();
            p.velocity = Random.insideUnitCircle.normalized * speed * Random.Range(0.4f, 1f);
            p.lifetime = Random.Range(0.25f, 0.5f);
        }
    }

    public static void Popup(Vector3 position, string text, Color color, float scale = 1f)
    {
        if (!Instance || !Instance.popupFont) return;
        var go = new GameObject("Popup");
        go.transform.position = position + (Vector3)(Random.insideUnitCircle * 0.15f);
        go.transform.localScale = Vector3.one * scale;
        var tm = go.AddComponent<TextMesh>();
        tm.font = Instance.popupFont;
        tm.text = text;
        tm.color = color;
        tm.fontSize = 48;
        tm.characterSize = 0.06f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = Instance.popupFont.material;
        mr.sortingOrder = 100;
        go.AddComponent<FloatingText>();
    }

    public static void Shake(float intensity, float duration)
    {
        if (CameraFollow.Instance) CameraFollow.Instance.Shake(intensity, duration);
    }
}
