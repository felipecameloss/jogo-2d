using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Janela da ferraria e da armaria: até 5 compras (1 a 5 ou clique), Esc fecha.
public class StationUI : MonoBehaviour
{
    const int MaxCards = 5;
    const float CardWidth = 310f, CardHeight = 400f, CardSpacing = 330f;
    const float IconSize = 72f; // ícones de 24 px, em 3x

    public StationSystem source;
    GameObject modal;
    Text heading, materials, message, empty;
    readonly GameObject[] cards = new GameObject[MaxCards];
    readonly Text[] titles = new Text[MaxCards], descriptions = new Text[MaxCards], costs = new Text[MaxCards];
    readonly Image[] icons = new Image[MaxCards];
    readonly Button[] buttons = new Button[MaxCards];
    bool wasOpen;

    void Start()
    {
        var canvas = UIStyle.Canvas(transform, "StationUI", 150, true);
        var veil = UIStyle.Image(canvas, "Veu", UIStyle.Veil);
        UIStyle.Stretch(veil.rectTransform);
        veil.raycastTarget = true;
        modal = veil.gameObject;
        var window = UIStyle.Panel(modal.transform, "Janela", Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero, new Vector2(1720f, 760f));
        heading = UIStyle.Label(window, "", UIStyle.Title, new Vector2(0f, 310f), new Vector2(1600f, 80f));
        heading.color = UIStyle.Gold;
        materials = UIStyle.Label(window, "", UIStyle.Body, new Vector2(0f, 245f), new Vector2(1600f, 44f));
        materials.color = UIStyle.Muted;
        for (int i = 0; i < MaxCards; i++)
        {
            int index = i;
            buttons[i] = UIStyle.Button(window, "", UIStyle.Body, new Vector2((i - (MaxCards - 1) / 2f) * CardSpacing, -5f), new Vector2(CardWidth, CardHeight), () => source.Buy(index), out var unused);
            unused.gameObject.SetActive(false);
            cards[i] = buttons[i].gameObject;
            icons[i] = UIStyle.Icon(buttons[i].transform, new Vector2(0f, 145f), IconSize);
            titles[i] = UIStyle.Bold(UIStyle.Label(buttons[i].transform, "", UIStyle.Body, new Vector2(0f, 70f), new Vector2(CardWidth - 30f, 70f)));
            titles[i].color = UIStyle.Gold;
            descriptions[i] = UIStyle.Label(buttons[i].transform, "", UIStyle.Body, new Vector2(0f, -40f), new Vector2(CardWidth - 30f, 140f));
            costs[i] = UIStyle.Label(buttons[i].transform, "", UIStyle.Body, new Vector2(0f, -155f), new Vector2(CardWidth - 30f, 60f));
        }
        empty = UIStyle.Label(window, "Nada mais a ensinar", UIStyle.Large, new Vector2(0f, -10f), new Vector2(1600f, 60f));
        empty.color = UIStyle.Muted;
        message = UIStyle.Label(window, "", UIStyle.Body, new Vector2(0f, -235f), new Vector2(1600f, 44f));
        UIStyle.Button(window, "Fechar  [Esc]", UIStyle.Body, new Vector2(0f, -310f), new Vector2(360f, 64f), source.Close, out _);
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
        materials.text = bag ? $"Madeira {bag.Wood}   •   Pedra {bag.Stone}   •   compre com 1 a 5 ou clique" : "";
        message.text = source.Message;
        message.color = source.Message.StartsWith("Comprado") ? UIStyle.Good : UIStyle.Bad;
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
            UIStyle.SetIcon(icons[i], offer.Icon);
            costs[i].text = BuildSpot.Cost(offer.Wood, offer.Stone);
            costs[i].color = bag && bag.CanAfford(offer.Wood, offer.Stone) ? UIStyle.Good : UIStyle.Bad;
        }
        if (!wasOpen && EventSystem.current && offers.Count > 0) EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        wasOpen = true;
        int option = GameInput.OptionPressed();
        if (option >= 0) source.Buy(option);
    }
}
