using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    private Vector3 offset = new Vector3(0f, 4f, -8f);

    void LateUpdate()
    {
        if (target == null) return;

        // Follow position with smoothing
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            10f * Time.deltaTime
        );

        // Always look at a point slightly above the player
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}
