using UnityEngine;

// Projétil disparado pelos chefes. Não usa física: checa a distância até o jogador
// (barato e suficiente para algumas dezenas de tiros na tela).
public class EnemyBullet : MonoBehaviour
{
    const float PlayerRadius = 0.35f;

    Vector2 velocity;
    float damage, radius, lifetime, age;
    Transform player;
    PlayerStats stats;

    public static void Fire(Vector2 position, Vector2 velocity, float damage, Color color, float size = 0.35f, float lifetime = 6f)
    {
        var fx = FxManager.Instance;
        if (!fx || !fx.particleSprite) return;
        var gm = GameManager.Instance;
        if (!gm || !gm.Player) return;

        var go = new GameObject("BossBullet");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * size;
        go.transform.rotation = Quaternion.Euler(0f, 0f, 45f); // quadrado vira losango
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = fx.particleSprite;
        sr.color = color;
        sr.sortingOrder = 85; // acima da escuridão da noite, para dar pra desviar

        var b = go.AddComponent<EnemyBullet>();
        b.velocity = velocity;
        b.damage = damage;
        b.radius = size * 0.5f;
        b.lifetime = lifetime;
        b.stats = gm.Player;
        b.player = gm.Player.transform;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime || !player || (GameManager.Instance && GameManager.Instance.State == GameState.GameOver))
        {
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, 360f * Time.deltaTime);

        float r = radius + PlayerRadius;
        if (((Vector2)(player.position - transform.position)).sqrMagnitude <= r * r)
        {
            stats.TakeDamage(damage); // respeita invencibilidade e dash
            Destroy(gameObject);
        }
    }
}
