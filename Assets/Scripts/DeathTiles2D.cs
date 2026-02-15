using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class DeathTile2D : MonoBehaviour
{
    [Header("Tags")]
    public string playerTag = "Player";
    public string robotTag = "Robot";

    [Header("Platform Safety Check")]
    [Tooltip("If the actor is overlapping this layer, they are considered 'on a platform' and will NOT die.")]
    public LayerMask platformLayerMask;

    [Tooltip("How far around the actor to check for a platform under them.")]
    public float platformCheckRadius = 0.20f;

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

    // Use Stay so moving platforms can "save" you even after you enter the death zone.
    private void OnTriggerStay2D(Collider2D other)
    {
        // If standing on a platform, do nothing.
        if (IsOnPlatform(other.transform.position))
            return;

        // PLAYER: die + restart level
        if (!_playerTriggered && other.CompareTag(playerTag))
        {
            _playerTriggered = true;

            if (disablePlayerOnDeath)
                other.gameObject.SetActive(false);

            if (restartDelay <= 0f)
            {
                GlobalGameState.talkCount = GlobalGameState.dataCountSaver[0];
                GlobalGameState.hackCount = GlobalGameState.dataCountSaver[1];
                GlobalGameState.destroyCount = GlobalGameState.dataCountSaver[2];
                RestartScene();
            }
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

    private bool IsOnPlatform(Vector2 actorWorldPos)
    {
        // If no platform layer mask set, treat as "not on platform"
        if (platformLayerMask.value == 0) return false;

        // OverlapCircle works well for grid games. Keep radius < half tile.
        Collider2D hit = Physics2D.OverlapCircle(actorWorldPos, platformCheckRadius, platformLayerMask);
        return hit != null;
    }

    private void HandleRobotDeath(GameObject robot)
    {
        Destroy(robot);

        GlobalGameState.destroyCount += 1;

        Debug.Log($"TalkCount: {GlobalGameState.talkCount}, HackCount: {GlobalGameState.hackCount}, destroyCount: {GlobalGameState.destroyCount}");

        RestorePlayerControl();
    }

    private void RestorePlayerControl()
    {
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
