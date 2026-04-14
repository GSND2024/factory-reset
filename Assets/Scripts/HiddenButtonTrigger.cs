using UnityEngine;

public class HiddenButtonTrigger : MonoBehaviour
{
    public GameObject lazer;
    [SerializeField] private AudioSource audioSource;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (lazer)
            {
                var lazerCollision = lazer.GetComponent<LazerCollision>();
                bool alreadyKilled = lazerCollision != null && lazerCollision.isPermakilled;

                if (!alreadyKilled)
                {
                    lazer.SetActive(false);
                    audioSource.Play();
                }
            }

            gameObject.SetActive(false);
        }
    }
}