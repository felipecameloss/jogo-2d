using UnityEngine;

// Partícula reaproveitável (ver FxManager). Quando acaba, volta para o pool.
public class FxParticle : MonoBehaviour
{
    public Vector2 velocity;
    public float lifetime = 0.4f;

    SpriteRenderer sr;
    Vector3 startScale;
    Color color;
    float age;

    public void Play(Color c, Vector3 scale, Vector2 vel, float life)
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        color = c;
        sr.color = c;
        startScale = scale;
        transform.localScale = scale;
        velocity = vel;
        lifetime = life;
        age = 0f;
        gameObject.SetActive(true);
    }

    void Update()
    {
        age += Time.deltaTime;
        float k = age / lifetime;
        if (k >= 1f)
        {
            FxManager.Release(this);
            return;
        }
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity *= 1f - 4f * Time.deltaTime;
        transform.localScale = startScale * (1f - k);
        var c = color;
        c.a = color.a * (1f - k);
        sr.color = c;
    }
}
