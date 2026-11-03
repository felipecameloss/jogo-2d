using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class FloatingText : MonoBehaviour
{
    public float lifetime = 0.6f;
    public float riseSpeed = 1.5f;

    TextMesh tm;
    Color startColor;
    float age;

    void Start()
    {
        tm = GetComponent<TextMesh>();
        startColor = tm.color;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        var c = startColor;
        c.a = 1f - age / lifetime;
        tm.color = c;
    }
}
