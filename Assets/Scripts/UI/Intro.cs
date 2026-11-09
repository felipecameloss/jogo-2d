using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Introdução da primeira run: seis cartões contam a história. Cada um tem uma cena montada com a
// própria arte do jogo (Bento, ferreiros, fogueira, monstros, árvores) e o texto surgindo letra a
// letra. O jogo fica parado pelo GameManager (estado Menu), então o primeiro dia só começa depois.
// Espaço, clique ou A avançam; Enter, Esc ou Select pulam. Aparece uma vez; as opções reativam
// junto com o tutorial.
public class Intro : MonoBehaviour
{
    const string SeenKey = "intro_vista";
    const int Px = 4;                      // unidades de UI por pixel de arte: nítido em 1080p e em 540p
    const int StageW = 272, StageH = 112;  // tamanho da cena, em pixels de arte
    const float LettersPerSecond = 55f, FadeTime = 0.35f;

    public static bool Seen
    {
        get => PlayerPrefs.GetInt(SeenKey, 0) == 1;
        set { PlayerPrefs.SetInt(SeenKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    struct Card
    {
        public string Text;
        public Action Build;
    }

    // Sprite animado da cena, em tempo real (o jogo está com timeScale 0).
    class Anim
    {
        public Image Image;
        public Sprite[] Frames;
        public int First, Count;
        public float FrameTime, Time;
    }

    static readonly Color Night = new Color(0.62f, 0.66f, 0.86f);  // personagens e árvores no escuro
    static readonly Color FireGlow = new Color(1f, 0.62f, 0.3f);

    // Escurece sem mexer na transparência (Color * float multiplicaria o alfa também).
    static Color Dim(Color color, float k) => new Color(color.r * k, color.g * k, color.b * k, 1f);

    Card[] cards;
    Transform canvas;
    RectTransform content, stage;
    Text body, hint;
    Image fade;
    int index;
    float letters, hold;
    bool busy, finishing;
    readonly List<Anim> anims = new List<Anim>();
    readonly List<(Image image, float alpha)> glows = new List<(Image, float)>();
    readonly List<Object> owned = new List<Object>(); // texturas e sprites criados aqui
    Sprite[] player, woman, fire, tree, rocks;

    void Start()
    {
        var gm = GameManager.Instance;
        if (!gm || !gm.EnterMenu(Skip))
        {
            Destroy(this);
            return;
        }
        LoadArt();
        BuildLayout();
        cards = new[]
        {
            new Card { Text = "O vale de Alvorada vivia em paz, e as fogueiras da aldeia ardiam até o amanhecer.", Build = Village },
            new Card { Text = "Até que, sob as ruínas do vale, o Rei da Noite despertou.", Build = King },
            new Card { Text = "Sua maldição deu vida à escuridão: o lodo dos pântanos, os morcegos das cavernas, as pedras antigas. Ao amanhecer, tudo se desfaz, mas toda noite eles voltam.", Build = Monsters },
            new Card { Text = "A aldeia caiu em uma só noite. Quem escapou se perdeu na floresta.", Build = Forest },
            new Card { Text = "Bento, um jovem lenhador, salvou a última brasa da forja e acendeu uma fogueira. Só o fogo resiste à maldição, e é atrás dele que os monstros virão.", Build = LastEmber },
            new Card { Text = "De dia, colete, construa e resgate os perdidos. À noite, defenda a fogueira.", Build = TitleCard },
        };
        ShowCard(0);
        StartCoroutine(FadeTo(0f)); // começa no preto
    }

    void OnDestroy()
    {
        foreach (var o in owned)
            if (o) Destroy(o);
    }

    void LoadArt()
    {
        var gm = GameManager.Instance;
        var animator = gm && gm.Player ? gm.Player.GetComponent<CharacterAnimator>() : null;
        player = animator ? animator.frames : null;
        woman = CampSystem.LoadFrames("Characters/character_women") ?? player;
        fire = CampSystem.FireFrames();
        var day = FindAnyObjectByType<DayResourceSystem>();
        if (day && day.treePrefab)
        {
            var loop = day.treePrefab.GetComponent<SpriteLoop>();
            var sr = day.treePrefab.GetComponentInChildren<SpriteRenderer>();
            tree = loop && loop.frames != null && loop.frames.Length > 0 ? loop.frames : sr && sr.sprite ? new[] { sr.sprite } : null;
        }
        rocks = day ? day.rockSprites : null;
    }

    // ---------------------------------------------------------------- fluxo

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        foreach (var a in anims)
        {
            a.Time += dt;
            a.Image.sprite = a.Frames[a.First + (int)(a.Time / a.FrameTime) % a.Count];
        }
        float t = Time.unscaledTime;
        float flicker = 0.85f + 0.1f * Mathf.Sin(t * 9f) + 0.05f * Mathf.Sin(t * 23f);
        foreach (var (image, alpha) in glows)
        {
            var c = image.color;
            c.a = alpha * flicker;
            image.color = c;
        }

        if (finishing || busy || cards == null) return;
        if (GameInput.SkipTutorialPressed()) { Skip(); return; }

        string text = cards[index].Text;
        bool advance = GameInput.AdvancePressed();
        if (letters < text.Length)
        {
            // Avançar com o texto pela metade só completa o texto.
            letters = advance ? text.Length : Mathf.Min(text.Length, letters + LettersPerSecond * dt);
            body.text = Typed(text, (int)letters);
            return;
        }
        hold += dt;
        if (advance || hold >= 2f + text.Length * 0.03f) StartCoroutine(Next());
    }

    // O resto do texto fica invisível, mas ocupa espaço: as palavras não pulam de linha enquanto surgem.
    static string Typed(string text, int count) =>
        count >= text.Length ? text : text.Substring(0, count) + "<color=#00000000>" + text.Substring(count) + "</color>";

    IEnumerator Next()
    {
        busy = true;
        AudioManager.Play(Sfx.Select, 0.5f);
        yield return FadeTo(1f);
        if (index + 1 >= cards.Length)
        {
            yield return End();
            yield break;
        }
        ShowCard(index + 1);
        yield return FadeTo(0f);
        busy = false;
    }

    // Enter, Select ou Esc (o GameManager chama isto ao apertar Esc no estado Menu).
    void Skip()
    {
        if (finishing) return;
        StopAllCoroutines();
        StartCoroutine(End());
    }

    // Escurece, tira a cena, devolve o jogo e clareia por cima dele.
    IEnumerator End()
    {
        finishing = true;
        Seen = true;
        yield return FadeTo(1f);
        Destroy(content.gameObject);
        anims.Clear();
        glows.Clear();
        if (GameManager.Instance) GameManager.Instance.ExitMenu();
        yield return FadeTo(0f);
        Destroy(canvas.gameObject);
        Destroy(this);
    }

    IEnumerator FadeTo(float target)
    {
        float start = fade.color.a;
        for (float t = 0f; t < FadeTime; t += Time.unscaledDeltaTime)
        {
            SetFade(Mathf.Lerp(start, target, t / FadeTime));
            yield return null;
        }
        SetFade(target);
    }

    void SetFade(float alpha) => fade.color = new Color(0f, 0f, 0f, alpha);

    void ShowCard(int i)
    {
        index = i;
        letters = 0f;
        hold = 0f;
        body.text = "";
        anims.Clear();
        glows.Clear();
        for (int c = stage.childCount - 1; c >= 0; c--) Destroy(stage.GetChild(c).gameObject);
        cards[i].Build();
        bool last = i == cards.Length - 1;
        hint.text = $"{i + 1}/{cards.Length}     Espaço ou clique: {(last ? "começar" : "continuar")}     Enter: pular";
    }

    // ---------------------------------------------------------------- tela

    void BuildLayout()
    {
        canvas = UIStyle.Canvas(transform, "Introducao", 200);
        content = new GameObject("Conteudo", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(canvas, false);
        UIStyle.Stretch(content);
        UIStyle.Stretch(UIStyle.Image(content, "Fundo", new Color(0.025f, 0.02f, 0.04f)).rectTransform);

        // Quadro da cena: miolo de StageW x StageH pixels de arte.
        var size = new Vector2(StageW * Px + 2 * UIStyle.Border, StageH * Px + 2 * UIStyle.Border);
        stage = UIStyle.Panel(content, "Quadro", Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, 120f), size);
        stage.gameObject.AddComponent<RectMask2D>();

        var story = UIStyle.Panel(content, "Historia", Vector2.one * 0.5f, Vector2.one * 0.5f, new Vector2(0f, -262f), new Vector2(1400f, 196f));
        body = UIStyle.Label(story, "", UIStyle.Body, TextAnchor.MiddleCenter, Color.white);
        UIStyle.Stretch(body.rectTransform, 24f);

        hint = UIStyle.Label(content, "", UIStyle.Body, new Vector2(0f, -420f), new Vector2(1600f, 50f));
        hint.color = UIStyle.Muted;

        fade = UIStyle.Image(canvas, "Fade", Color.black);
        UIStyle.Stretch(fade.rectTransform);
    }

    // ---------------------------------------------------------------- cenas

    void Village()
    {
        const int ground = 22;
        Sky(new Color32(58, 44, 84, 255), new Color32(222, 140, 92, 255));
        Glow(214, ground + 2, 34, new Color(1f, 0.78f, 0.45f), 0.6f); // sol se pondo
        Hills(ground, 20, new Color32(108, 68, 82, 255), 1);
        Tree(18, ground, new Color(0.9f, 0.78f, 0.75f));
        Tree(252, ground, new Color(0.9f, 0.78f, 0.75f));
        Ground(ground, new Color32(70, 98, 58, 255), new Color32(98, 134, 74, 255));
        Actor(woman, 0, 8, 0.13f, 152, ground + 3);                                  // ferreira, ao fundo
        Glow(136, ground + 6, 22, FireGlow, 1f);
        Actor(fire, 0, 99, 0.1f, 136, ground - 6);
        Actor(player, 24, 8, 0.13f, 106, ground - 6);                                // Bento
        Actor(player, 16, 8, 0.13f, 168, ground - 6, CampSystem.SmithTint, 0.4f);  // ferreiro
    }

    void King()
    {
        const int ground = 20;
        Sky(new Color32(8, 6, 20, 255), new Color32(52, 30, 74, 255));
        Stars(2, 22, 40);
        Moon(206, 80, 17);
        Rock(0, 58, ground - 2);
        Rock(4, 86, ground - 1);
        Rock(2, 218, ground - 2);
        Ground(ground, new Color32(30, 30, 42, 255), new Color32(48, 46, 64, 255));
        Glow(136, ground + 34, 36, new Color(0.62f, 0.38f, 0.9f), 0.8f);
        Actor(EnemyArt.Frames("rei_da_noite"), 0, 8, EnemyArt.FrameTime("rei_da_noite"), 136, ground - 8);
    }

    void Monsters()
    {
        const int ground = 22;
        Sky(new Color32(10, 12, 28, 255), new Color32(30, 40, 62, 255));
        Stars(3, 18, 56);
        Ground(ground, new Color32(34, 46, 40, 255), new Color32(52, 68, 54, 255));
        Monster("golem_anciao", 236, ground - 4, 0.3f);
        Monster("golem", 60, ground - 7, 0f);
        Monster("slime", 112, ground - 7, 0.25f);
        Monster("slime", 142, ground - 6, 0.6f);
        Monster("bat", 180, ground - 7, 0.1f);
        Monster("bat", 204, ground - 3, 0.35f);
    }

    void Forest()
    {
        const int ground = 20;
        Sky(new Color32(6, 8, 18, 255), new Color32(22, 28, 46, 255));
        Stars(4, 14, 70);
        Moon(40, 92, 9);
        Hills(ground, 24, new Color32(18, 24, 38, 255), 4);
        Tree(30, ground + 2, Dim(Night, 0.7f));
        Tree(196, ground + 3, Dim(Night, 0.7f));
        Ground(ground, new Color32(26, 36, 34, 255), new Color32(38, 52, 46, 255));
        Actor(player, 16, 8, 0.13f, 70, ground - 6, CampSystem.SmithTint * Night);  // ferreiro, perdido
        Tree(136, ground - 2, Dim(Night, 0.85f));
        Actor(woman, 24, 8, 0.13f, 214, ground - 6, Night, 0.5f);                   // ferreira, perdida
        Tree(262, ground - 3, Dim(Night, 0.85f));
    }

    void LastEmber()
    {
        const int ground = 22;
        Sky(new Color32(8, 8, 22, 255), new Color32(32, 26, 54, 255));
        Stars(5, 20, 56);
        Hills(ground, 20, new Color32(22, 22, 42, 255), 5);
        Tree(22, ground, Dim(Night, 0.75f));
        Tree(252, ground, Dim(Night, 0.75f));
        Ground(ground, new Color32(34, 44, 40, 255), new Color32(52, 64, 54, 255));
        Glow(150, ground + 10, 44, FireGlow, 0.9f);
        Actor(fire, 0, 99, 0.1f, 150, ground - 6);
        Actor(player, 24, 8, 0.13f, 122, ground - 6, new Color(1f, 0.92f, 0.84f));
    }

    void TitleCard()
    {
        const int ground = 22;
        Sky(new Color32(8, 8, 22, 255), new Color32(40, 28, 58, 255));
        Stars(6, 26, 40);
        Moon(238, 92, 10);
        Ground(ground, new Color32(34, 44, 40, 255), new Color32(52, 64, 54, 255));
        Glow(136, ground + 10, 46, FireGlow, 0.9f);
        Actor(fire, 0, 99, 0.1f, 136, ground - 6);
        Actor(player, 24, 8, 0.13f, 110, ground - 6, new Color(1f, 0.92f, 0.84f));

        // Nome em 64 px ampliado 2x: a fonte pixelada fica nítida em múltiplos de 16.
        var title = UIStyle.Bold(UIStyle.Label(stage, GameInfo.Title.ToUpperInvariant(), UIStyle.Title, TextAnchor.MiddleCenter, UIStyle.Gold));
        var rect = title.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -20f);
        rect.sizeDelta = new Vector2(540f, 90f);
        rect.localScale = new Vector3(2f, 2f, 1f);
    }

    // ---------------------------------------------------------------- peças da cena
    // Coordenadas em pixels de arte, a partir do canto de baixo à esquerda da cena.

    Image Place(Image image, float x, float y, float w, float h, Vector2 pivot)
    {
        var rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = Vector2.zero;
        rect.pivot = pivot;
        rect.anchoredPosition = new Vector2(Mathf.Round(x) * Px, Mathf.Round(y) * Px);
        rect.sizeDelta = new Vector2(w * Px, h * Px);
        return image;
    }

    // Personagem ou objeto animado, com a base do quadro em y e centrado em x.
    Image Actor(Sprite[] frames, int first, int count, float frameTime, float x, float y, Color? tint = null, float phase = 0f)
    {
        if (frames == null || frames.Length == 0) return null;
        if (first >= frames.Length) first = 0;
        count = Mathf.Clamp(count, 1, frames.Length - first);
        var sprite = frames[first];
        var image = UIStyle.Image(stage, "Ator", tint ?? Color.white);
        image.sprite = sprite;
        Place(image, x, y, sprite.rect.width, sprite.rect.height, new Vector2(0.5f, 0f));
        if (count > 1)
            anims.Add(new Anim { Image = image, Frames = frames, First = first, Count = count, FrameTime = Mathf.Max(0.02f, frameTime), Time = phase });
        return image;
    }

    void Monster(string sheet, float x, float y, float phase) =>
        Actor(EnemyArt.Frames(sheet), 0, 8, EnemyArt.FrameTime(sheet), x, y, null, phase);

    void Tree(float x, float ground, Color tint) => Actor(tree, 0, 99, 0.18f, x, ground - 12, tint);

    void Rock(int which, float x, float y)
    {
        if (rocks == null || rocks.Length == 0) return;
        Actor(new[] { rocks[which % rocks.Length] }, 0, 1, 1f, x, y, new Color(0.55f, 0.55f, 0.72f));
    }

    void Sky(Color top, Color bottom)
    {
        var image = UIStyle.Image(stage, "Ceu", Color.white);
        image.sprite = Own(Gradient(top, bottom, 14));
        UIStyle.Stretch(image.rectTransform);
    }

    void Ground(int height, Color color, Color edge)
    {
        Place(UIStyle.Image(stage, "Chao", color), 0, 0, StageW, height, Vector2.zero);
        Place(UIStyle.Image(stage, "Borda", edge), 0, height - 1, StageW, 1, Vector2.zero);
    }

    // Morros no horizonte: dão profundidade e recebem a sombra das árvores, que no céu viraria uma mancha.
    void Hills(int baseY, int height, Color color, int seed)
    {
        var rng = new System.Random(seed);
        float p1 = (float)rng.NextDouble() * 6.28f, p2 = (float)rng.NextDouble() * 6.28f;
        var texture = NewTexture(StageW, height);
        var clear = new Color(0f, 0f, 0f, 0f);
        for (int x = 0; x < StageW; x++)
        {
            float top = height * (0.6f + 0.22f * Mathf.Sin(x * 0.035f + p1) + 0.16f * Mathf.Sin(x * 0.11f + p2));
            for (int y = 0; y < height; y++) texture.SetPixel(x, y, y < top ? color : clear);
        }
        var image = UIStyle.Image(stage, "Morros", Color.white);
        image.sprite = Own(ToSprite(texture));
        Place(image, 0, baseY, StageW, height, Vector2.zero);
    }

    void Stars(int seed, int count, int minY)
    {
        var rng = new System.Random(seed);
        for (int i = 0; i < count; i++)
        {
            var color = new Color(0.9f, 0.9f, 1f, 0.35f + 0.65f * (float)rng.NextDouble());
            Place(UIStyle.Image(stage, "Estrela", color), rng.Next(2, StageW - 2), rng.Next(minY, StageH - 2), 1, 1, Vector2.zero);
        }
    }

    void Moon(int x, int y, int radius)
    {
        var image = UIStyle.Image(stage, "Lua", Color.white);
        image.sprite = Own(MoonSprite(radius));
        Place(image, x, y, radius * 2, radius * 2, Vector2.one * 0.5f);
    }

    // Halo em anéis de pixel, tremulando como fogo.
    void Glow(float x, float y, int radius, Color color, float alpha)
    {
        var image = UIStyle.Image(stage, "Brilho", color);
        image.sprite = Own(GlowSprite(radius));
        Place(image, x, y, radius * 2, radius * 2, Vector2.one * 0.5f);
        glows.Add((image, alpha));
    }

    // ---------------------------------------------------------------- texturas geradas

    Sprite Own(Sprite sprite)
    {
        owned.Add(sprite.texture);
        owned.Add(sprite);
        return sprite;
    }

    static Texture2D NewTexture(int w, int h) =>
        new Texture2D(w, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };

    static Sprite ToSprite(Texture2D texture)
    {
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f, 100f);
    }

