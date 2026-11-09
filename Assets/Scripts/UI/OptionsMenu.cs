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
        "Construir, reparar, resgatar e abrir: F  •  botão norte\n" +
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
        var canvas = UIStyle.Canvas(transform, "Opcoes", 300, true);
        var shade = UIStyle.Image(canvas, "Veu", UIStyle.Veil);
        UIStyle.Stretch(shade.rectTransform);
        shade.raycastTarget = true;
        var window = UIStyle.Panel(shade.transform, "Janela", Vector2.one * 0.5f, Vector2.one * 0.5f, Vector2.zero, new Vector2(1100, 880));

        Label(window, "OPÇÕES", new Vector2(0, 370), new Vector2(1000, 80), 60).color = UIStyle.Gold;
        var music = VolumeRow(window, "Música", 260, GameSettings.MusicVolume, v => GameSettings.MusicVolume = v, out musicValue);
        VolumeRow(window, "Efeitos", 180, GameSettings.SfxVolume, v => { GameSettings.SfxVolume = v; AudioManager.Play(Sfx.Pickup, 0.6f); }, out sfxValue);
        Label(window, "CONTROLES", new Vector2(0, 90), new Vector2(1000, 60), 40).color = UIStyle.Gold;
        var controls = Label(window, Controls, new Vector2(0, -75), new Vector2(1040, 260), 32);
        controls.lineSpacing = 0.95f;

        Text tutorialLabel = null;
        MakeButton(window, new Vector2(0, -265), new Vector2(760, 60), "", 30, out tutorialLabel).onClick.AddListener(() =>
        {
            // Liga ou desliga os dois juntos: a introdução e o tutorial da primeira run.
            bool seen = FirstRunGuide;
            Tutorial.Seen = seen;
            Intro.Seen = seen;
            tutorialLabel.text = TutorialText();
        });
        tutorialLabel.text = TutorialText();
        MakeButton(window, new Vector2(0, -365), new Vector2(360, 70), "Voltar  [Esc]", 40, out _).onClick.AddListener(Close);
        Refresh();

        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(music.gameObject);
    }

    static bool FirstRunGuide => !Tutorial.Seen || !Intro.Seen;
    static string TutorialText() => FirstRunGuide ? "Introdução e tutorial na próxima run: sim" : "Introdução e tutorial na próxima run: não";

    static Button MakeButton(Transform parent, Vector2 position, Vector2 size, string content, int fontSize, out Text label) =>
        UIStyle.Button(parent, content, fontSize, position, size, null, out label);

    Slider VolumeRow(Transform parent, string name, float y, float value, Action<float> onChange, out Text valueText)
    {
        var label = Label(parent, name, new Vector2(-380, y), new Vector2(220, 60), 40);
        label.alignment = TextAnchor.MiddleRight;
        valueText = Label(parent, "", new Vector2(390, y), new Vector2(140, 60), 40);
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

    static Text Label(Transform parent, string content, Vector2 position, Vector2 size, int fontSize) =>
        UIStyle.Label(parent, content, fontSize, position, size);
}
