using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerCollisionHandler : MonoBehaviour
{
    private PlayerLifeSystem lifeSystem;

    private void Awake()
    {
        lifeSystem = GetComponent<PlayerLifeSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            Debug.Log($"[Collision] Hit: {other.gameObject.name} (Tag: {other.tag})");
            
            if (lifeSystem != null)
            {
                lifeSystem.LoseLife();
            }
            else
            {
                // Fallback to old behavior if no life system
                GameManager gm = GameManager.Instance;
                if (gm != null)
                {
                    gm.TriggerGameOver();
                }
                else
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}
