using UnityEngine;

// Quadros do baú e sprite da chave.
public static class ChestArt
{
    const float ChestPixelsPerUnit = 24f;
    static readonly Vector2 ChestPivot = new Vector2(0.5f, 0.1f); // Base do baú.
    static Sprite[] chestFrames;

    // Quadros de Assets/Art/Resources/Chests/chest.png. Ficam em cache porque a textura é do
    // projeto e não deve ser destruída no fim da run.
    public static Sprite[] ChestFrames()
    {
        if (chestFrames != null && chestFrames[0]) return chestFrames;
        var sheet = Resources.Load<Sprite>("Chests/chest");
        chestFrames = CampArt.Slice(sheet.texture, ChestPivot, ChestPixelsPerUnit);
        return chestFrames;
    }

    // A mesma arte para os três baús; o tom diferencia os mais fortes.
    public static Color Tint(int tier) =>
        tier == 0 ? Color.white : tier == 1 ? new Color(0.75f, 0.9f, 1f) : new Color(0.85f, 0.7f, 1f);

    // A chave de Assets/Art/Resources/Pickups/chave.png. A 20 px por unidade (não 24) fica do
    // tamanho da chave antiga. Em cache: a textura é do projeto e não pode ser destruída.
    // Sem o arquivo, gera a chave antiga; aí generated = true e quem chamou destrói a textura.
    const float KeyPixelsPerUnit = 20f;
    static Sprite keyArt;

    public static Sprite Key(out bool generated)
    {
        generated = false;
        if (keyArt) return keyArt;
        var art = Resources.Load<Sprite>("Pickups/chave");
        if (art)
        {
            keyArt = Sprite.Create(art.texture, art.rect, new Vector2(0.5f, 0.5f), KeyPixelsPerUnit, 0, SpriteMeshType.FullRect);
            return keyArt;
        }
        generated = true;
        return GeneratedKey();
    }

    static Sprite GeneratedKey()
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
