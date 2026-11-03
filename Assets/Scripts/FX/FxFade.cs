using UnityEngine;

// Faz um sprite mudar de escala e sumir ao longo do tempo, depois se destrói.
[RequireComponent(typeof(SpriteRenderer))]
public class FxFade : MonoBehaviour
{
    Vector3 startScale, endScale;
    float duration, age, startAlpha;
    SpriteRenderer sr;

    public void Init(Vector2 from, Vector2 to, float time)
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = new Vector3(from.x, from.y, 1f);
        endScale = new Vector3(to.x, to.y, 1f);
        duration = Mathf.Max(0.01f, time);
        startAlpha = sr.color.a;
        transform.localScale = startScale;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / duration);
        transform.localScale = Vector3.Lerp(startScale, endScale, 1f - (1f - t) * (1f - t));
        var c = sr.color;
        c.a = startAlpha * (1f - t);
        sr.color = c;
        if (t >= 1f) Destroy(gameObject);
    }
}
