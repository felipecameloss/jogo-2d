using UnityEngine;

// Bomba com pavio: pisca cada vez mais rápido e explode.
[RequireComponent(typeof(SpriteRenderer))]
public class Bomb : MonoBehaviour
{
    float fuse, radius, damage, knockback, age;
    Sprite explosionSprite;
    Color color;
    SpriteRenderer sr;

    public void Init(float fuseTime, float radius, float damage, float knockback, Sprite explosionSprite, Color color)
    {
        fuse = fuseTime;
        this.radius = radius;
        this.damage = damage;
        this.knockback = knockback;
        this.explosionSprite = explosionSprite;
        this.color = color;
        sr = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * 0.5f;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = age / fuse;
        bool blink = Mathf.Sin(age * Mathf.Lerp(8f, 40f, t)) > 0f;
        sr.color = blink ? Color.white : color;
        transform.localScale = Vector3.one * (0.5f + t * 0.15f);

        if (age >= fuse) Explode();
    }

    void Explode()
    {
        Enemy.DamageInRadius(transform.position, radius, damage, knockback);
        FxManager.Flash(explosionSprite, transform.position, Vector2.one * radius, Vector2.one * radius * 2.2f, new Color(1f, 0.6f, 0.2f, 0.9f), 0.3f);
        FxManager.Burst(transform.position, color, 16, 7f, 0.18f);
        FxManager.Shake(0.15f, 0.15f);
        AudioManager.Play(Sfx.EnemyDie, 0.8f, 0.15f);
        Destroy(gameObject);
    }
}
