using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DecorationEntry
{
    public GameObject prefab;
    public float weight = 1f;
}

// Espalha árvores e arbustos pelo mapa infinito.
// O mundo é dividido em blocos (chunks); cada bloco sempre gera as mesmas plantas
// (semente fixa por coordenada), então voltar a um lugar mostra o mesmo cenário.
// Blocos longe da câmera são desativados e os objetos reaproveitados.
public class DecorationScatter : MonoBehaviour
{
    public Transform follow;
    public DecorationEntry[] entries;
    public float chunkSize = 8f;
    [Tooltip("Quantos blocos em volta da câmera ficam ativos.")]
    public int radius = 2;
    public int minPerChunk = 0;
    public int maxPerChunk = 3;
    [Tooltip("Área em volta do ponto inicial sem plantas.")]
    public float clearRadiusAtStart = 3f;
    public int seed = 1234;

    readonly Dictionary<Vector2Int, List<GameObject>> active = new Dictionary<Vector2Int, List<GameObject>>();
    readonly Dictionary<GameObject, Stack<GameObject>> pool = new Dictionary<GameObject, Stack<GameObject>>();
    readonly Dictionary<GameObject, GameObject> prefabOf = new Dictionary<GameObject, GameObject>();
    readonly List<Vector2Int> toRemove = new List<Vector2Int>();
    Vector2Int lastCenter = new Vector2Int(int.MinValue, int.MinValue);
    float totalWeight;

    void Start()
    {
        foreach (var e in entries) if (e.prefab) totalWeight += Mathf.Max(0f, e.weight);
    }

    void LateUpdate()
    {
        if (!follow && Camera.main) follow = Camera.main.transform;
        if (!follow || totalWeight <= 0f) return;

        var center = new Vector2Int(Mathf.FloorToInt(follow.position.x / chunkSize), Mathf.FloorToInt(follow.position.y / chunkSize));
        if (center == lastCenter) return;
        lastCenter = center;

        toRemove.Clear();
        foreach (var key in active.Keys)
            if (Mathf.Abs(key.x - center.x) > radius || Mathf.Abs(key.y - center.y) > radius) toRemove.Add(key);
        foreach (var key in toRemove) Despawn(key);

        for (int x = center.x - radius; x <= center.x + radius; x++)
        for (int y = center.y - radius; y <= center.y + radius; y++)
        {
            var key = new Vector2Int(x, y);
            if (!active.ContainsKey(key)) Spawn(key);
        }
    }

    void Spawn(Vector2Int key)
    {
        var list = new List<GameObject>();
        active[key] = list;
        var rng = new System.Random(unchecked(seed ^ (key.x * 73856093) ^ (key.y * 19349663)));
        int count = rng.Next(minPerChunk, maxPerChunk + 1);
        for (int i = 0; i < count; i++)
        {
            var prefab = Pick((float)rng.NextDouble() * totalWeight);
            var pos = new Vector3((key.x + (float)rng.NextDouble()) * chunkSize, (key.y + (float)rng.NextDouble()) * chunkSize, 0f);
            bool flip = rng.NextDouble() < 0.5;
            if (!prefab || ((Vector2)pos).magnitude < clearRadiusAtStart) continue;
            if (CampSystem.Instance && (CampSystem.Instance.IsReserved(pos, 0f) || CampSystem.Instance.IsNearPerson(pos, 1.5f))) continue;

            var go = Get(prefab);
            go.transform.position = pos;
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr) sr.flipX = flip;
            go.SetActive(true);
            list.Add(go);
        }
    }

    void Despawn(Vector2Int key)
    {
        foreach (var go in active[key])
        {
            go.SetActive(false);
            pool[prefabOf[go]].Push(go);
        }
        active.Remove(key);
    }

    GameObject Pick(float r)
    {
        foreach (var e in entries)
        {
            if (!e.prefab) continue;
            r -= Mathf.Max(0f, e.weight);
            if (r <= 0f) return e.prefab;
        }
        return entries[entries.Length - 1].prefab;
    }

    GameObject Get(GameObject prefab)
    {
        if (!pool.TryGetValue(prefab, out var stack)) pool[prefab] = stack = new Stack<GameObject>();
        if (stack.Count > 0) return stack.Pop();
        var go = Instantiate(prefab, transform);
        go.SetActive(false);
        prefabOf[go] = prefab;
        return go;
    }
}
