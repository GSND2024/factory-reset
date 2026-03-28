using UnityEngine;

public class PressurePlateLevel4 : MonoBehaviour
{
    [Header("ID / Puzzle")]
    public int plateID; // Unique ID for this pressure plate (0, 1, 2, etc.)
    public bool locked = false; // Locked after puzzle success to keep pressed color

    [Header("Colors")]
    public Color idleColor = Color.gray;
    public Color pressedColor = Color.green;

    private bool isPressed = false;
    private SpriteRenderer rend;

    [Header("Audio")]
    [SerializeField] private AudioClip plateOn;
    [SerializeField] private AudioClip plateOff;
    [SerializeField] private float pushVolume = 1f;


    private void Start()
    {
        // Get SpriteRenderer for color control
        rend = GetComponent<SpriteRenderer>();
        SetIdleColor();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Robot") || other.CompareTag("Robot (1)"))
                && !isPressed && !locked)
        {
           AudioManager.PlaySFXAtPoint(plateOn, transform.position, pushVolume);
            isPressed = true;
            SetPressedColor();
            PuzzleManager2D.instance.PlatePressed(plateID);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Robot") || other.CompareTag("Robot (1)"))
            && !locked)
        {
           AudioManager.PlaySFXAtPoint(plateOff, transform.position, pushVolume);
            isPressed = false;
            // Do not revert color here directly;
            // PuzzleManager2D will handle reset or success lock.
        }
    }

    // ---- These methods are called by PuzzleManager2D ----
    public void SetIdleColor()
    {
        if (rend != null)
            rend.color = idleColor;
        isPressed = false;
    }

    public void SetPressedColor()
    {
        if (rend != null)
            rend.color = pressedColor;
        isPressed = true;
    }

    public void LockPressedColor()
    {
        locked = true;
        SetPressedColor();
    }

    public void UnlockAndReset()
    {
        locked = false;
        SetIdleColor();
    }
}

