using System;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GridMovement : MonoBehaviour
{
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private LayerMask blockingLayer;
    [SerializeField] private Vector2 gridOrigin = new Vector2(-0.342f, -0.01f);
    [SerializeField] private bool autoComputeGridOriginFromStart = true;

    // -------------------------
    // Sprite cycling (Player only)
    // -------------------------
    [Header("Player Sprite Cycling (optional)")]
    [Tooltip("Sprites cycled in order on each accepted input press (Player tag only).")]
    [SerializeField] private Sprite s0;
    [SerializeField] private Sprite s1;
    [SerializeField] private Sprite s2;
    [SerializeField] private Sprite s3;

    private Sprite[] _cycleSprites;
    private int _cycleIndex = 0;
    private SpriteRenderer _sr;
    private bool _facingRight = true;

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

    private void Awake()
    {
        // Cache sprite renderer + build sprite array (Player-only usage, but harmless to cache always)
        _sr = GetComponentInChildren<SpriteRenderer>();
        _cycleSprites = new Sprite[4] { s0, s1, s2, s3 };

        // If you want to initialize _cycleIndex based on current sprite, uncomment:
        // if (CompareTag("Player") && _sr != null && _sr.sprite != null)
        // {
        //     for (int i = 0; i < _cycleSprites.Length; i++)
        //         if (_cycleSprites[i] == _sr.sprite) { _cycleIndex = i; break; }
        // }

        if (GlobalGameState.isLevel7 && autoComputeGridOriginFromStart)
        {
            Vector3 p = transform.position;
            float ox = p.x - Mathf.Round(p.x / gridSize) * gridSize;
            float oy = p.y - Mathf.Round(p.y / gridSize) * gridSize;
            gridOrigin = new Vector2(ox, oy);
        }
    }

    private void Update()
    {
        if (IsPaused || !HasControl || _isMoving) return;

        // LEVEL 7: wait
        if (GlobalGameState.isLevel7 && HasControl && Input.GetKeyDown(KeyCode.X))
        {
            // Sprite advance on "X" press too (Player only)
            AdvanceSpriteAndFacing(Vector2.zero, wasWait:true);

            TurnSystem.ResolveTurn(this, Vector2.zero, true);
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Vector2.up;

            AdvanceSpriteAndFacing(direction, wasWait:false);

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

            AdvanceSpriteAndFacing(direction, wasWait:false);

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

            AdvanceSpriteAndFacing(direction, wasWait:false);

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

            AdvanceSpriteAndFacing(direction, wasWait:false);

            if (GlobalGameState.isLevel7)
            {
                TurnSystem.ResolveTurn(this, direction, false);
                return;
            }
            TryMove(direction);
        }
        else if (Input.GetKeyDown(KeyCode.R))
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

    /// <summary>
    /// Player-only: advances sprite cycle once per input press, and updates facing when pressing left/right.
    /// W/S/X keep current facing. A/D flip facing.
    /// </summary>
    private void AdvanceSpriteAndFacing(Vector2 dir, bool wasWait)
    {
        if (!CompareTag("Player")) return;
        if (_sr == null) return;

        // Handle facing (only on left/right)
        if (dir == Vector2.right)
            _facingRight = true;
        else if (dir == Vector2.left)
            _facingRight = false;

        // Mirror along X axis when facing left (Unity SpriteRenderer flipX is easiest)
        _sr.flipX = _facingRight;

        // Advance cycle on any accepted input press (including wait X)
        // If you *don't* want wait to animate, remove the "wasWait" usage and early-return here.
        _cycleIndex = (_cycleIndex + 1) % _cycleSprites.Length;

        Sprite next = _cycleSprites[_cycleIndex];
        if (next != null)
            _sr.sprite = next;
        // If some slots are left null, we just keep the current sprite.
    }

    private void TryMove(Vector2 direction)
    {
        Vector2 targetPos = (Vector2)transform.position + direction * gridSize;

        const float cellCheckRadius = 0.15f;
        var charHits = Physics2D.OverlapCircleAll(targetPos, cellCheckRadius);
        foreach (var ch in charHits)
        {
            if (!ch) continue;
            if (ch.isTrigger) continue;
            if (IsSelfOrChild(ch.transform, transform)) continue;

            if (ch.CompareTag("Player") || ch.CompareTag("Robot"))
            {
                return;
            }
        }

        var hits = Physics2D.OverlapCircleAll(targetPos, 0.1f, blockingLayer);

        bool blockedBySolid = false;
        Pushable pushableInFront = null;

        foreach (var h in hits)
        {
            if (!h) continue;

            bool isLaser = h.CompareTag("Laser");
            bool iAmRobot = CompareTag("Robot");

            if (isLaser)
            {
                if (!iAmRobot)
                {
                    blockedBySolid = true;
                    break;
                }
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

        if (pushableInFront != null)
        {
            Vector2 boxTarget = (Vector2)pushableInFront.transform.position + direction * gridSize;

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
                if (h.CompareTag("Laser")) continue;
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

        TryMove(dir);
    }

    // -------------------------
    // LEVEL 7: composite movement (platform carry + intended move) with no input lag
    // -------------------------

    public void BeginLevel7CompositeMove(Vector2 carryDeltaWorld, Vector2 intendedDir, bool wasWait, float platformDuration)
    {
        if (!GlobalGameState.isLevel7) return;
        if (IsPaused) return;
        if (_isMoving) return;

        StartCoroutine(Level7CompositeMoveCoroutine(carryDeltaWorld, intendedDir, wasWait, platformDuration));
    }

    private IEnumerator Level7CompositeMoveCoroutine(Vector2 carryDeltaWorld, Vector2 intendedDir, bool wasWait, float platformDuration)
    {
        _isMoving = true;

        float half = platformDuration * 0.5f;
        if (half < 0f) half = 0f;

        Vector2 start = transform.position;
        Vector2 carried = start + carryDeltaWorld;

        // Phase A: immediately start moving with the platform (matches platform animation start)
        yield return StartCoroutine(LerpTo(start, carried, half));

        // Phase B: intended move (or wait)
        Vector2 final = carried;

        if (!wasWait && intendedDir != Vector2.zero)
        {
            Vector2 computed;
            if (EvaluateMoveFromPosition(carried, intendedDir, out computed))
            {
                final = computed;
            }
            else
            {
                final = carried; // blocked, so stay
            }
        }

        yield return StartCoroutine(LerpTo(carried, final, half));

        transform.position = SnapToGrid(final);
        _isMoving = false;
    }

    private IEnumerator LerpTo(Vector2 from, Vector2 to, float duration)
    {
        if (duration <= 0f)
        {
            transform.position = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector2.Lerp(from, to, t);
            yield return null;
        }

        transform.position = SnapToGrid(to);
    }

    private bool EvaluateMoveFromPosition(Vector2 fromPos, Vector2 dir, out Vector2 targetPos)
    {
        targetPos = fromPos + dir * gridSize;

        const float cellCheckRadius = 0.15f;

        var charHits = Physics2D.OverlapCircleAll(targetPos, cellCheckRadius);
        foreach (var ch in charHits)
        {
            if (!ch) continue;
            if (ch.isTrigger) continue;
            if (IsSelfOrChild(ch.transform, transform)) continue;

            if (ch.CompareTag("Player") || ch.CompareTag("Robot"))
                return false;
        }

        var hits = Physics2D.OverlapCircleAll(targetPos, 0.1f, blockingLayer);

        bool blockedBySolid = false;
        Pushable pushableInFront = null;

        foreach (var h in hits)
        {
            if (!h) continue;

            bool isLaser = h.CompareTag("Laser");
            bool iAmRobot = CompareTag("Robot");

            if (isLaser)
            {
                if (!iAmRobot) return false;
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

        if (blockedBySolid) return false;

        if (pushableInFront != null)
        {
            Vector2 boxTarget = (Vector2)pushableInFront.transform.position + dir * gridSize;

            var charAtBoxTarget = Physics2D.OverlapCircleAll(boxTarget, cellCheckRadius);
            foreach (var ch in charAtBoxTarget)
            {
                if (!ch) continue;
                if (ch.isTrigger) continue;
                if (ch.CompareTag("Player") || ch.CompareTag("Robot"))
                    return false;
            }

            var boxHits = Physics2D.OverlapCircleAll(boxTarget, 0.1f, blockingLayer);
            bool boxBlocked = false;
            foreach (var h in boxHits)
            {
                if (!h) continue;
                if (h.CompareTag("Laser")) continue;
                if (h.isTrigger) continue;
                boxBlocked = true;
                break;
            }
            if (boxBlocked) return false;

            pushableInFront.Push(dir);
        }

        return true;
    }

    private Vector3 SnapToGrid(Vector3 pos)
    {
        if (!GlobalGameState.isLevel7) return pos;

        float x = Mathf.Round((pos.x - gridOrigin.x) / gridSize) * gridSize + gridOrigin.x;
        float y = Mathf.Round((pos.y - gridOrigin.y) / gridSize) * gridSize + gridOrigin.y;
        return new Vector3(x, y, pos.z);
    }
}