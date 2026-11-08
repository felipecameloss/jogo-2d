using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Aviso de interação, direção do baú mais próximo e a janela "escolha 1 de 3".
public class ChestUI : MonoBehaviour
{
    public ChestSystem source;
    GameObject modal;
    Text heading, nearby, direction;
    readonly Text[] titles = new Text[3], descriptions = new Text[3];
    readonly Button[] buttons = new Button[3];
    bool wasOpen;

    void Start()
    {
        var root = new GameObject("ChestUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        direction = Label(root.transform, "ChestDirection", new Vector2(1, 1), new Vector2(-250, -170), new Vector2(480, 55), 23);
        direction.color = new Color(1, 0.84f, 0.45f);
        nearby = Label(root.transform, "Interaction", new Vector2(0.5f, 0), new Vector2(0, 235), new Vector2(1000, 60), 25);
        nearby.color = new Color(1, 0.9f, 0.65f);

        var shade = Panel(root.transform, "ChestModal", Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.015f, 0.025f, 0.06f, 0.88f));
        shade.rectTransform.anchorMin = Vector2.zero;
        shade.rectTransform.anchorMax = Vector2.one;
        shade.rectTransform.offsetMin = shade.rectTransform.offsetMax = Vector2.zero;
        shade.raycastTarget = true;
        modal = shade.gameObject;
        var window = Panel(modal.transform, "Window", Vector2.one * 0.5f, Vector2.zero, new Vector2(1160, 560), new Color(0.07f, 0.11f, 0.17f));
        heading = Label(window.transform, "Heading", Vector2.one * 0.5f, new Vector2(0, 220), new Vector2(1080, 60), 38);
        var hint = Label(window.transform, "Hint", Vector2.one * 0.5f, new Vector2(0, 165), new Vector2(1080, 45), 24);
        hint.color = new Color(1, 0.83f, 0.37f);
        hint.text = "Escolha UMA recompensa (1 / 2 / 3 ou clique) • a chave só é gasta ao escolher";
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            var card = Panel(window.transform, "Offer" + i, Vector2.one * 0.5f, new Vector2((i - 1) * 365, 0), new Vector2(345, 240), new Color(0.13f, 0.2f, 0.27f));
            card.raycastTarget = true;
            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card;
            button.onClick.AddListener(() => source.Choose(index));
            var colors = button.colors;
            colors.highlightedColor = new Color(1, 0.88f, 0.6f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            buttons[i] = button;
            titles[i] = Label(card.transform, "Title", Vector2.one * 0.5f, new Vector2(0, 70), new Vector2(315, 65), 26);
            descriptions[i] = Label(card.transform, "Description", Vector2.one * 0.5f, new Vector2(0, -25), new Vector2(300, 120), 23);
        }
        var close = Panel(window.transform, "Close", Vector2.one * 0.5f, new Vector2(0, -215), new Vector2(430, 54), new Color(0.22f, 0.29f, 0.36f));
        close.raycastTarget = true;
        close.gameObject.AddComponent<Button>().onClick.AddListener(source.Close);
        Label(close.transform, "Label", Vector2.one * 0.5f, Vector2.zero, new Vector2(410, 50), 23).text = "Fechar sem abrir [Esc / P / Start]";
        modal.SetActive(false);
    }

    void Update()
    {
        if (!source || !GameManager.Instance || !modal) return;
        bool open = source.ShowOffers;
        modal.SetActive(open);
        bool playing = GameManager.Instance.State == GameState.Playing;
        nearby.gameObject.SetActive(playing);
        direction.gameObject.SetActive(playing);
        nearby.text = source.Prompt;
        direction.text = source.Direction;
        if (!open) { wasOpen = false; return; }
        heading.text = source.Heading;
        var offers = source.Offers;
        for (int i = 0; i < 3; i++)
        {
            titles[i].text = $"[{i + 1}] {offers[i].Title}";
            descriptions[i].text = offers[i].Description;
        }
        if (!wasOpen && EventSystem.current) EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        wasOpen = true;
        int option = GameInput.OptionPressed();
        if (option >= 0) source.Choose(option);
    }

    static Image Panel(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = anchor;
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = size;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static Text Label(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = UIStyle.Font;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = anchor;
        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = size;
        return text;
    }
}
