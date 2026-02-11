using System;
using UnityEngine;

public static class TurnSystem
{
    // Platforms listen to this. When fired, they move one step and carry riders.
    public static event Action OnPlatformStep;

    // True while TurnSystem is resolving a turn (prevents re-entrancy)
    public static bool IsResolving { get; private set; }

    // Stores what the actor intended for THIS turn
    private static Vector2 _pendingDir;
    private static bool _pendingWasWait;
    private static GridMovement _pendingActor;

    public static void ResolveTurn(GridMovement actor, Vector2 intendedDir, bool wasWait)
    {
        if (IsResolving) return;
        if (actor == null) return;

        IsResolving = true;

        _pendingActor = actor;
        _pendingDir = intendedDir;
        _pendingWasWait = wasWait;

        // 1) Platforms move and carry first
        OnPlatformStep?.Invoke();

        // 2) Then actor performs intended action
        if (!_pendingWasWait)
        {
            _pendingActor.TryMoveFromLevel7(_pendingDir);
        }

        _pendingActor = null;
        IsResolving = false;
    }
}
