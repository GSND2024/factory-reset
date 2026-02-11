using UnityEngine;
using UnityEngine.SceneManagement; 

public class Level7Trigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GlobalGameState.isLevel6 = false;
            SceneManager.LoadScene("PrototypeLevel7");
        }
    }
}