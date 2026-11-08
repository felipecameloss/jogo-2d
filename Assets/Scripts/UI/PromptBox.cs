using UnityEngine;
using UnityEngine.UI;

// Aviso de interação ("[F] Construir cerca..."): painel opaco que acompanha o tamanho do texto
// e some quando não há nada a dizer.
public class PromptBox
{
    const float Height = 64f, MinWidth = 200f;

    readonly RectTransform frame;
    readonly Text text;
    string current;

    public PromptBox(Transform canvas, float y)
    {
        var inner = UIStyle.Panel(canvas, "Aviso", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, y), new Vector2(MinWidth, Height));
        frame = (RectTransform)inner.parent;
        text = UIStyle.Label(inner, "", UIStyle.Body, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.7f));
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        UIStyle.Stretch(text.rectTransform);
        frame.gameObject.SetActive(false);
    }

    public void Set(string value)
    {
        bool show = !string.IsNullOrEmpty(value);
        if (frame.gameObject.activeSelf != show) frame.gameObject.SetActive(show);
        if (!show || value == current) return;
        current = value;
        text.text = value;
        frame.sizeDelta = new Vector2(Mathf.Max(MinWidth, text.preferredWidth + 40f), Height);
    }
}
