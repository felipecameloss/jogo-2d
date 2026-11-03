using UnityEngine;

// Chão "infinito": um SpriteRenderer em modo Tiled que acompanha a câmera
// andando em passos do tamanho do tile, então o padrão nunca "escorrega".
[RequireComponent(typeof(SpriteRenderer))]
public class InfiniteBackground : MonoBehaviour
{
    public Transform follow;
    public float tileSize = 2f;

    void LateUpdate()
    {
        if (!follow && Camera.main) follow = Camera.main.transform;
        if (!follow) return;
        Vector3 p = follow.position;
        transform.position = new Vector3(Mathf.Round(p.x / tileSize) * tileSize, Mathf.Round(p.y / tileSize) * tileSize, 0f);
    }
}
