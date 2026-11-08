using UnityEngine;
using UnityEngine.UI;

// Painel de materiais (madeira, pedra, chaves e ouro) e a caixa de coleta, que só aparece
// perto de uma árvore ou rocha.
public class ResourceHUD : MonoBehaviour
{
    const float PromptWidth = 620f;

    public DayResourceSystem source;
    Text left, right, promptText;
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
        left = UIStyle.Label(panel, "", UIStyle.Body, TextAnchor.LowerLeft, Color.white);
        UIStyle.Stretch(left.rectTransform, UIStyle.Padding);
        right = UIStyle.Label(panel, "", UIStyle.Body, TextAnchor.LowerRight, Color.white);
        UIStyle.Stretch(right.rectTransform, UIStyle.Padding);

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
        left.text = $"<color=#A6DD77>Madeira</color>  {bag.Wood}\n<color=#C9D4ED>Pedra</color>  {bag.Stone}";
        right.text = $"<color=#FFD45E>Chaves</color>  {bag.Keys}\n<color=#FFD45E>Ouro</color>  {(wallet ? wallet.Gold : 0)}";

        bool collecting = !string.IsNullOrEmpty(source.Prompt);
        prompt.SetActive(collecting);
        if (!collecting) return;
        promptText.text = source.Prompt;
        progress.rectTransform.anchorMax = new Vector2(source.Progress, 1f);
    }
}
