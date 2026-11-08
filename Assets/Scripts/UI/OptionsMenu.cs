using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Tela de opções: volume de música e efeitos (salvos entre sessões) e controles.
// Abre por cima do menu principal ou da pausa; Esc / P ou "Voltar" fecham.
public class OptionsMenu : MonoBehaviour
{
    const string Controls =
        "Mover: WASD ou setas  •  analógico\n" +
        "Dash: Shift\n" +
        "Coletar (segurar): E  •  botão oeste\n" +
        "Construir, reparar, resgatar, abrir baú e estação: F  •  botão norte\n" +
        "Escolher opção: 1 a 5 ou clique\n" +
        "Pausar: Esc ou P";

    static OptionsMenu current;
    static int closedFrame = -1;

    public static bool IsOpen => current;
    // No frame em que fecha, o mesmo Esc não pode também despausar o jogo.
    public static bool BlocksInput => current || closedFrame == Time.frameCount;

    Action onClose;
    Text musicValue, sfxValue;

    public static void Open(Action onClose = null)
    {
        if (current) return;
        current = new GameObject("OptionsMenu").AddComponent<OptionsMenu>();
        current.onClose = onClose;
        current.Build();
        AudioManager.Play(Sfx.Select, 0.5f);
    }

    public static void Close()
    {
        if (!current) return;
        GameSettings.Save();
        closedFrame = Time.frameCount;
        var menu = current;
        current = null;
        Destroy(menu.gameObject);
        menu.onClose?.Invoke();
    }

    void Update()
    {
        if (GameInput.PausePressed()) Close();
    }

    void OnDestroy()
    {
        if (current == this) current = null;
    }

    void Build()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 300;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        var shade = Box(transform, Vector2.zero, Vector2.zero, new Color(0.03f, 0.02f, 0.01f, 0.85f));
        shade.rectTransform.anchorMin = Vector2.zero;
        shade.rectTransform.anchorMax = Vector2.one;
        shade.raycastTarget = true;
        var window = Box(shade.transform, Vector2.zero, new Vector2(1100, 800), UIStyle.PanelColor);

        Label(window.transform, "OPÇÕES", new Vector2(0, 330), new Vector2(1000, 80), 60).color = UIStyle.Gold;
        var music = VolumeRow(window.transform, "Música", 220, GameSettings.MusicVolume, v => GameSettings.MusicVolume = v, out musicValue);
        VolumeRow(window.transform, "Efeitos", 140, GameSettings.SfxVolume, v => { GameSettings.SfxVolume = v; AudioManager.Play(Sfx.Pickup, 0.6f); }, out sfxValue);
        Label(window.transform, "CONTROLES", new Vector2(0, 50), new Vector2(1000, 60), 40).color = UIStyle.Gold;
        var controls = Label(window.transform, Controls, new Vector2(0, -120), new Vector2(1000, 270), 28);
        controls.lineSpacing = 1.15f;

        var back = Box(window.transform, new Vector2(0, -330), new Vector2(360, 70), UIStyle.ButtonColor);
        back.raycastTarget = true;
        var button = back.gameObject.AddComponent<Button>();
        button.targetGraphic = back;
        button.onClick.AddListener(Close);
        Label(back.transform, "Voltar  [Esc]", Vector2.zero, new Vector2(340, 64), 34);
        Refresh();

        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(music.gameObject);
    }

    Slider VolumeRow(Transform parent, string name, float y, float value, Action<float> onChange, out Text valueText)
    {
        var label = Label(parent, name, new Vector2(-330, y), new Vector2(260, 60), 36);
        label.alignment = TextAnchor.MiddleRight;
        valueText = Label(parent, "", new Vector2(390, y), new Vector2(140, 60), 34);
        var slider = MakeSlider(parent, new Vector2(40, y), value);
        slider.onValueChanged.AddListener(v => { onChange(v); Refresh(); });
        return slider;
    }

    void Refresh()
    {
        musicValue.text = $"{Mathf.RoundToInt(GameSettings.MusicVolume * 100)}%";
        sfxValue.text = $"{Mathf.RoundToInt(GameSettings.SfxVolume * 100)}%";
    }

    static Slider MakeSlider(Transform parent, Vector2 position, float value)
    {
        var track = Box(parent, position, new Vector2(500, 26), new Color(0.08f, 0.05f, 0.03f));
        track.raycastTarget = true;
        var fillArea = Rect(track.transform, "Fill Area");
        var fill = Box(fillArea, Vector2.zero, Vector2.zero, new Color(0.85f, 0.6f, 0.25f));
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);
        var handleArea = Rect(track.transform, "Handle Area");
        handleArea.offsetMin = new Vector2(12, 0);
        handleArea.offsetMax = new Vector2(-12, 0);
        var handle = Box(handleArea, Vector2.zero, new Vector2(26, 26), UIStyle.Gold);
        handle.rectTransform.anchorMin = new Vector2(0f, 0f);
        handle.rectTransform.anchorMax = new Vector2(0f, 1f);
        handle.rectTransform.sizeDelta = new Vector2(26, 18);
        var slider = track.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = value;
        return slider;
    }

    static RectTransform Rect(Transform parent, string name)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        return rect;
    }

    static Image Box(Transform parent, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject("Box", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.one * 0.5f;
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = size;
        return image;
    }

    static Text Label(Transform parent, string content, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = UIStyle.Font;
        text.text = content;
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
