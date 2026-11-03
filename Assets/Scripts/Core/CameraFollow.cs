using UnityEngine;

// Câmera que segue o jogador suavemente e sabe tremer (screen shake).
public class CameraFollow : MonoBehaviour
{
    public static CameraFollow Instance { get; private set; }

    public Transform target;
    public float smoothing = 10f;

    Vector3 basePosition;
    float shakeTime, shakeDuration, shakeIntensity;

    void Awake()
    {
        Instance = this;
        basePosition = transform.position;
    }

    void LateUpdate()
    {
        if (target)
        {
            Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);
            basePosition = Vector3.Lerp(basePosition, goal, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
        }

        Vector3 offset = Vector3.zero;
        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            offset = (Vector3)(Random.insideUnitCircle * shakeIntensity * (shakeTime / shakeDuration));
        }
        transform.position = basePosition + offset;
    }

    public void Shake(float intensity, float duration)
    {
        float current = shakeTime > 0f ? shakeIntensity * (shakeTime / shakeDuration) : 0f;
        if (intensity < current) return;
        shakeIntensity = intensity;
        shakeDuration = shakeTime = duration;
    }
}
