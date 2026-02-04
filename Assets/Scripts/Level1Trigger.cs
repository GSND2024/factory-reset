using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1Trigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GlobalGameState.isLevel0 = false;
            SceneManager.LoadScene("PrototypeLevel1");
        }
    }
}
