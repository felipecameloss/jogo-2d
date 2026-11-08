using System;
using UnityEngine;
using UnityEngine.UI;

// Tutorial do primeiro dia: objetivos curtos, um de cada vez, sem pausar o jogo.
// Termina ao anoitecer, ao cumprir tudo ou com Enter. Depois de concluído ou pulado,
// não aparece mais (dá para reativar nas opções).
public class Tutorial : MonoBehaviour
{
    const string SeenKey = "tutorial_visto";
    const float DoneDelay = 1.2f, NightMessageTime = 6f;

    public static bool Seen
    {
        get => PlayerPrefs.GetInt(SeenKey, 0) == 1;
        set { PlayerPrefs.SetInt(SeenKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    struct Step
    {
        public string Text;
        public Func<bool> Done;
        public Step(string text, Func<bool> done) { Text = text; Done = done; }
    }

    Step[] steps;
    int index;
    float doneTimer = -1f, nightTimer = -1f;
    bool sawWood, sawStone;
    GameObject panel;
    Text title, body;
    ResourceInventory bag;

    void Start()
    {
        var player = GameManager.Instance ? GameManager.Instance.Player : null;
        bag = player ? player.GetComponent<ResourceInventory>() : null;
        steps = new[]
        {
            new Step("Segure <b>E</b> perto de uma árvore e de uma rocha para coletar madeira e pedra.", () => sawWood && sawStone),
            new Step("Volte à fogueira e aperte <b>F</b> numa marca do chão para erguer uma cerca.", HasWall),
            new Step("Há pessoas perdidas pelo mapa. Siga a indicação azul no canto da tela e aperte <b>F</b> para resgatar.", RescuedSomeone),
            new Step("Ache uma chave pelo mapa e abra um baú com <b>F</b>: ele dá armas e equipamentos.", () => ChestSystem.Instance && ChestSystem.Instance.ClosedChests < 3),
        };
        Build();
    }

    static bool HasWall()
    {
        foreach (var structure in Structure.Active)
            if (structure.Kind != StructureKind.Heart && structure.Kind != StructureKind.Station) return true;
        return false;
    }

    static bool RescuedSomeone()
    {
        var camp = CampSystem.Instance;
        if (!camp) return false;
        foreach (var person in camp.Persons)
            if (person && person.State != PersonState.Lost) return true;
        return false;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        bool playing = gm && gm.State == GameState.Playing;
        panel.SetActive(playing);
        if (!playing) return;

        if (GameInput.SkipTutorialPressed()) { Finish(); return; }
        // A bolsa pode ser gasta logo depois: guarda que o jogador já coletou cada material.
        if (bag) { sawWood |= bag.Wood > 0; sawStone |= bag.Stone > 0; }

        var cycle = DayNightCycle.Instance;
        if (cycle && cycle.IsNight)
        {
            if (nightTimer < 0f)
            {
                nightTimer = NightMessageTime;
                title.text = "A NOITE CHEGOU";
                body.text = "Os monstros vão atrás da fogueira. Defenda-a e repare o que eles quebrarem com <b>F</b>.";
            }
            nightTimer -= Time.deltaTime;
            if (nightTimer <= 0f) Finish();
            return;
        }

        if (doneTimer >= 0f)
        {
            doneTimer -= Time.deltaTime;
            if (doneTimer < 0f) Advance();
            return;
        }
        if (steps[index].Done())
        {
            doneTimer = DoneDelay;
            title.text = "OBJETIVO CUMPRIDO!";
            title.color = new Color(0.6f, 1f, 0.5f);
            AudioManager.Play(Sfx.LevelUp, 0.4f);
        }
    }

    void Advance()
    {
        index++;
        if (index >= steps.Length) { Finish(); return; }
        Show();
    }

    void Show()
    {
        title.text = $"OBJETIVO {index + 1}/{steps.Length}";
        title.color = UIStyle.Gold;
        body.text = steps[index].Text;
    }

    void Finish()
    {
        Seen = true;
        Destroy(panel.transform.parent.gameObject);
        Destroy(this);
    }

    void Build()
    {
        var root = new GameObject("TutorialUI", typeof(Canvas), typeof(CanvasScaler));
        root.transform.SetParent(transform, false);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = -1; // Os menus continuam por cima.
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // Abaixo do painel de materiais, no canto esquerdo.
        var box = new GameObject("Objetivo", typeof(RectTransform), typeof(Image));
        box.transform.SetParent(root.transform, false);
        var image = box.GetComponent<Image>();
        image.color = new Color(0.12f, 0.08f, 0.05f, 0.9f);
        image.raycastTarget = false;
        var rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(285f, -370f);
        rect.sizeDelta = new Vector2(540f, 190f);
        panel = box;

        title = Label(box.transform, new Vector2(0f, 65f), new Vector2(500f, 40f), 28);
        body = Label(box.transform, new Vector2(0f, -5f), new Vector2(500f, 100f), 23);
        var skip = Label(box.transform, new Vector2(0f, -72f), new Vector2(500f, 30f), 19);
        skip.text = "[Enter] pular tutorial";
        skip.color = new Color(1f, 1f, 1f, 0.55f);
        Show();
    }

    static Text Label(Transform parent, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.font = UIStyle.Font;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.supportRichText = true;
        text.raycastTarget = false;
        text.rectTransform.anchorMin = text.rectTransform.anchorMax = Vector2.one * 0.5f;
        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = size;
        return text;
    }
}
