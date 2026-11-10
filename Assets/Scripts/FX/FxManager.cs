using System.Collections.Generic;
using UnityEngine;

// Efeitos visuais simples: partículas, números de dano e tremida de câmera.
// Os objetos de efeito são reaproveitados (pool) em vez de criados e destruídos a cada golpe,
// o que evita travadas de Garbage Collector quando a noite enche a tela de inimigos.
public class FxManager : MonoBehaviour
{
    public static FxManager Instance { get; private set; }

    public Sprite particleSprite;
    public Font popupFont;

    [Header("Limites (desempenho)")]
    [Tooltip("Máximo de números de dano na tela ao mesmo tempo. Acima disso, novos números são ignorados.")]
    public int maxPopups = 60;
    [Tooltip("Máximo de partículas na tela ao mesmo tempo.")]
    public int maxParticles = 300;

    readonly Stack<FxParticle> freeParticles = new Stack<FxParticle>();
    readonly Stack<FloatingText> freePopups = new Stack<FloatingText>();
    readonly Stack<FxFade> freeFlashes = new Stack<FxFade>();
    int activeParticles, activePopups;

    void Awake()
    {
        Instance = this;
        if (!popupFont || popupFont.name == "LegacyRuntime") popupFont = UIStyle.Font;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Burst(Vector3 position, Color color, int count = 8, float speed = 4f, float size = 0.18f)
    {
        var fx = Instance;
        if (!fx || !fx.particleSprite) return;
        count = Mathf.Min(count, fx.maxParticles - fx.activeParticles);
        for (int i = 0; i < count; i++)
        {
            FxParticle p;
            if (fx.freeParticles.Count > 0) p = fx.freeParticles.Pop();
            else
            {
                var go = new GameObject("FxParticle");
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = fx.particleSprite;
                sr.sortingOrder = 50;
                p = go.AddComponent<FxParticle>();
            }
            p.transform.position = position;
            p.Play(color, Vector3.one * size * Random.Range(0.6f, 1.2f),
                Random.insideUnitCircle.normalized * speed * Random.Range(0.4f, 1f), Random.Range(0.25f, 0.5f));
            fx.activeParticles++;
        }
    }

    public static void Popup(Vector3 position, string text, Color color, float scale = 1f)
    {
        var fx = Instance;
        if (!fx || !fx.popupFont || fx.activePopups >= fx.maxPopups) return;

        FloatingText ft;
        if (fx.freePopups.Count > 0) ft = fx.freePopups.Pop();
        else
        {
            var go = new GameObject("Popup");
            var tm = go.AddComponent<TextMesh>();
            tm.font = fx.popupFont;
            tm.fontSize = 32; // múltiplo de 16: grid da fonte pixelada
            tm.characterSize = 0.09f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = fx.popupFont.material;
            mr.sortingOrder = 100;
            ft = go.AddComponent<FloatingText>();
        }
        ft.transform.position = position + (Vector3)(Random.insideUnitCircle * 0.15f);
        ft.transform.localScale = Vector3.one * scale;
        ft.Play(text, color);
        fx.activePopups++;
    }

    // Sprite temporário que some aos poucos (raios, cortes, explosões, sombras do dash).
    // Se sprite for null, usa o quadrado das partículas.
    public static SpriteRenderer Flash(Sprite sprite, Vector3 position, Vector2 startScale, Vector2 endScale, Color color,
        float duration, float rotation = 0f, int sortingOrder = 40)
    {
        if (!sprite && Instance) sprite = Instance.particleSprite;
        if (!sprite) return null;
        var fade = SpawnFlash(sprite, position, color, rotation, sortingOrder);
        fade.Init(startScale, endScale, duration);
        return fade.Renderer;
    }

    // Tira de quadros tocada uma vez, sem mudar de escala (explosão, golpe da espada).
    // Escala negativa em x espelha em volta do pivô do sprite.
    public static SpriteRenderer Animate(Sprite[] frames, Vector3 position, Vector2 scale, float duration,
        float rotation = 0f, int sortingOrder = 40)
    {
        if (frames == null || frames.Length == 0 || !frames[0]) return null;
        var fade = SpawnFlash(frames[0], position, Color.white, rotation, sortingOrder);
        fade.Init(scale, scale, duration, frames);
        return fade.Renderer;
    }

    static FxFade SpawnFlash(Sprite sprite, Vector3 position, Color color, float rotation, int sortingOrder)
    {
        var fx = Instance;
        FxFade fade;
        if (fx && fx.freeFlashes.Count > 0) fade = fx.freeFlashes.Pop();
        else
        {
            var go = new GameObject("FxFlash");
            go.AddComponent<SpriteRenderer>();
            fade = go.AddComponent<FxFade>();
        }
        fade.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, rotation));
        var sr = fade.Renderer;
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        sr.flipX = false;
        sr.flipY = false;
        fade.gameObject.SetActive(true);
        return fade;
    }

    // Chamados pelos próprios efeitos quando terminam.
    public static void Release(FxParticle p)
    {
        if (!Instance) { Destroy(p.gameObject); return; }
        p.gameObject.SetActive(false);
        Instance.freeParticles.Push(p);
        Instance.activeParticles--;
    }

    public static void Release(FloatingText ft)
    {
        if (!Instance) { Destroy(ft.gameObject); return; }
        ft.gameObject.SetActive(false);
        Instance.freePopups.Push(ft);
        Instance.activePopups--;
    }

    public static void Release(FxFade fade)
    {
        if (!Instance) { Destroy(fade.gameObject); return; }
        fade.gameObject.SetActive(false);
        Instance.freeFlashes.Push(fade);
    }

    public static void Shake(float intensity, float duration)
    {
        if (CameraFollow.Instance) CameraFollow.Instance.Shake(intensity, duration);
    }
}
