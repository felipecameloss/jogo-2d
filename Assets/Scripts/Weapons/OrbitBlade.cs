using System.Collections.Generic;
using UnityEngine;

public class OrbitBlade : MonoBehaviour
{
    [HideInInspector] public OrbitWeapon owner;

    readonly Dictionary<Enemy, float> nextHitTime = new Dictionary<Enemy, float>();

    void OnTriggerStay2D(Collider2D other)
    {
        if (!owner || !other.TryGetComponent(out Enemy enemy) || enemy.IsDead) return;
        if (nextHitTime.TryGetValue(enemy, out float t) && Time.time < t) return;

        if (nextHitTime.Count > 300) nextHitTime.Clear(); // evita acumular inimigos já mortos
        nextHitTime[enemy] = Time.time + owner.hitInterval;

        Vector2 away = (enemy.transform.position - owner.transform.position).normalized;
        enemy.TakeDamage(owner.BladeDamage, away * owner.knockback);
    }
}
