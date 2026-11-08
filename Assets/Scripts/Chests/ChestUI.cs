using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Aviso de interação do baú e a janela "escolha 1 de 3".
// A direção do baú mais próximo fica nas missões (QuestLog).
public class ChestUI : MonoBehaviour
{
    const float CardWidth = 360f, CardHeight = 260f, CardSpacing = 390f;

    public ChestSystem source;
    GameObject modal, hud;
    Text heading;
    readonly Text[] titles = new Text[3], descriptions = new Text[3];
    readonly Button[] buttons = new Button[3];
    PromptBox nearby;
    bool wasOpen;

    void Start()
    {
        var hudCanvas = UIStyle.Canvas(transform, "ChestHUD", -1);
        hud = hudCanvas.gameObject;
        nearby = new PromptBox(hudCanvas, 190f);

        var canvas = UIStyle.Canvas(transform, "ChestUI", 150, true);
        var veil = UIStyle.Image(canvas, "Veu", UIStyle.Veil);
        UIStyle.Stretch(veil.rectTransform);
        veil.raycastTarget = true;
        modal = veil.gameObject;
        var window = UIStyle.Panel(modal.transform, "Janela", Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero, new Vector2(1240f, 600f));
        heading = UIStyle.Label(window, "", UIStyle.Title, new Vector2(0f, 230f), new Vector2(1180f, 70f));
        heading.color = UIStyle.Gold;
        var hint = UIStyle.Label(window, "Escolha UMA recompensa (1, 2, 3 ou clique). A chave só é gasta ao escolher.", UIStyle.Body, new Vector2(0f, 170f), new Vector2(1180f, 44f));
        hint.color = UIStyle.Muted;
        for (int i = 0; i < 3; i++)
        {
            int index = i;
            buttons[i] = UIStyle.Button(window, "", UIStyle.Body, new Vector2((i - 1) * CardSpacing, -20f), new Vector2(CardWidth, CardHeight), () => source.Choose(index), out var unused);
            unused.gameObject.SetActive(false);
            titles[i] = UIStyle.Label(buttons[i].transform, "", UIStyle.Large, new Vector2(0f, 80f), new Vector2(CardWidth - 30f, 70f));
            titles[i].color = UIStyle.Gold;
            descriptions[i] = UIStyle.Label(buttons[i].transform, "", UIStyle.Body, new Vector2(0f, -30f), new Vector2(CardWidth - 40f, 140f));
        }
        UIStyle.Button(window, "Fechar sem abrir  [Esc]", UIStyle.Body, new Vector2(0f, -240f), new Vector2(440f, 60f), source.Close, out _);
        modal.SetActive(false);
    }

    void Update()
    {
        if (!source || !GameManager.Instance || !modal) return;
        bool open = source.ShowOffers;
        modal.SetActive(open);
        bool playing = GameManager.Instance.State == GameState.Playing;
        hud.SetActive(playing);
        if (playing) nearby.Set(source.Prompt);
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
}
