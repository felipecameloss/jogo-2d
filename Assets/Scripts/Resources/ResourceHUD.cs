using UnityEngine;
using UnityEngine.UI;

public class ResourceHUD : MonoBehaviour
{
    public DayResourceSystem source;
    Text inventoryText, promptText;
    Image progress;
    GameObject canvasObject;

    void Start()
    {
        canvasObject = new GameObject("ResourceHUD", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1; // Os menus de pausa e level up continuam por cima.
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        var panel = Box("Materials", canvasObject.transform, new Vector2(0, 1), new Vector2(165, -160), new Vector2(290, 180), new Color(0.04f, 0.09f, 0.12f, 0.88f));
        inventoryText = TextBox(panel.transform, new Vector2(0, 0), new Vector2(260, 160), 23);
        inventoryText.alignment = TextAnchor.MiddleLeft;
        var prompt = Box("CollectPrompt", canvasObject.transform, new Vector2(0.5f, 0), new Vector2(0, 140), new Vector2(760, 70), new Color(0.04f, 0.09f, 0.12f, 0.88f));
        promptText = TextBox(prompt.transform, new Vector2(0, 6), new Vector2(740, 52), 22);
        var track = Box("ProgressTrack", prompt.transform, new Vector2(0.5f, 0), new Vector2(0, 7), new Vector2(720, 5), new Color(0.2f, 0.27f, 0.3f));
        progress = Box("Progress", track.transform, Vector2.zero, Vector2.zero, new Vector2(720, 5), new Color(0.55f, 0.9f, 0.4f));
        progress.rectTransform.anchorMin = progress.rectTransform.anchorMax = new Vector2(0, 0.5f);
        progress.rectTransform.pivot = new Vector2(0, 0.5f);
    }

    void Update()
    {
        if (!source || !source.Inventory || !GameManager.Instance) return;
        bool show = GameManager.Instance.State == GameState.Playing;
        canvasObject.SetActive(show);
        var bag = source.Inventory;
        inventoryText.text = $"<b>MATERIAIS DA VILA</b>\n<color=#A6DD77>Madeira</color>     {bag.Wood}\n<color=#C9D4ED>Pedra</color>          {bag.Stone}\n<color=#75EFFF>Cristal</color>         {bag.Crystal}";
        promptText.text = source.Prompt;
        progress.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 720 * source.Progress);
    }

    static Image Box(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        var rt = image.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        return image;
    }

    static Text TextBox(Transform parent, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        text.rectTransform.sizeDelta = size;
        text.rectTransform.anchoredPosition = position;
        return text;
    }
}
