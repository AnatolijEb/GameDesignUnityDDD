using UnityEngine;

public class PickupHoverMotion : MonoBehaviour
{
    [SerializeField] private float hoverHeight = 1.4f;
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float rotationSpeed = 90f;

    private Vector3 baseLocalPosition;
    private float phaseOffset;

    private void Awake()
    {
        baseLocalPosition = transform.localPosition + Vector3.up * hoverHeight;
        phaseOffset = Random.value * Mathf.PI * 2f;
        transform.localPosition = baseLocalPosition;
    }

    private void Update()
    {
        float bob = Mathf.Sin(Time.time * bobSpeed + phaseOffset) * bobAmplitude;
        transform.localPosition = baseLocalPosition + Vector3.up * bob;
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }
}
