using System.Collections.Generic;
using UnityEngine;

// Vai, desacelera e volta para o dono. Acerta cada inimigo uma vez na ida e uma na volta.
public class Boomerang : MonoBehaviour
{
    Transform owner;
    Vector2 direction;
    float speed, outTime, damage, radius, knockback, age;
    bool returning;
    readonly HashSet<Enemy> hit = new HashSet<Enemy>();

    public void Launch(Transform owner, Vector2 dir, float speed, float outTime, float damage, float size, float knockback)
    {
        this.owner = owner;
        direction = dir.normalized;
        this.speed = speed;
        this.outTime = outTime;
        this.damage = damage;
        this.knockback = knockback;
        radius = size * 0.5f;
        transform.localScale = Vector3.one * size;
    }

    void Update()
    {
        if (!owner) { Destroy(gameObject); return; }

        age += Time.deltaTime;
        transform.Rotate(0f, 0f, 900f * Time.deltaTime);

        if (!returning)
        {
            float t = age / outTime;
            transform.position += (Vector3)(direction * speed * Mathf.Lerp(1f, 0.15f, t) * Time.deltaTime);
            if (t >= 1f)
            {
                returning = true;
                hit.Clear();
            }
        }
        else
        {
            Vector3 toOwner = owner.position - transform.position;
            float step = speed * Mathf.Min(1.6f, 0.4f + (age - outTime) * 2f) * Time.deltaTime;
            if (toOwner.magnitude <= Mathf.Max(step, 0.3f) || age > 6f) { Destroy(gameObject); return; }
            transform.position += toOwner.normalized * step;
        }

        HitEnemies();
    }

    void HitEnemies()
    {
        float r = radius + 0.3f; // + raio aproximado do inimigo
        float r2 = r * r;
        Vector2 pos = transform.position;
        for (int i = Enemy.Active.Count - 1; i >= 0; i--)
        {
            if (i >= Enemy.Active.Count) continue;
            var e = Enemy.Active[i];
            if (e.IsDead || hit.Contains(e)) continue;
            Vector2 offset = (Vector2)e.transform.position - pos;
            if (offset.sqrMagnitude > r2) continue;
            hit.Add(e);
            e.TakeDamage(damage, offset.normalized * knockback);
        }
    }
}
