using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 2.5f;

    Vector2 direction;
    float speed, damage, knockback, age;
    int pierceLeft;
    readonly HashSet<Enemy> alreadyHit = new HashSet<Enemy>();

    public void Launch(Vector2 dir, float spd, float dmg, int pierce, float sizeMultiplier, float knock)
    {
        direction = dir.normalized;
        speed = spd;
        damage = dmg;
        pierceLeft = pierce;
        knockback = knock;
        transform.localScale *= sizeMultiplier;
        transform.right = direction;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        age += Time.deltaTime;
        if (age >= lifetime) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Enemy enemy) || enemy.IsDead || alreadyHit.Contains(enemy)) return;

        alreadyHit.Add(enemy);
        enemy.TakeDamage(damage, direction * knockback);
        if (--pierceLeft < 0) Destroy(gameObject);
    }
}
