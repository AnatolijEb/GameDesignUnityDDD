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
        // We only smooth X and Y, but keep Z relatively fixed to the target (which is fixed at 0 anyway)
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // Stabilize rotation: Instead of Looking At the player (which causes skewing when off-center),
        // we look at a point straight ahead of the camera's current X position.
        // This keeps the road lines parallel.
        Vector3 lookAtTarget = new Vector3(transform.position.x, target.position.y + lookAtHeight, target.position.z + 10f);
        transform.LookAt(lookAtTarget);
    }
}
