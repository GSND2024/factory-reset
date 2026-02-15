using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private LayerMask blockingLayer;

    private bool _isMoving = false;

    public bool HasControl = true;
    private Vector2 direction;

    public bool IsPaused { get; private set; } = false;

    public void SetPaused(bool paused) => IsPaused = paused;

    public event Action<Vector2> OnMoveAcceptedLevel6;
    public event Action<Vector2> OnMoveFinishedLevel6;

    public void TryMoveFromScript_Level6(Vector2 dir)
    {
        if (!GlobalGameState.isLevel6) return;
        if (IsPaused || _isMoving) return;

        // Intentionally ignores HasControl in Level 6 so the robot can be moved by scripts
        TryMove(dir);
    }


    private void Update()
    {
        if (IsPaused || !HasControl || _isMoving) return;

        if (GlobalGameState.isLevel7 && HasControl && Input.GetKeyDown(KeyCode.X))
        {
            TurnSystem.ResolveTurn(this, Vector2.zero, true);
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Vector2.up;
            if (GlobalGameState.isLevel7)
            {
                TurnSystem.ResolveTurn(this, direction, false);
                return;
            }
            TryMove(direction);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Vector2.down;
            if (GlobalGameState.isLevel7)
            {
                TurnSystem.ResolveTurn(this, direction, false);
                return;
            }
            TryMove(direction);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Vector2.left;
            if (GlobalGameState.isLevel7)
            {
                TurnSystem.ResolveTurn(this, direction, false);
                return;
            }
            TryMove(direction);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Vector2.right;
            if (GlobalGameState.isLevel7)
            {
                TurnSystem.ResolveTurn(this, direction, false);
                return;
            }
            TryMove(direction);
        }
        else if (Input.GetKeyDown(KeyCode.R) )
        {
            if (!GlobalGameState.dialogueActive)
            {
                GlobalGameState.talkCount = GlobalGameState.dataCountSaver[0];
                GlobalGameState.hackCount = GlobalGameState.dataCountSaver[1];
                GlobalGameState.destroyCount = GlobalGameState.dataCountSaver[2];
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void TryMove(Vector2 direction)
    {
        Vector2 targetPos = (Vector2)transform.position + direction * gridSize;

        // ---- NEW: Block if a Player/Robot is already in the target cell (no layer dependence) ----
        // Small radius to sample the grid cell center; tweak if your cells/colliders are larger.
        const float cellCheckRadius = 0.15f;
        var charHits = Physics2D.OverlapCircleAll(targetPos, cellCheckRadius);
        foreach (var ch in charHits)
        {
            if (!ch) continue;
            if (ch.isTrigger) continue; // ignore trigger-only sensors
            if (IsSelfOrChild(ch.transform, transform)) continue; // don't block on our own collider

            if (ch.CompareTag("Player") || ch.CompareTag("Robot"))
            {
                // Someone's standing there—no entry.
                return;
            }
        }
        // --------------------------------------------------------------------

        // Existing blocking logic (walls/solids/lasers/pushables)
        var hits = Physics2D.OverlapCircleAll(targetPos, 0.1f, blockingLayer);

        bool blockedBySolid = false;
        Pushable pushableInFront = null;

        foreach (var h in hits)
        {
            if (!h) continue;

            bool isLaser = h.CompareTag("Laser");
            bool iAmRobot = CompareTag("Robot");

            // LASER RULES: robots can enter lasers; others cannot.
            if (isLaser)
            {
                if (!iAmRobot)
                {
                    blockedBySolid = true;
                    break;
                }
                // if robot, allow and continue
                continue;
            }

            if (h.isTrigger) continue;

            var p = h.GetComponentInParent<Pushable>();
            if (p != null)
            {
                pushableInFront = p;
                continue;
            }

            blockedBySolid = true;
            break;
        }

        if (blockedBySolid) return;

        // Handle pushing a box (still respects characters in the box's target cell)
        if (pushableInFront != null)
        {
            Vector2 boxTarget = (Vector2)pushableInFront.transform.position + direction * gridSize;

            // Don't push into a Player/Robot either.
            var charAtBoxTarget = Physics2D.OverlapCircleAll(boxTarget, cellCheckRadius);
            foreach (var ch in charAtBoxTarget)
            {
                if (!ch) continue;
                if (ch.isTrigger) continue;
                if (ch.CompareTag("Player") || ch.CompareTag("Robot"))
                    return;
            }

            var boxHits = Physics2D.OverlapCircleAll(boxTarget, 0.1f, blockingLayer);
            bool boxBlocked = false;
            foreach (var h in boxHits)
            {
                if (!h) continue;
                if (h.CompareTag("Laser")) continue; // allow if that's intended
                if (h.isTrigger) continue;
                boxBlocked = true;
                break;
            }
            if (boxBlocked) return;

            pushableInFront.Push(direction);
        }

        if (GlobalGameState.isLevel6 && CompareTag("Player"))
        {
            OnMoveAcceptedLevel6?.Invoke(direction);
        }

        StartCoroutine(Move(targetPos));
    }

    private static bool IsSelfOrChild(Transform candidate, Transform root)
    {
        if (candidate == null || root == null) return false;
        var t = candidate;
        while (t != null)
        {
            if (t == root) return true;
            t = t.parent;
        }
        return false;
    }

    private IEnumerator Move(Vector2 targetPos)
    {
        _isMoving = true;

        Vector2 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            transform.position = Vector2.Lerp(startPosition, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        _isMoving = false;

        if (GlobalGameState.isLevel6)
        {
            OnMoveFinishedLevel6?.Invoke(targetPos);
        }

    }

    public void TryMoveFromLevel7(Vector2 dir)
    {
        if (!GlobalGameState.isLevel7) return;
        if (IsPaused) return;
        if (_isMoving) return;

        // Ignore HasControl here so TurnSystem can apply the move after platform carry.
        TryMove(dir);
    }

}
