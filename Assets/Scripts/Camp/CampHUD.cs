using UnityEngine;
using UnityEngine.UI;

// Vida da fogueira (logo abaixo do relógio) e aviso de construção.
// A direção das pessoas perdidas fica nas missões (QuestLog).
public class CampHUD : MonoBehaviour
{
    const float BarWidth = 492f, LowHealth = 0.3f;
    static readonly Color FireColor = new Color(1f, 0.55f, 0.15f);
    static readonly Color AlertColor = new Color(1f, 0.2f, 0.15f);

    public CampSystem source;
    GameObject root;
    Text label;
    Image fill;
    PromptBox prompt;

    void Start()
    {
        var canvas = UIStyle.Canvas(transform, "CampHUD", -1); // Os menus continuam por cima.
        root = canvas.gameObject;

        var panel = UIStyle.Panel(canvas, "Fogueira", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -154f), new Vector2(520f, 90f));
        label = UIStyle.Label(panel, "", UIStyle.Body, TextAnchor.UpperCenter, Color.white);
        UIStyle.Stretch(label.rectTransform, 4f);
        var track = UIStyle.Image(panel, "Barra", new Color(0.05f, 0.03f, 0.02f));
        track.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        track.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        track.rectTransform.pivot = new Vector2(0.5f, 0f);
        track.rectTransform.anchoredPosition = new Vector2(0f, 10f);
        track.rectTransform.sizeDelta = new Vector2(BarWidth - 20f, 26f);
        fill = UIStyle.Image(track.rectTransform, "Vida", FireColor);
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(1f, 1f);
        fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;

        prompt = new PromptBox(canvas, 190f);
    }

    void Update()
    {
        if (!source || !GameManager.Instance || !root) return;
        bool show = GameManager.Instance.State == GameState.Playing;
        root.SetActive(show);
        if (!show) return;
        prompt.Set(source.Prompt);
        var heart = source.Heart;
        float ratio = heart && heart.MaxHealth > 0f ? heart.Health / heart.MaxHealth : 0f;
        fill.rectTransform.anchorMax = new Vector2(ratio, 1f);
        bool low = ratio <= LowHealth;
        fill.color = low && Mathf.Sin(Time.time * 10f) > 0f ? AlertColor : FireColor;
        label.text = low ? "A FOGUEIRA ESTÁ SE APAGANDO!" : $"Fogueira  {Mathf.CeilToInt(heart ? heart.Health : 0f)}/{Mathf.CeilToInt(heart ? heart.MaxHealth : 0f)}";
        label.color = low ? UIStyle.Bad : Color.white;
    }
}
