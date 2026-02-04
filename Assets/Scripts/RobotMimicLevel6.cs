using UnityEngine;

public class RobotMimicLevel6 : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridMovement playerMovement;
    [SerializeField] private GridMovement robotMovement;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private Actions robotActions;

    [Header("Zones")]
    [SerializeField] private LayerMask copyZoneMask;
    [SerializeField] private LayerMask safeZoneMask;
    [SerializeField] private float zoneCheckRadius = 0.22f;

    private ContactFilter2D _copyFilter;
    private readonly Collider2D[] _overlap = new Collider2D[16];

    private void Awake()
    {
        if (!playerMovement)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerMovement = p.GetComponent<GridMovement>();
        }

        if (!robotMovement) robotMovement = GetComponent<GridMovement>();
        if (!robotActions) robotActions = GetComponent<Actions>();

        if (!playerCollider && playerMovement)
            playerCollider = playerMovement.GetComponent<Collider2D>();

        _copyFilter = new ContactFilter2D();
        _copyFilter.useLayerMask = true;
        _copyFilter.layerMask = copyZoneMask;
        _copyFilter.useTriggers = true;
    }

    private void OnEnable()
    {
        if (playerMovement != null)
            playerMovement.OnMoveAcceptedLevel6 += OnPlayerMoveAccepted;

        if (robotMovement != null)
            robotMovement.OnMoveFinishedLevel6 += OnRobotMoveFinished;
    }

    private void OnDisable()
    {
        if (playerMovement != null)
            playerMovement.OnMoveAcceptedLevel6 -= OnPlayerMoveAccepted;

        if (robotMovement != null)
            robotMovement.OnMoveFinishedLevel6 -= OnRobotMoveFinished;
    }

    private void OnPlayerMoveAccepted(Vector2 dir)
    {
        if (!GlobalGameState.isLevel6) return;
        if (!playerCollider) return;
        if (!robotMovement) return;

        // Only copy if the player STARTED the move while overlapping CopyZone
        if (!PlayerIsOnCopyZone()) return;

        // Robot refuses to copy if it would leave SafeZone
        if (!RobotWouldRemainOnSafeZone(dir)) return;

        robotMovement.TryMoveFromScript_Level6(dir);
    }

    private void OnRobotMoveFinished(Vector2 robotNewPos)
    {
        if (!GlobalGameState.isLevel6) return;
        if (robotActions == null) return;

        // If hacked and robot ended off SafeZone, destroy it
        if (!IsOnSafeZone(robotNewPos))
        {
            Destroy(gameObject);
            if (playerMovement != null)
            {
                playerMovement.HasControl = true;
            }
        }
    }

    private bool PlayerIsOnCopyZone()
    {
        int count = playerCollider.Overlap(_copyFilter, _overlap);
        return count > 0;
    }

    private bool RobotWouldRemainOnSafeZone(Vector2 dir)
    {
        if (safeZoneMask.value == 0) return true;

        Vector2 direction = NormalizeToCardinal(dir);
        Vector2 current = transform.position;
        Vector2 target = current + direction * 1f;

        // Use robot's grid size if available (from its GridMovement)
        if (robotMovement != null)
        {
            // gridSize is private in your GridMovement, so we assume 1 here.
            // If your gridSize is not 1, tell me and I'll adjust this safely.
        }

        return IsOnSafeZone(current) && IsOnSafeZone(target);
    }

    private bool IsOnSafeZone(Vector2 worldPos)
    {
        if (safeZoneMask.value == 0) return true;
        return Physics2D.OverlapCircle(worldPos, zoneCheckRadius, safeZoneMask) != null;
    }

    private static Vector2 NormalizeToCardinal(Vector2 v)
    {
        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
            return new Vector2(Mathf.Sign(v.x), 0f);
        return new Vector2(0f, Mathf.Sign(v.y));
    }
} 
