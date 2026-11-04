using UnityEngine;

public static class TradeArt
{
    public static Sprite Create(bool merchant, int tier)
    {
        const int size = 40;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color[size * size];
        Color gold = tier == 0 ? new Color(0.83f, 0.58f, 0.25f) : tier == 1 ? new Color(0.65f, 0.85f, 0.95f) : new Color(0.78f, 0.5f, 1f);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            Color c = Color.clear;
            if ((x - 20) * (x - 20) / 250f + (y - 5) * (y - 5) / 12f < 1) c = new Color(0, 0, 0, 0.3f);
            if (merchant)
            {
                if ((x == 6 || x == 33) && y >= 5 && y <= 29) c = new Color(0.4f, 0.23f, 0.12f);
                if (x >= 14 && x <= 25 && y >= 14 && y <= 21) c = new Color(0.95f, 0.7f, 0.43f);
                if (x >= 13 && x <= 26 && y >= 21 && y <= 24) c = new Color(0.24f, 0.24f, 0.5f);
                if ((x == 17 || x == 23) && y == 18) c = new Color(0.1f, 0.08f, 0.12f);
                if (x >= 5 && x <= 34 && y >= 7 && y <= 13) c = y == 13 ? new Color(0.73f, 0.48f, 0.24f) : new Color(0.4f, 0.25f, 0.17f);
                if (x >= 3 && x <= 36 && y >= 27 && y <= 34 - Mathf.Abs(x - 20) / 5)
                    c = (x / 6) % 2 == 0 ? new Color(0.27f, 0.66f, 0.58f) : new Color(0.95f, 0.85f, 0.6f);
            }
            else
            {
                if (x >= 7 && x <= 32 && y >= 6 && y <= 22) c = new Color(0.42f, 0.23f, 0.13f);
                if (x >= 9 && x <= 30 && y > 22 && y <= 27) c = new Color(0.62f, 0.36f, 0.18f);
                if (x >= 7 && x <= 32 && y == 17) c = gold;
                if ((x == 11 || x == 28) && y >= 6 && y <= 26) c = gold;
                if (x >= 18 && x <= 22 && y >= 14 && y <= 20) c = gold;
                if (x == 20 && y >= 16 && y <= 18) c = Color.black;
                if (tier == 2 && (Mathf.Abs(x - 20) + Mathf.Abs(y - 33) <= 2)) c = gold;
            }
            pixels[y * size + x] = c;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.3f), 20);
    }
}