    // Céu em faixas, como nos jogos de pixel art.
    static Sprite Gradient(Color top, Color bottom, int steps)
    {
        var texture = NewTexture(1, steps);
        for (int y = 0; y < steps; y++) texture.SetPixel(0, y, Color.Lerp(bottom, top, y / (steps - 1f)));
        return ToSprite(texture);
    }

    static Sprite MoonSprite(int radius)
    {
        int size = radius * 2;
        var texture = NewTexture(size, size);
        var light = new Color32(232, 230, 242, 255);
        var shade = new Color32(186, 184, 212, 255);
        var crater = new Color32(204, 202, 226, 255);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x + 0.5f - radius, dy = y + 0.5f - radius;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            Color c = new Color(0f, 0f, 0f, 0f);
            if (d <= radius)
            {
                c = dx + dy * 0.3f > radius * 0.35f ? (Color)shade : light;
                float c1 = Vector2.Distance(new Vector2(dx, dy), new Vector2(-radius * 0.3f, radius * 0.2f));
                float c2 = Vector2.Distance(new Vector2(dx, dy), new Vector2(radius * 0.15f, -radius * 0.35f));
                if (c1 < radius * 0.2f || c2 < radius * 0.14f) c = crater;
            }
            texture.SetPixel(x, y, c);
        }
        return ToSprite(texture);
    }

    static Sprite GlowSprite(int radius)
    {
        int size = radius * 2;
        var texture = NewTexture(size, size);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x + 0.5f - radius, dy = y + 0.5f - radius;
            float t = Mathf.Sqrt(dx * dx + dy * dy) / radius;
            float a = t < 0.3f ? 0.45f : t < 0.55f ? 0.3f : t < 0.8f ? 0.17f : t < 1f ? 0.07f : 0f;
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, a));
        }
        return ToSprite(texture);
    }
}
