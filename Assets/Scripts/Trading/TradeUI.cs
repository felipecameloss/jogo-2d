using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TradeUI : MonoBehaviour
{
    public TradeSystem source;
    GameObject modal;
    Text heading, balance, notice, nearby, direction;
    readonly Text[] titles = new Text[3], descriptions = new Text[3], prices = new Text[3];
    readonly Button[] buttons = new Button[3];
    bool wasOpen;

    void Start()
    {
        var root = new GameObject("TradeUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 150;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        direction = Label(root.transform, "MerchantDirection", new Vector2(1, 1), new Vector2(-230, -170), new Vector2(440, 55), 23);
        direction.color = new Color(1, 0.84f, 0.45f);
        nearby = Label(root.transform, "Interaction", new Vector2(0.5f, 0), new Vector2(0, 235), new Vector2(1000, 60), 25);
        nearby.color = new Color(1, 0.9f, 0.65f);
        var shade = Panel(root.transform, "TradeModal", Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.015f, 0.025f, 0.06f, 0.88f));
        shade.rectTransform.anchorMin = Vector2.zero;
        shade.rectTransform.anchorMax = Vector2.one;
        shade.rectTransform.offsetMin = shade.rectTransform.offsetMax = Vector2.zero;
        shade.raycastTarget = true;
        modal = shade.gameObject;
        var window = Panel(modal.transform, "Window", Vector2.one * 0.5f, Vector2.zero, new Vector2(1160, 620), new Color(0.07f, 0.11f, 0.17f));
        heading = Label(window.transform, "Heading", Vector2.one * 0.5f, new Vector2(0, 240), new Vector2(1080, 60), 38);
        balance = Label(window.transform, "Balance", Vector2.one * 0.5f, new Vector2(0, 185), new Vector2(1080, 45), 25);
        balance.color = new Color(1, 0.83f, 0.37f);
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            var card = Panel(window.transform, "Offer" + i, Vector2.one * 0.5f, new Vector2((i - 1) * 365, 5), new Vector2(345, 280), new Color(0.13f, 0.2f, 0.27f));
            card.raycastTarget = true;
            var button = card.gameObject.AddComponent<Button>();
            button.targetGraphic = card;
            button.onClick.AddListener(() => source.Buy(index));
            var colors = button.colors;
            colors.highlightedColor = new Color(1, 0.88f, 0.6f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f);
            button.colors = colors;
            buttons[i] = button;
            titles[i] = Label(card.transform, "Title", Vector2.one * 0.5f, new Vector2(0, 90), new Vector2(315, 65), 26);
            descriptions[i] = Label(card.transform, "Description", Vector2.one * 0.5f, new Vector2(0, 0), new Vector2(300, 105), 23);
            prices[i] = Label(card.transform, "Price", Vector2.one * 0.5f, new Vector2(0, -100), new Vector2(315, 60), 22);
        }
        notice = Label(window.transform, "Notice", Vector2.one * 0.5f, new Vector2(0, -170), new Vector2(1100, 65), 23);
        var close = Panel(window.transform, "Close", Vector2.one * 0.5f, new Vector2(0, -250), new Vector2(430, 54), new Color(0.22f, 0.29f, 0.36f));
        close.raycastTarget = true;
        close.gameObject.AddComponent<Button>().onClick.AddListener(source.Close);
        Label(close.transform, "Label", Vector2.one * 0.5f, Vector2.zero, new Vector2(410, 50), 23).text = "Voltar ao jogo [Esc / P / Start]";
        modal.SetActive(false);
    }

    void Update()
    {
        if (!source || !GameManager.Instance || !modal) return;
        bool open = source.IsOpen;
        modal.SetActive(open);
        bool playing = GameManager.Instance.State == GameState.Playing;
        nearby.gameObject.SetActive(playing);
        direction.gameObject.SetActive(playing);
        nearby.text = source.NearbyPrompt;
        direction.text = source.MerchantDirection;
        if (!open) { wasOpen = false; return; }
        heading.text = source.Heading;
        balance.text = $"Seu ouro: {source.Wallet.Gold}  •  Escolha com 1 / 2 / 3 ou clique";
        notice.text = source.Notice;
        var offers = source.Offers;
        var player = GameManager.Instance.Player;
        for (int i = 0; i < 3; i++)
        {
            var offer = offers[i];
            titles[i].text = $"[{i + 1}] {offer.Title}";
            descriptions[i].text = offer.Description;
            bool usable = offer.CanUse(player);
            bool affordable = source.Wallet.CanAfford(offer.Price);
            buttons[i].interactable = usable && affordable;
            prices[i].text = offer.Sold ? "ESGOTADO" : !usable ? "VIDA CHEIA" : affordable ? $"{offer.Price} OURO" : $"{offer.Price} OURO • saldo insuficiente";
            prices[i].color = usable && affordable ? new Color(1, 0.83f, 0.37f) : new Color(0.7f, 0.7f, 0.75f);
        }
        if (!wasOpen && EventSystem.current)
        {
            foreach (var button in buttons)
                if (button.interactable) { EventSystem.current.SetSelectedGameObject(button.gameObject); break; }
        }
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

    static Text Label(Transform parent, string name, Vector2 anchor, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
