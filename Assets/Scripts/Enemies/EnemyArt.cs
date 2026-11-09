using System.Collections.Generic;
using UnityEngine;

// Arte animada dos inimigos e chefes, em Assets/Art/Resources/Enemies/<nome>.png.
// Cada folha é uma tira de quadros quadrados olhando para a direita (o Enemy vira com flipX),
// importada como sprite único e fatiada aqui, como em CampArt. Sem o arquivo, devolve null
// e o inimigo continua com o sprite e a cor do prefab.
public static class EnemyArt
{
    public const float PixelsPerUnit = 24f;

    struct Info
    {
        public float bodyCenter; // altura do centro do corpo, em px a partir da base do quadro
        public float frameTime;  // segundos por quadro
        public Info(float bodyCenter, float frameTime) { this.bodyCenter = bodyCenter; this.frameTime = frameTime; }
    }

    // O pivô fica no centro do corpo, como nas formas antigas: colisor, mira das armas,
    // tiros dos chefes e números de dano continuam no mesmo lugar. A sombra fica abaixo dele.
    static readonly Dictionary<string, Info> Sheets = new Dictionary<string, Info>
    {
        { "slime", new Info(12f, 0.1f) },
        { "bat", new Info(19f, 0.06f) },
        { "golem", new Info(23f, 0.12f) },
        { "golem_anciao", new Info(29f, 0.14f) },
        { "rei_da_noite", new Info(40f, 0.12f) },
    };

    static readonly Dictionary<string, Sprite[]> Cache = new Dictionary<string, Sprite[]>();

    public static Sprite[] Frames(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        // null guardado = arquivo que não existe; quadro destruído (troca de cena) = fatiar de novo.
        if (Cache.TryGetValue(name, out var cached) && (cached == null || cached[0])) return cached;

        var sheet = Resources.Load<Sprite>("Enemies/" + name);
        if (!sheet)
        {
            Cache[name] = null;
            return null;
        }
        var texture = sheet.texture;
        int size = texture.height;
        float center = Sheets.TryGetValue(name, out var info) ? info.bodyCenter : size * 0.5f;
        var pivot = new Vector2(0.5f, center / size);
        var frames = new Sprite[Mathf.Max(1, texture.width / size)];
        for (int i = 0; i < frames.Length; i++)
            frames[i] = Sprite.Create(texture, new Rect(i * size, 0, size, size), pivot, PixelsPerUnit, 0, SpriteMeshType.FullRect);
        Cache[name] = frames;
        return frames;
    }

    public static float FrameTime(string name) =>
        name != null && Sheets.TryGetValue(name, out var info) ? info.frameTime : 0.1f;
}
