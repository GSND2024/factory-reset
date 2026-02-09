using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DeathTile2D : MonoBehaviour
{
    [Header("Tags")]
    public string playerTag = "Player";
    public string robotTag = "Robot";

    [Header("Player Death")]
    public float restartDelay = 0f;
    public bool disablePlayerOnDeath = true;

    private bool _playerTriggered;

    private void Reset()
    {
        Collider2D c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void Awake()
    {
        Collider2D c = GetComponent<Collider2D>();
        if (c) c.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // PLAYER: die + restart level
        if (!_playerTriggered && other.CompareTag(playerTag))
        {
            _playerTriggered = true;

            if (disablePlayerOnDeath)
                other.gameObject.SetActive(false);

            if (restartDelay <= 0f)
                RestartScene();
            else
                Invoke(nameof(RestartScene), restartDelay);

            return;
        }

        // ROBOT: destroy robot and return control to player
        if (other.CompareTag(robotTag))
        {
            HandleRobotDeath(other.gameObject);
        }
    }

    private void HandleRobotDeath(GameObject robot)
    {
        // Destroy robot
        Destroy(robot);

        // Return control to the player
        RestorePlayerControl();
    }

    private void RestorePlayerControl()
    {
        // Find player GridMovement and give control back
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        GridMovement playerMovement = player.GetComponent<GridMovement>();
        if (playerMovement != null)
        {
            playerMovement.HasControl = true;
        }
    }

    private void RestartScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
    }
}
