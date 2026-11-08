using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Identidade visual da interface: fonte pixelada (Jersey 10, licença OFL), painéis opacos com
// moldura e tamanhos de texto em múltiplos de 10, que caem no grid de pixels da fonte.
public static class UIStyle
{
    public static readonly Color PanelColor = new Color(0.11f, 0.075f, 0.05f);
    public static readonly Color BorderColor = new Color(0.52f, 0.36f, 0.19f);
    public static readonly Color ButtonColor = new Color(0.33f, 0.22f, 0.13f);
    public static readonly Color ButtonPrimary = new Color(0.25f, 0.4f, 0.2f);
    public static readonly Color Gold = new Color(1f, 0.83f, 0.42f);
    public static readonly Color Muted = new Color(0.72f, 0.65f, 0.55f);
    public static readonly Color Good = new Color(0.62f, 1f, 0.5f);
    public static readonly Color Bad = new Color(1f, 0.5f, 0.4f);
    public static readonly Color Info = new Color(0.65f, 0.85f, 1f);
    public static readonly Color Veil = new Color(0.03f, 0.02f, 0.01f, 0.85f); // atrás das janelas

    public const int Small = 20, Body = 30, Large = 40, Title = 60, Huge = 80;
    public const float Border = 4f, Padding = 14f;

    static Font font;

    public static Font Font
    {
        get
        {
            if (!font) font = Resources.Load<Font>("Fonts/Jersey10");
            if (!font) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }
    }

    // Canvas de tela no padrão do jogo: 1920x1080 de referência e pixel perfect.
    public static Transform Canvas(Transform parent, string name, int order, bool interactive = false)
    {
        var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler));
        go.transform.SetParent(parent, false);
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = order;
        canvas.pixelPerfect = true;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        if (interactive) go.AddComponent<GraphicRaycaster>();
        return go.transform;
    }

    // Painel opaco com moldura. Devolve o miolo, onde o conteúdo é posto.
    public static RectTransform Panel(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        var frame = Image(parent, name, BorderColor);
        var rect = frame.rectTransform;
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        var inner = Image(rect, "Miolo", PanelColor);
        Stretch(inner.rectTransform, Border);
        return inner.rectTransform;
    }

    public static Image Image(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static Text Label(Transform parent, string content, int size, TextAnchor align, Color color)
    {
        var go = new GameObject("Texto", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        Style(text, size);
        text.text = content;
        text.alignment = align;
        text.color = color;
        text.raycastTarget = false;
        text.supportRichText = true;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    // Rótulo num ponto fixo do pai (âncora no centro).
    public static Text Label(Transform parent, string content, int size, Vector2 position, Vector2 box, TextAnchor align = TextAnchor.MiddleCenter)
    {
        var text = Label(parent, content, size, align, Color.white);
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = Vector2.one * 0.5f;
        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = box;
        return text;
    }

    public static void Stretch(RectTransform rect, float inset = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    // Fonte, tamanho no grid e sombra dura de 2 px, sem transparência.
    public static void Style(Text text, int size)
    {
        text.font = Font;
        text.fontSize = Snap(size);
        var shadow = text.GetComponent<Shadow>();
        if (!shadow) shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectDistance = new Vector2(2f, -2f);
        shadow.effectColor = Color.black;
    }

    public static int Snap(int size)
    {
        if (size <= 24) return Small;
        if (size <= 34) return Body;
        if (size <= 50) return Large;
        if (size <= 70) return Title;
        return Huge;
    }

    // Botão no padrão do jogo: moldura, fundo opaco e realce claro ao passar o mouse.
    public static void StyleButton(Button button, bool primary = false)
    {
        var image = button.GetComponent<Image>();
        if (image)
        {
            image.sprite = null;
            image.color = primary ? ButtonPrimary : ButtonColor;
            var outline = button.GetComponent<Outline>();
            if (!outline) outline = button.gameObject.AddComponent<Outline>();
            outline.effectColor = BorderColor;
            outline.effectDistance = new Vector2(Border, Border);
        }
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.35f, 1.3f, 1.15f);
        colors.selectedColor = new Color(1.25f, 1.2f, 1.1f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        colors.colorMultiplier = 1.5f;
        button.colors = colors;
    }

    // As cenas geradas usam a fonte padrão do Unity e sombras translúcidas: aplica o estilo
    // em todos os textos, botões e canvases já existentes.
    public static void ApplyToScene()
    {
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            canvas.pixelPerfect = true;
        foreach (var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Style(text, text.fontSize);
            if (text.color.a < 1f) { var c = text.color; c.a = 1f; text.color = c; }
        }
        foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string name = button.name;
            StyleButton(button, name == "Jogar" || name == "Continuar" || name == "JogarNovamente");
        }
    }

    // Novo botão numa coluna de menu já existente, copiando o tamanho de outro botão.
    public static Button CloneButton(GameObject template, string label, int siblingIndex, UnityAction action)
    {
        var copy = Object.Instantiate(template, template.transform.parent);
        copy.name = label;
        copy.SetActive(true);
        copy.transform.SetSiblingIndex(siblingIndex);
        var text = copy.GetComponentInChildren<Text>(true);
        if (text) text.text = label;
        var button = copy.GetComponent<Button>();
        StyleButton(button);
        button.onClick = new Button.ButtonClickedEvent(); // sem os ouvintes do original
        button.onClick.AddListener(action);
        return button;
    }

    // Botão criado do zero (janelas feitas por código).
    public static Button Button(Transform parent, string content, int size, Vector2 position, Vector2 box, UnityAction action, out Text label)
    {
        var image = Image(parent, content, ButtonColor);
        image.raycastTarget = true;
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.one * 0.5f;
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = box;
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        StyleButton(button);
        if (action != null) button.onClick.AddListener(action);
        label = Label(image.transform, content, size, Vector2.zero, box - new Vector2(16f, 8f));
        return button;
    }

    public static string Heading(Vector3 offset)
    {
        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y)) return offset.x > 0 ? "leste" : "oeste";
        return offset.y > 0 ? "norte" : "sul";
    }
}
