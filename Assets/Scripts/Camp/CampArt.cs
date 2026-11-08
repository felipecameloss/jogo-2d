using System.Collections.Generic;
using UnityEngine;

// Sprites do acampamento. Procura a arte final em Assets/Art/Resources/Camp/<nome>.png
// e, se não existir, gera um placeholder.
// Cerca e muro: 13 quadros de 48x64 na mesma ordem — 0 horizontal de cima, 1 de baixo, 2 vertical da direita,
// 3 da esquerda, 4-7 diagonais,
// 8 ┘, 9 └, 10 ┐, 11 ┌, 12 poste. Portões: 4 linhas de 6 quadros (abrindo para sul, oeste, norte, leste).
public static class CampArt
{
    const float PixelsPerUnit = 20f;
    public const float ArtPixelsPerUnit = 24f;
    // Passo da grade do anel: cada peça de cerca, muro ou portão ocupa 32 px.
    public const float Cell = 32f / ArtPixelsPerUnit;
    static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Sprite[]> FrameCache = new Dictionary<string, Sprite[]>();

    public static Sprite Get(string name)
    {
        if (Cache.TryGetValue(name, out var cached) && cached) return cached;
        var sprite = Resources.Load<Sprite>("Camp/" + name);
        if (!sprite) sprite = Placeholder(name);
        Cache[name] = sprite;
        return sprite;
    }

    // Arte animada: spritesheet horizontal de quadros quadrados. Sem arquivo, devolve só o placeholder.
    public static Sprite[] Frames(string name, Vector2 pivot, float pixelsPerUnit)
    {
        if (FrameCache.TryGetValue(name, out var cached) && cached[0]) return cached;
        var sheet = Resources.Load<Sprite>("Camp/" + name);
        var frames = sheet ? Slice(sheet.texture, pivot, pixelsPerUnit) : new[] { Get(name) };
        FrameCache[name] = frames;
        return frames;
    }

    // Arte em grade (peças do anel, portões, estações): quadros de largura x altura, lidos linha a linha
    // a partir do topo, a 24 px por unidade. Sem arquivo, devolve só o placeholder.
    public static Sprite[] Sheet(string name, int width, int height, Vector2 pivot)
    {
        string key = name + "#sheet";
        if (FrameCache.TryGetValue(key, out var cached) && cached[0]) return cached;
        var sheet = Resources.Load<Sprite>("Camp/" + name);
        Sprite[] frames;
        if (sheet)
        {
            var texture = sheet.texture;
            int columns = texture.width / width, rows = texture.height / height;
            frames = new Sprite[columns * rows];
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                var rect = new Rect(column * width, texture.height - (row + 1) * height, width, height);
                frames[row * columns + column] = Sprite.Create(texture, rect, pivot, ArtPixelsPerUnit, 0, SpriteMeshType.FullRect);
            }
        }
        else frames = new[] { Get(name) };
        FrameCache[key] = frames;
        return frames;
    }

    // Índice protegido: com o placeholder há um quadro só.
    public static Sprite Pick(Sprite[] frames, int index) => frames[Mathf.Clamp(index, 0, frames.Length - 1)];

    public static Sprite[] Slice(Texture2D texture, Vector2 pivot, float pixelsPerUnit)
    {
        int size = texture.height;
        var frames = new Sprite[Mathf.Max(1, texture.width / size)];
        for (int i = 0; i < frames.Length; i++)
            frames[i] = Sprite.Create(texture, new Rect(i * size, 0, size, size), pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
        return frames;
    }

    static Sprite Placeholder(string name)
    {
        switch (name)
        {
            case "heart": return Fire();
            case "fence": return Bar(new Color(0.55f, 0.36f, 0.18f), new Color(0.36f, 0.22f, 0.1f), 5);
            case "wall": return Bar(new Color(0.58f, 0.6f, 0.65f), new Color(0.36f, 0.38f, 0.44f), 11);
            case "gate_wood": return Bar(new Color(0.72f, 0.5f, 0.24f), new Color(0.3f, 0.18f, 0.08f), 22);
            case "gate_stone": return Bar(new Color(0.58f, 0.6f, 0.65f), new Color(0.3f, 0.18f, 0.08f), 22);
            case "station_weapon_npc": return Station(new Color(0.75f, 0.32f, 0.2f));
            case "station_armor_npc": return Station(new Color(0.35f, 0.5f, 0.75f));
            default: return Mark();
        }
    }

    static Sprite Fire()
    {
        const int size = 32;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float width = Mathf.Max(0f, 12f - y * 0.42f);
            float distance = Mathf.Abs(x - 15.5f);
            Color c = Color.clear;
            if (y >= 7 && distance < width) c = distance < width * 0.45f ? new Color(1f, 0.9f, 0.45f) : new Color(1f, 0.5f, 0.12f);
            if (y < 7 && distance < 13f) c = new Color(0.34f, 0.2f, 0.1f);
            pixels[y * size + x] = c;
        }
        return Create(pixels, size, size, new Vector2(0.5f, 0.2f));
    }

    static Sprite Bar(Color main, Color dark, int stripe)
    {
        const int width = 44, height = 12;
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            pixels[y * width + x] = y == 0 || y == height - 1 || x % stripe == 0 ? dark : main;
        return Create(pixels, width, height, Vector2.one * 0.5f);
    }

    static Sprite Station(Color roof)
    {
        const int width = 36, height = 34;
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
            pixels[y * width + x] = y > 18 ? (x >= 2 && x <= 33 ? roof : Color.clear) : (x >= 5 && x <= 30 ? new Color(0.45f, 0.3f, 0.18f) : Color.clear);
        return Create(pixels, width, height, new Vector2(0.5f, 0.1f));
    }

    static Sprite Mark()
    {
        const int size = 12;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool corner = (x < 3 || x > 8) && (y < 3 || y > 8);
            bool edge = x == 0 || x == size - 1 || y == 0 || y == size - 1;
            pixels[y * size + x] = corner && edge ? new Color(1f, 0.92f, 0.65f, 0.75f) : Color.clear;
        }
        return Create(pixels, size, size, Vector2.one * 0.5f);
    }

    static Sprite Create(Color[] pixels, int width, int height, Vector2 pivot)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), pivot, PixelsPerUnit);
    }
}
