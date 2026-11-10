using UnityEngine;
using UnityEngine.UI;

// Painel de materiais (madeira, pedra, chaves e ouro) e a caixa de coleta, que só aparece
// perto de uma árvore ou rocha.
public class ResourceHUD : MonoBehaviour
{
    const float PromptWidth = 620f;
    const float PanelWidth = 520f, PanelHeight = 68f;
    const float IconSize = 28f;

    public DayResourceSystem source;
    Text wood, stone, keys, gold, promptText;
    Image progress;
    GameObject canvasObject, prompt;
    PlayerWallet wallet;

    void Start()
    {
        var canvas = UIStyle.Canvas(transform, "ResourceHUD", -1); // Os menus continuam por cima.
        canvasObject = canvas.gameObject;

        // Painel 520×68 em (20,-20) no canto superior esquerdo, sem título.
        var panel = UIStyle.Panel(canvas, "Materiais", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(PanelWidth, PanelHeight));

        var woodSprite = Resources.Load<Sprite>("Hud/madeira");
        if (!woodSprite) Debug.LogError("ResourceHUD: Sprite 'Hud/madeira' não encontrado!");
        var stoneSprite = Resources.Load<Sprite>("Hud/pedra");
        if (!stoneSprite) Debug.LogError("ResourceHUD: Sprite 'Hud/pedra' não encontrado!");
        var keySprite = Resources.Load<Sprite>("Pickups/chave");
        if (!keySprite) Debug.LogError("ResourceHUD: Sprite 'Pickups/chave' não encontrado!");
        var goldSprite = Resources.Load<Sprite>("Pickups/moeda");
        if (!goldSprite) Debug.LogError("ResourceHUD: Sprite 'Pickups/moeda' não encontrado!");

        wood = CreateSlot(panel, 0, woodSprite);
        stone = CreateSlot(panel, 1, stoneSprite);
        keys = CreateSlot(panel, 2, keySprite);
        gold = CreateSlot(panel, 3, goldSprite);

        var box = UIStyle.Panel(canvas, "Coleta", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(PromptWidth, 76f));
        prompt = box.parent.gameObject;
        promptText = UIStyle.Label(box, "", UIStyle.Large, Vector2.zero, new Vector2(PromptWidth - 30f, 50f));
        promptText.rectTransform.anchorMin = promptText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        promptText.rectTransform.anchoredPosition = new Vector2(0f, 4f);
        var track = UIStyle.Image(box, "Barra", new Color(0.05f, 0.03f, 0.02f));
        track.rectTransform.anchorMin = new Vector2(0f, 0f);
        track.rectTransform.anchorMax = new Vector2(1f, 0f);
        track.rectTransform.offsetMin = new Vector2(10f, 6f);
        track.rectTransform.offsetMax = new Vector2(-10f, 14f);
        progress = UIStyle.Image(track.rectTransform, "Progresso", UIStyle.Good);
        progress.rectTransform.anchorMin = Vector2.zero;
        progress.rectTransform.anchorMax = new Vector2(0f, 1f);
        progress.rectTransform.offsetMin = progress.rectTransform.offsetMax = Vector2.zero;
    }

    Text CreateSlot(RectTransform panel, int index, Sprite sprite)
    {
        float startX = 10f + 124f * index;

        var icon = UIStyle.Image(panel, "Icone", Color.white);
        icon.sprite = sprite;
        icon.preserveAspect = true;
        var iRect = icon.rectTransform;
        iRect.anchorMin = iRect.anchorMax = new Vector2(0f, 0.5f);
        iRect.pivot = new Vector2(0f, 0.5f);
        iRect.anchoredPosition = new Vector2(startX, 0f);
        iRect.sizeDelta = new Vector2(IconSize, IconSize);

        var text = UIStyle.Label(panel, "", UIStyle.Body, TextAnchor.MiddleLeft, Color.white);
        var tRect = text.rectTransform;
        tRect.anchorMin = tRect.anchorMax = new Vector2(0f, 0.5f);
        tRect.pivot = new Vector2(0f, 0.5f);
        tRect.anchoredPosition = new Vector2(startX + 36f, 0f);
        tRect.sizeDelta = new Vector2(80f, 40f);
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 24;
        text.resizeTextMaxSize = 32;

        return text;
    }

    void Update()
    {
        if (!source || !source.Inventory || !GameManager.Instance) return;
        bool show = GameManager.Instance.State == GameState.Playing;
        canvasObject.SetActive(show);
        if (!show) return;
        if (!wallet && GameManager.Instance.Player) wallet = GameManager.Instance.Player.GetComponent<PlayerWallet>();
        var bag = source.Inventory;
        wood.text = bag.Wood.ToString();
        stone.text = bag.Stone.ToString();
        keys.text = bag.Keys.ToString();
        gold.text = (wallet ? wallet.Gold : 0).ToString();

        bool collecting = !string.IsNullOrEmpty(source.Prompt);
        prompt.SetActive(collecting);
        if (!collecting) return;
        promptText.text = source.Prompt;
        progress.rectTransform.anchorMax = new Vector2(source.Progress, 1f);
    }
}
