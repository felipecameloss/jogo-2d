using UnityEngine;

// Número de dano / texto flutuante reaproveitável (ver FxManager).
[RequireComponent(typeof(TextMesh))]
public class FloatingText : MonoBehaviour
{
    public float lifetime = 0.6f;
    public float riseSpeed = 1.5f;

    TextMesh tm;
    Color startColor;
    float age;

    public void Play(string text, Color color)
    {
        if (!tm) tm = GetComponent<TextMesh>();
        tm.text = text;
        tm.color = color;
        startColor = color;
        age = 0f;
        gameObject.SetActive(true);
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime)
        {
            FxManager.Release(this);
            return;
        }
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        var c = startColor;
        c.a = 1f - age / lifetime;
        tm.color = c;
    }
}
