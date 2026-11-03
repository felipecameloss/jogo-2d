using UnityEngine;

// Área no chão que causa dano periódico por um tempo e depois some.
[RequireComponent(typeof(SpriteRenderer))]
public class DamageZone : MonoBehaviour
{
    float radius, damage, duration, tickInterval, age, tick;
    SpriteRenderer sr;
    float baseAlpha;

    public void Init(float radius, float damage, float duration, float tickInterval)
    {
        this.radius = radius;
        this.damage = damage;
        this.duration = duration;
        this.tickInterval = tickInterval;
        sr = GetComponent<SpriteRenderer>();
        baseAlpha = sr.color.a;
        transform.localScale = Vector3.one * radius * 2f;
    }

    void Update()
    {
        age += Time.deltaTime;
        tick -= Time.deltaTime;
        if (tick <= 0f)
        {
            tick = tickInterval;
            Enemy.DamageInRadius(transform.position, radius, damage, 0f);
        }

        // Aparece rápido e some nos últimos 0.4s.
        float alpha = Mathf.Min(age / 0.15f, (duration - age) / 0.4f, 1f);
        var c = sr.color;
        c.a = baseAlpha * Mathf.Clamp01(alpha);
        sr.color = c;

        if (age >= duration) Destroy(gameObject);
    }
}
