using System;
using UnityEngine;

public static class TurnSystem
{
    public static event Action OnPlatformStep;

    // Set these from a scene initializer (TurnSystemInit) in Level 7
    public static LayerMask platformLayerMask;
    public static float platformDetectRadius = 0.05f;

    public static void ResolveTurn(GridMovement actor, Vector2 intendedDir, bool wasWait)
    {
        if (!GlobalGameState.isLevel7) return;
        if (actor == null) return;

        // Preview carry delta from the platform currently under the actor
        Vector2 carryDelta = Vector2.zero;
        MovingPlatformTurnBased2D platform = FindPlatformUnder(actor.transform.position);
        if (platform != null)
            carryDelta = platform.PreviewDeltaWorldThisTurn();

        // Start platforms moving immediately (they animate themselves)
        OnPlatformStep?.Invoke();

        // Start actor animation immediately (carry then intended move) over the same duration as platforms
        float platformDuration = (platform != null) ? platform.moveDuration : 0.1f;
        actor.BeginLevel7CompositeMove(carryDelta, intendedDir, wasWait, platformDuration);
    }

    private static MovingPlatformTurnBased2D FindPlatformUnder(Vector2 actorPos)
    {
        if (platformLayerMask.value == 0) return null;

        Collider2D hit = Physics2D.OverlapCircle(actorPos, platformDetectRadius, platformLayerMask);
        if (hit == null) return null;

        return hit.GetComponentInParent<MovingPlatformTurnBased2D>();
    }
}
