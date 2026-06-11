using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollisionHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Debug.Log($"[Collision] Hit: {other.gameObject.name} (Tag: {other.tag})");
            // TODO: replace with pizza loss once life system is implemented
            GameManager gm = Object.FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                gm.TriggerGameOver();
            }
            else
            {
                // Fallback to reloading the active scene directly
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
}
