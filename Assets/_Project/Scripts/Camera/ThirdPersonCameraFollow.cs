using UnityEngine;

public class ThirdPersonCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 4f, -8f);
    public float followSpeed = 10f;
    public float lookAtHeight = 1f;

    private void LateUpdate()
    {
        if (target == null) return;

        // Follow position with smoothing
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // Always look at a point slightly above the player
        transform.LookAt(target.position + Vector3.up * lookAtHeight);
    }
}
