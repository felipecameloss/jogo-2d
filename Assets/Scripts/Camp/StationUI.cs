using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Janela da ferraria e da armaria: até 5 compras (1 a 5 ou clique), Esc fecha.
public class StationUI : MonoBehaviour
{
    const int MaxCards = 5;
    const float CardWidth = 300f, CardSpacing = 320f;

    public StationSystem source;
    GameObject modal;
    Text heading, materials, message, empty;
    readonly GameObject[] cards = new GameObject[MaxCards];
    readonly Text[] titles = new Text[MaxCards], descriptions = new Text[MaxCards], costs = new Text[MaxCards];
    readonly Button[] buttons = new Button[MaxCards];
    bool wasOpen;

    void Start()
    {
        var root = new GameObject("StationUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var shade = Panel(root.transform, "StationModal", Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.04f, 0.03f, 0.02f, 0.88f));
        shade.rectTransform.anchorMin = Vector2.zero;
        shade.rectTransform.anchorMax = Vector2.one;
        shade.rectTransform.offsetMin = shade.rectTransform.offsetMax = Vector2.zero;
        shade.raycastTarget = true;
        modal = shade.gameObject;
        var window = Panel(modal.transform, "Window", Vector2.one * 0.5f, Vector2.zero, new Vector2(1700, 600), new Color(0.16f, 0.11f, 0.07f));
        heading = Label(window.transform, "Heading", new Vector2(0, 240), new Vector2(1600, 60), 38);
        materials = Label(window.transform, "Materials", new Vector2(0, 185), new Vector2(1600, 40), 24);
        materials.color = new Color(1, 0.83f, 0.37f);
        for (int i = 0; i < MaxCards; i++)
        {
            int index = i;
            var card = Panel(window.transform, "Offer" + i, Vector2.one * 0.5f, new Vector2((i - (MaxCards - 1) / 2f) * CardSpacing, -10), new Vector2(CardWidth, 250), new Color(0.27f, 0.19f, 0.12f));
            card.raycastTarget = true;
            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card;
            button.onClick.AddListener(() => source.Buy(index));
            var colors = button.colors;
            colors.highlightedColor = new Color(1, 0.88f, 0.6f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            buttons[i] = button;
            cards[i] = card.gameObject;
            titles[i] = Label(card.transform, "Title", new Vector2(0, 80), new Vector2(280, 60), 25);
            descriptions[i] = Label(card.transform, "Description", new Vector2(0, -5), new Vector2(275, 110), 21);
            costs[i] = Label(card.transform, "Cost", new Vector2(0, -95), new Vector2(280, 40), 21);
        }
        empty = Label(window.transform, "Empty", new Vector2(0, -10), new Vector2(1600, 60), 30);
        empty.text = "Nada mais a ensinar";
        message = Label(window.transform, "Message", new Vector2(0, -170), new Vector2(1600, 40), 23);
        var close = Panel(window.transform, "Close", Vector2.one * 0.5f, new Vector2(0, -240), new Vector2(430, 54), new Color(0.35f, 0.25f, 0.16f));
        close.raycastTarget = true;
        close.gameObject.AddComponent<Button>().onClick.AddListener(source.Close);
        Label(close.transform, "Label", Vector2.zero, new Vector2(410, 50), 23).text = "Fechar [Esc / P / Start]";
        modal.SetActive(false);
    }

    void Update()
    {
        if (!source || !modal) return;
        bool open = source.Open;
        modal.SetActive(open);
        if (!open) { wasOpen = false; return; }

        var bag = source.Inventory;
        heading.text = source.Heading;
        materials.text = bag ? $"Madeira {bag.Wood}  •  Pedra {bag.Stone}  •  compre com 1 a 5 ou clique" : "";
        message.text = source.Message;
        var offers = source.Offers;
        empty.gameObject.SetActive(offers.Count == 0);
        for (int i = 0; i < MaxCards; i++)
        {
            bool used = i < offers.Count;
            cards[i].SetActive(used);
            if (!used) continue;
            var offer = offers[i];
            titles[i].text = $"[{i + 1}] {offer.Title}";
            descriptions[i].text = offer.Description;
            costs[i].text = BuildSpot.Cost(offer.Wood, offer.Stone);
            costs[i].color = bag && bag.CanAfford(offer.Wood, offer.Stone) ? new Color(0.75f, 1f, 0.6f) : new Color(1f, 0.5f, 0.4f);
        }
        if (!wasOpen && EventSystem.current && offers.Count > 0) EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        wasOpen = true;
        int option = GameInput.OptionPressed();
        if (option >= 0) source.Buy(option);
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

    static Text Label(Transform parent, string name, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = UIStyle.Font;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = Vector2.one * 0.5f;
        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = size;
        return text;
    }
}
