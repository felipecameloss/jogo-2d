using UnityEngine;

// Faz um sprite mudar de escala e sumir ao longo do tempo, depois volta para o pool (ver FxManager).
// Com quadros, toca a tira uma vez na escala fixa: os quadros já trazem o próprio sumiço.
[RequireComponent(typeof(SpriteRenderer))]
public class FxFade : MonoBehaviour
{
    Vector3 startScale, endScale;
    float duration, age, startAlpha;
    Sprite[] frames;
    SpriteRenderer sr;

    public SpriteRenderer Renderer => sr ? sr : (sr = GetComponent<SpriteRenderer>());

    public void Init(Vector2 from, Vector2 to, float time, Sprite[] animation = null)
    {
        sr = Renderer;
        startScale = new Vector3(from.x, from.y, 1f);
        endScale = new Vector3(to.x, to.y, 1f);
        duration = Mathf.Max(0.01f, time);
        startAlpha = sr.color.a;
        frames = animation != null && animation.Length > 0 ? animation : null;
        age = 0f;
        transform.localScale = startScale;
        if (frames != null) sr.sprite = frames[0];
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / duration);
        if (frames != null)
            sr.sprite = frames[Mathf.Min(frames.Length - 1, (int)(t * frames.Length))];
        else
        {
            transform.localScale = Vector3.Lerp(startScale, endScale, 1f - (1f - t) * (1f - t));
            var c = sr.color;
            c.a = startAlpha * (1f - t);
            sr.color = c;
        }
        if (t >= 1f) FxManager.Release(this);
    }
}
