using UnityEngine;

public class FxParticle : MonoBehaviour
{
    public Vector2 velocity;
    public float lifetime = 0.4f;

    SpriteRenderer sr;
    Vector3 startScale;
    float age;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
    }

    void Update()
    {
        age += Time.deltaTime;
        float k = age / lifetime;
        if (k >= 1f)
        {
            Destroy(gameObject);
            return;
        }
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity *= 1f - 4f * Time.deltaTime;
        transform.localScale = startScale * (1f - k);
        var c = sr.color;
        c.a = 1f - k;
        sr.color = c;
    }
}
