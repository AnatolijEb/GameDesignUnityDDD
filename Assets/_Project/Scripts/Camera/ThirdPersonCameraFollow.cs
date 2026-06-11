using UnityEngine;

[ExecuteAlways]
public class ThirdPersonCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 4f, -8f);
    public float followSpeed = 10f;
    public float lookAtHeight = 1f;
    public float lookAheadDistance = 10f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        if (Application.isPlaying)
        {
            // Follow position with smoothing in Play Mode
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );
        }
        else
        {
            // Snap immediately in Edit Mode for live preview
            transform.position = targetPosition;
        }

        // Stabilize rotation: Instead of Looking At the player (which causes skewing when off-center),
        // we look at a point straight ahead of the camera's current X position.
        // This keeps the road lines parallel.
        Vector3 lookAtTarget = new Vector3(transform.position.x, target.position.y + lookAtHeight, target.position.z + lookAheadDistance);
        transform.LookAt(lookAtTarget);
    }
}
