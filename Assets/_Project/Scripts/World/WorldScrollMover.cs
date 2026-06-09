using UnityEngine;

public class WorldScrollMover : MonoBehaviour
{
    public RunSpeedManager runSpeedManager;

    private void Start()
    {
        if (runSpeedManager == null)
        {
            runSpeedManager = RunSpeedManager.Instance;
            if (runSpeedManager == null)
            {
                runSpeedManager = Object.FindFirstObjectByType<RunSpeedManager>();
            }
        }
    }

    private void Update()
    {
        if (runSpeedManager != null)
        {
            transform.Translate(Vector3.back * runSpeedManager.CurrentSpeed * Time.deltaTime, Space.World);
        }
    }
}
