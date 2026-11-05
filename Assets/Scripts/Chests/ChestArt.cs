using UnityEngine;

// Sprites pixelados do baú e da chave, gerados uma vez por run.
public static class ChestArt
{
    public static Color TierColor(int tier) =>
        tier == 0 ? new Color(0.83f, 0.58f, 0.25f) : tier == 1 ? new Color(0.65f, 0.85f, 0.95f) : new Color(0.78f, 0.5f, 1f);

    public static Sprite Chest(int tier)
    {
        const int size = 40;
        var texture = NewTexture(size);
        var pixels = new Color[size * size];
        Color trim = TierColor(tier);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Color c = Color.clear;
            if ((x - 20) * (x - 20) / 250f + (y - 5) * (y - 5) / 12f < 1) c = new Color(0, 0, 0, 0.3f);
            if (x >= 7 && x <= 32 && y >= 6 && y <= 22) c = new Color(0.42f, 0.23f, 0.13f);
            if (x >= 9 && x <= 30 && y > 22 && y <= 27) c = new Color(0.62f, 0.36f, 0.18f);
            if (x >= 7 && x <= 32 && y == 17) c = trim;
            if ((x == 11 || x == 28) && y >= 6 && y <= 26) c = trim;
            if (x >= 18 && x <= 22 && y >= 14 && y <= 20) c = trim;
            if (x == 20 && y >= 16 && y <= 18) c = Color.black;
            if (tier == 2 && Mathf.Abs(x - 20) + Mathf.Abs(y - 33) <= 2) c = trim;
            pixels[y * size + x] = c;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.15f), 20);
    }

    public static Sprite Key()
    {
        const int size = 16;
        var texture = NewTexture(size);
        var pixels = new Color[size * size];
        var gold = new Color(1f, 0.82f, 0.25f);
        var dark = new Color(0.62f, 0.42f, 0.1f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Color c = Color.clear;
            float dx = x - 4.5f, dy = y - 10.5f;
            float r = dx * dx + dy * dy;
            if (r < 12f && r > 3f) c = gold;               // argola
            if (y >= 10 && y <= 11 && x >= 7 && x <= 14) c = gold; // haste
            if (x >= 11 && x <= 12 && y >= 7 && y <= 9) c = gold;  // dentes
            if (x >= 13 && x <= 14 && y >= 8 && y <= 9) c = gold;
            if (c == gold && (y == 10 || r > 9f)) c = dark;
            pixels[y * size + x] = c;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 20);
    }

    static Texture2D NewTexture(int size) =>
        new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
}
