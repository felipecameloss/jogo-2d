using UnityEngine;

// Animação simples em loop (árvores e arbustos balançando).
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteLoop : MonoBehaviour
{
    public Sprite[] frames;
    public float frameTime = 0.12f;
    [Tooltip("Começa num frame aleatório para as plantas não balançarem todas juntas.")]
    public bool randomStart = true;

    SpriteRenderer sr;
    float time;

    void Awake() => sr = GetComponent<SpriteRenderer>();

    void OnEnable()
    {
        if (randomStart && frames != null) time = Random.Range(0f, frames.Length * frameTime);
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        time += Time.deltaTime;
        sr.sprite = frames[(int)(time / frameTime) % frames.Length];
    }
}
