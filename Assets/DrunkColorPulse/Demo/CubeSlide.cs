using UnityEngine;

public class CubeSlide : MonoBehaviour
{    
    public Vector3 targetPosition = new Vector3(5, 0, 0);
    public float speed = 10f;
    private Vector3 origPosition;
    private Vector3 currTargetPosition;

    void Start()
    {
        origPosition = transform.position;
        currTargetPosition = targetPosition;
    }

    void Update()
    {                
        transform.position = Vector3.MoveTowards(transform.position, currTargetPosition, speed * Time.deltaTime);
        if (transform.position == targetPosition) currTargetPosition = origPosition;
        else if (transform.position == origPosition) currTargetPosition = targetPosition;
    }
}