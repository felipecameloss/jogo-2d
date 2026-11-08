using UnityEngine;
using UnityEngine.UI;

// Vida da fogueira, aviso de construção e direção da pessoa perdida mais próxima.
public class CampHUD : MonoBehaviour
{
    const float BarWidth = 420f, LowHealth = 0.3f;

    public CampSystem source;
    GameObject root;
    Text label, prompt, direction;
    Image fill;

    void Start()
    {
        root = new GameObject("CampHUD", typeof(Canvas), typeof(CanvasScaler));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1; // Os menus continuam por cima.
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var track = Box(root.transform, new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(BarWidth + 8f, 26f), new Color(0.05f, 0.05f, 0.07f, 0.85f));
        fill = Box(track.transform, new Vector2(0f, 0.5f), new Vector2(4f, 0f), new Vector2(BarWidth, 18f), new Color(1f, 0.55f, 0.15f));
        fill.rectTransform.pivot = new Vector2(0f, 0.5f);
        label = Text(track.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, -32f), new Vector2(700f, 34f), 23);
        prompt = Text(root.transform, new Vector2(0.5f, 0f), new Vector2(0f, 300f), new Vector2(1000f, 50f), 25);
        prompt.color = new Color(1f, 0.9f, 0.65f);
        // Logo abaixo da direção do baú, que fica no canto superior direito.
        direction = Text(root.transform, new Vector2(1f, 1f), new Vector2(-250f, -225f), new Vector2(480f, 55f), 23);
        direction.color = new Color(0.7f, 0.9f, 1f);
    }

    void Update()
    {
        if (!source || !GameManager.Instance || !root) return;
        bool show = GameManager.Instance.State == GameState.Playing;
        root.SetActive(show);
        if (!show) return;
        prompt.text = source.Prompt;
        direction.text = source.PersonDirection;
        var heart = source.Heart;
        float ratio = heart && heart.MaxHealth > 0f ? heart.Health / heart.MaxHealth : 0f;
        fill.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, BarWidth * ratio);
        bool low = ratio <= LowHealth;
        fill.color = low && Mathf.Sin(Time.time * 10f) > 0f ? new Color(1f, 0.2f, 0.15f) : new Color(1f, 0.55f, 0.15f);
        label.text = low ? "A FOGUEIRA ESTÁ SE APAGANDO!" : "Fogueira";
        label.color = low ? new Color(1f, 0.45f, 0.35f) : Color.white;
    }

    static Image Box(Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject("Box", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = anchor;
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = size;
        return image;
    }

    static Text Text(Transform parent, Vector2 anchor, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = anchor;
        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = size;
        return text;
    }
}
