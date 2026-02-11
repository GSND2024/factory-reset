using UnityEngine;

public class MovingPlatformTurnBased2D : MonoBehaviour
{
    public enum MoveAxis
    {
        Horizontal,
        Vertical
    }

    [Header("Movement Pattern")]
    public MoveAxis moveAxis = MoveAxis.Horizontal;

    [Tooltip("How many tiles to move in one direction before reversing.")]
    public int stepsOneWay = 5;

    [Tooltip("If true, starts moving Right (Horizontal) or Up (Vertical). If false, starts Left/Down.")]
    public bool startPositiveDirection = true;

    [Tooltip("Pause this many turns when reaching either end (before reversing).")]
    public int pauseTurnsAtEnds = 1;

    [Header("Grid")]
    public float gridSize = 1f;

    [Header("Z Lock")]
    public float lockedZ = -2f;

    [Header("Blocking")]
    public LayerMask blockingLayer;
    public float blockCheckRadius = 0.10f;

    [Header("Passenger Carry")]
    public float passengerCheckRadius = 0.20f;
    public LayerMask passengerLayerMask;

    [Tooltip("Small padding added to auto radius.")]
    public float passengerRadiusPadding = 0.05f;

    private int _progress;
    private int _dirSign;
    private int _pauseRemaining;

    private void Awake()
    {
        Vector3 pos = transform.position;
        pos.z = lockedZ;
        transform.position = pos;

        _dirSign = startPositiveDirection ? 1 : -1;

        AutoScalePassengerRadius();
    }

    private void OnEnable()
    {
        TurnSystem.OnPlatformStep += StepPlatform;
    }

    private void OnDisable()
    {
        TurnSystem.OnPlatformStep -= StepPlatform;
    }

    private void StepPlatform()
    {
        if (_pauseRemaining > 0)
        {
            _pauseRemaining--;
            return;
        }

        Vector2Int stepDir = GetStepDir();

        // Snapshot riders BEFORE moving
        Collider2D[] riders = Physics2D.OverlapCircleAll(
            (Vector2)transform.position,
            passengerCheckRadius,
            passengerLayerMask
        );

        Vector2 targetXY = (Vector2)transform.position + (Vector2)stepDir * gridSize;

        if (!CanMoveTo(targetXY))
        {
            ReverseDirectionAndPause();
            return;
        }

        // Move platform (Z locked)
        transform.position = new Vector3(targetXY.x, targetXY.y, lockedZ);

        // Carry riders AFTER moving (compute delta from stepDir here)
        CarryRiders(riders, stepDir);

        _progress++;

        if (_progress >= stepsOneWay)
        {
            ReverseDirectionAndPause();
        }
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

    private void CarryRiders(Collider2D[] riders, Vector2Int stepDir)
    {
        if (riders == null || riders.Length == 0) return;

        Vector2 deltaXY = (Vector2)stepDir * gridSize;

        for (int i = 0; i < riders.Length; i++)
        {
            Collider2D c = riders[i];
            if (!c) continue;

            Transform t = c.transform;

            Vector3 newPos = t.position;
            newPos.x += deltaXY.x;
            newPos.y += deltaXY.y;
            t.position = newPos;
        }
    }

    private bool CanMoveTo(Vector2 targetPos)
    {
        Collider2D hit = Physics2D.OverlapCircle(targetPos, blockCheckRadius, blockingLayer);
        return hit == null;
    }

    private void AutoScalePassengerRadius()
    {
        Vector3 scale = transform.lossyScale;

        // Use the smaller dimension so we only grab riders
        // that are clearly centered on THIS platform
        float minDimension = Mathf.Min(scale.x, scale.y);

        // Quarter-tile radius + small padding
        passengerCheckRadius = (minDimension * gridSize) * 0.25f + passengerRadiusPadding;
    }
}
