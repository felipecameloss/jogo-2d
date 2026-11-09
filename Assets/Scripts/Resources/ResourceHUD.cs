using UnityEngine;
using UnityEngine.UI;

// Painel de materiais (madeira, pedra, chaves e ouro) e a caixa de coleta, que só aparece
// perto de uma árvore ou rocha.
public class ResourceHUD : MonoBehaviour
{
    const float PromptWidth = 620f;
    const float RowHeight = 38f, IconSize = 32f; // ícones de 16 px, em 2x

    public DayResourceSystem source;
    Text wood, stone, keys, gold, promptText;
    Image progress;
    GameObject canvasObject, prompt;
    PlayerWallet wallet;

    void Start()
    {
        var canvas = UIStyle.Canvas(transform, "ResourceHUD", -1); // Os menus continuam por cima.
        canvasObject = canvas.gameObject;

        // Logo abaixo do painel de nível.
        var panel = UIStyle.Panel(canvas, "Materiais", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -154f), new Vector2(UIStyle.ColumnWidth, 150f));
        var title = UIStyle.Bold(UIStyle.Label(panel, "MATERIAIS", UIStyle.Body, TextAnchor.UpperLeft, UIStyle.Gold));
        UIStyle.Stretch(title.rectTransform, UIStyle.Padding);
        wood = Row(panel, 1, TextAnchor.MiddleLeft, "Hud/madeira");
        stone = Row(panel, 0, TextAnchor.MiddleLeft, "Hud/pedra");
        keys = Row(panel, 1, TextAnchor.MiddleRight, null);
        gold = Row(panel, 0, TextAnchor.MiddleRight, null);

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

    void Update()
    {
        if (!source || !source.Inventory || !GameManager.Instance) return;
        bool show = GameManager.Instance.State == GameState.Playing;
        canvasObject.SetActive(show);
        if (!show) return;
        if (!wallet && GameManager.Instance.Player) wallet = GameManager.Instance.Player.GetComponent<PlayerWallet>();
        var bag = source.Inventory;
        wood.text = $"<color=#A6DD77>Madeira</color>  {bag.Wood}";
        stone.text = $"<color=#C9D4ED>Pedra</color>  {bag.Stone}";
        keys.text = $"<color=#FFD45E>Chaves</color>  {bag.Keys}";
        gold.text = $"<color=#FFD45E>Ouro</color>  {(wallet ? wallet.Gold : 0)}";

        bool collecting = !string.IsNullOrEmpty(source.Prompt);
        prompt.SetActive(collecting);
        if (!collecting) return;
        promptText.text = source.Prompt;
        progress.rectTransform.anchorMax = new Vector2(source.Progress, 1f);
    }

    // Uma linha do painel, contada de baixo (0) para cima, com ícone opcional (de Resources) à esquerda.
    static Text Row(RectTransform panel, int row, TextAnchor align, string icon)
    {
        float y = UIStyle.Padding + row * RowHeight;
        float left = UIStyle.Padding;
        var sprite = icon != null ? Resources.Load<Sprite>(icon) : null;
        if (sprite)
        {
            var image = UIStyle.Image(panel, "Icone", Color.white);
            image.sprite = sprite;
            image.preserveAspect = true;
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(left, y + (RowHeight - IconSize) * 0.5f);
            rect.sizeDelta = Vector2.one * IconSize;
            left += IconSize + 8f;
        }
        var text = UIStyle.Label(panel, "", UIStyle.Body, align, Color.white);
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = new Vector2(1f, 0f);
        textRect.offsetMin = new Vector2(left, y);
        textRect.offsetMax = new Vector2(-UIStyle.Padding, y + RowHeight);
        return text;
    }
}
