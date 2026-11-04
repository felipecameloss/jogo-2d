using UnityEngine;

// Pequenos sprites pixelados gerados uma vez por run, sem dependências externas.
public static class ResourceArt
{
    public static Sprite Create(ResourceKind kind)
    {
        const int size = 32;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - 15.5f;
            Color color = Color.clear;
            if (dx * dx / 150f + (y - 4) * (y - 4) / 9f < 1) color = new Color(0.05f, 0.1f, 0.1f, 0.3f);
            if (kind == ResourceKind.Wood)
            {
                if (x >= 13 && x <= 18 && y >= 4 && y <= 18) color = x < 16 ? new Color(0.35f, 0.18f, 0.1f) : new Color(0.57f, 0.32f, 0.15f);
                bool crown = dx * dx + (y - 21) * (y - 21) < 105 || (x - 9) * (x - 9) + (y - 17) * (y - 17) < 40;
                if (crown) color = x + y > 37 ? new Color(0.42f, 0.7f, 0.25f) : new Color(0.17f, 0.43f, 0.22f);
                if (crown && (x * 7 + y * 3) % 29 == 0) color = new Color(0.65f, 0.82f, 0.31f);
            }
            else if (kind == ResourceKind.Stone)
            {
                if (y >= 5 && y <= 22 && Mathf.Abs(dx) < 12 - Mathf.Max(0, y - 12) * 0.6f)
                    color = x + y > 31 ? new Color(0.66f, 0.72f, 0.77f) : new Color(0.37f, 0.45f, 0.53f);
                if (y >= 8 && y <= 18 && x == 12 + y / 4) color = new Color(0.25f, 0.31f, 0.4f);
            }
            else
            {
                if (y >= 5 && y <= 29 && Mathf.Abs(dx) < Mathf.Min((y - 3) * 0.6f, (32 - y) * 0.5f))
                    color = x < 16 ? new Color(0.13f, 0.6f, 0.77f) : new Color(0.51f, 0.95f, 1f);
                if (y >= 5 && y <= 18 && Mathf.Abs(x - 7) < Mathf.Min((y - 3) * 0.5f, (20 - y) * 0.5f)) color = new Color(0.38f, 0.63f, 0.98f);
                if (y >= 5 && y <= 21 && Mathf.Abs(x - 24) < Mathf.Min((y - 3) * 0.4f, (23 - y) * 0.5f)) color = new Color(0.65f, 0.49f, 0.95f);
            }
            pixels[y * size + x] = color;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.35f), 20f);
    }
}
