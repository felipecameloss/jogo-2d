using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Identidade visual da interface: fonte medieval (MedievalSharp, licença OFL) e cores de botão.
public static class UIStyle
{
    public static readonly Color ButtonColor = new Color(0.36f, 0.25f, 0.15f);
    public static readonly Color PanelColor = new Color(0.16f, 0.11f, 0.07f);
    public static readonly Color Gold = new Color(1f, 0.83f, 0.45f);

    static Font font;

    public static Font Font
    {
        get
        {
            if (!font) font = Resources.Load<Font>("Fonts/MedievalSharp");
            if (!font) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font;
        }
    }

    // As cenas geradas usam a fonte padrão do Unity: troca em todos os textos já existentes.
    public static void ApplyToScene()
    {
        foreach (var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            text.font = Font;
    }

    // Novo botão numa coluna de menu já existente, copiando o visual de outro botão.
    public static Button CloneButton(GameObject template, string label, int siblingIndex, UnityAction action)
    {
        var copy = Object.Instantiate(template, template.transform.parent);
        copy.name = label;
        copy.SetActive(true);
        copy.transform.SetSiblingIndex(siblingIndex);
        var text = copy.GetComponentInChildren<Text>(true);
        if (text) text.text = label;
        var image = copy.GetComponent<Image>();
        if (image) image.color = ButtonColor;
        var button = copy.GetComponent<Button>();
        button.onClick = new Button.ButtonClickedEvent(); // sem os ouvintes do original
        button.onClick.AddListener(action);
        return button;
    }
}
