using UnityEngine;
using System.Collections;

public class MovingPlatformTurnBased2D : MonoBehaviour
{
    public enum MoveAxis { Horizontal, Vertical }

    [Header("Movement Pattern")]
    public MoveAxis moveAxis = MoveAxis.Horizontal;
    public int stepsOneWay = 5;
    public bool startPositiveDirection = true;
    public int pauseTurnsAtEnds = 1;

    [Header("Grid")]
    public float gridSize = 1f;

    [Header("Movement Timing")]
    public float moveDuration = 0.1f;

    [Header("Z Lock")]
    public float lockedZ = -2f;

    [Header("Blocking")]
    public LayerMask blockingLayer;
    public float blockCheckRadius = 0.10f;

    [Header("Passenger Detection")]
    public float passengerCheckRadius = 0.2f;
    public LayerMask passengerLayerMask;
    public float passengerRadiusPadding = 0.02f;

    private int _progress;
    private int _dirSign;
    private int _pauseRemaining;
    private bool _isMoving;

    private BoxCollider2D _box;
    private Vector2 _riderCheckCenter;

    private void Awake()
    {
        Vector3 pos = transform.position;
        pos.z = lockedZ;
        transform.position = pos;

        _dirSign = startPositiveDirection ? 1 : -1;

        CacheColliderAndComputeRiderRadius();
    }

    private void OnEnable()
    {
        TurnSystem.OnPlatformStep += StepPlatform;
    }

    private void OnDisable()
    {
        TurnSystem.OnPlatformStep -= StepPlatform;
    }

    public Vector2 PreviewDeltaWorldThisTurn()
    {
        if (_isMoving) return Vector2.zero;

        if (_pauseRemaining > 0) return Vector2.zero;

        Vector2Int stepDir = GetStepDir();
        Vector2 deltaXY = (Vector2)stepDir * gridSize;

        Vector2 targetXY = (Vector2)transform.position + deltaXY;
        if (!CanMoveTo(targetXY)) return Vector2.zero;

        return deltaXY;
    }

    private void StepPlatform()
    {
        if (_isMoving) return;

        if (_pauseRemaining > 0)
        {
            _pauseRemaining--;
            return;
        }

        CacheColliderAndComputeRiderRadius();

        Vector2Int stepDir = GetStepDir();
        Vector2 deltaXY = (Vector2)stepDir * gridSize;

        Vector2 startXY = transform.position;
        Vector2 targetXY = startXY + deltaXY;

        if (!CanMoveTo(targetXY))
        {
            ReverseDirectionAndPause();
            return;
        }

        // Snapshot riders BEFORE moving
        Collider2D[] riders = Physics2D.OverlapCircleAll(_riderCheckCenter, passengerCheckRadius, passengerLayerMask);

        StartCoroutine(MovePlatformSmooth(startXY, targetXY, riders));

        _progress++;
        if (_progress >= stepsOneWay)
        {
            ReverseDirectionAndPause();
        }
    }

    private IEnumerator MovePlatformSmooth(Vector2 startXY, Vector2 targetXY, Collider2D[] riders)
    {
        _isMoving = true;

        Vector3 platformStart = new Vector3(startXY.x, startXY.y, lockedZ);
        Vector3 platformEnd = new Vector3(targetXY.x, targetXY.y, lockedZ);
        Vector3 delta = platformEnd - platformStart;

        // Cache rider start/end positions
        Transform[] riderTransforms = null;
        Vector3[] riderStarts = null;
        Vector3[] riderEnds = null;

        if (riders != null && riders.Length > 0)
        {
            riderTransforms = new Transform[riders.Length];
            riderStarts = new Vector3[riders.Length];
            riderEnds = new Vector3[riders.Length];

            for (int i = 0; i < riders.Length; i++)
            {
                Collider2D c = riders[i];
                if (!c)
                {
                    riderTransforms[i] = null;
                    continue;
                }

                Transform t = c.transform;
                riderTransforms[i] = t;

                riderStarts[i] = t.position;
                riderEnds[i] = t.position + delta;
            }
        }

        float tElapsed = 0f;

        while (tElapsed < moveDuration)
        {
            tElapsed += Time.deltaTime;
            float lerp = (moveDuration <= 0f) ? 1f : (tElapsed / moveDuration);

            transform.position = Vector3.Lerp(platformStart, platformEnd, lerp);

            if (riderTransforms != null)
            {
                for (int i = 0; i < riderTransforms.Length; i++)
                {
                    Transform rt = riderTransforms[i];
                    if (!rt) continue;
                    rt.position = Vector3.Lerp(riderStarts[i], riderEnds[i], lerp);
                }
            }

            yield return null;
        }

        transform.position = platformEnd;

        if (riderTransforms != null)
        {
            for (int i = 0; i < riderTransforms.Length; i++)
            {
                Transform rt = riderTransforms[i];
                if (!rt) continue;
                rt.position = riderEnds[i];
            }
        }

        _isMoving = false;
    }

    private Vector2Int GetStepDir()
    {
        if (moveAxis == MoveAxis.Horizontal)
            return new Vector2Int(_dirSign, 0);

        return new Vector2Int(0, _dirSign);
    }

    private void ReverseDirectionAndPause()
    {
        _dirSign *= -1;
        _progress = 0;
        _pauseRemaining = Mathf.Max(0, pauseTurnsAtEnds);
    }

    private bool CanMoveTo(Vector2 targetPos)
    {
        Collider2D hit = Physics2D.OverlapCircle(targetPos, blockCheckRadius, blockingLayer);
        return hit == null;
    }

    private void CacheColliderAndComputeRiderRadius()
    {
        if (_box == null)
            _box = GetComponentInChildren<BoxCollider2D>();

        if (_box == null)
        {
            _riderCheckCenter = transform.position;
            return;
        }

        Bounds b = _box.bounds;
        _riderCheckCenter = b.center;

        float minWorldDim = Mathf.Min(b.size.x, b.size.y);
        passengerCheckRadius = (minWorldDim * 0.25f) + passengerRadiusPadding;
    }
}
