using UnityEngine;
using System.Collections;

public class Pushable : MonoBehaviour
{
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private LayerMask blockingLayer;   // set to Blocking in Inspector
    [SerializeField] private string laserTag = "Laser"; // must match your Laser tag
    
    [Header("Audio")]
    [SerializeField] private AudioClip pushClip;
    [SerializeField] private float pushVolume = 1f;


    public bool Push(Vector2 direction)
    {
        // Calculate target pos for the box
        Vector2 targetPos = (Vector2)transform.position + direction * gridSize;

        // Is that target cell free for the box?
        var hits = Physics2D.OverlapCircleAll(targetPos, 0.1f, blockingLayer);

        foreach (var h in hits)
        {
            if (!h) continue;

            if (h.isTrigger) continue;              // triggers don't block
            if (h.CompareTag(laserTag)) continue;   // allow into lasers (optional)

            // Another solid thing there → can't push
            // Debug.Log("Blocked! Can't push into " + h.name);
            return false;
        }

        if (pushClip != null)
        {
           AudioManager.PlaySFXAtPoint(pushClip, transform.position, pushVolume);
        }

        StartCoroutine(Move(targetPos));

        return true;
    }

    private IEnumerator Move(Vector2 targetPos)
    {
        Vector2 start = transform.position;
        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector2.Lerp(start, targetPos, t / moveDuration);
            yield return null;
        }

        transform.position = targetPos;
    }
}
