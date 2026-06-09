using UnityEngine;

public class RoadChunk : MonoBehaviour
{
    public float chunkLength = 60f;
    
    [Header("Child References")]
    public Transform road;
    public Transform wallLeft;
    public Transform wallRight;
    public Transform obstacleParent;

    public Vector3 EndPosition => transform.position + Vector3.forward * chunkLength;
}
