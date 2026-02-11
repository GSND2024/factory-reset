using UnityEngine;
using UnityEngine.SceneManagement; 
public class FinalLevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GlobalGameState.isLevel7 = false;
            SceneManager.LoadScene("FinalLevel");
        }
    }
}
