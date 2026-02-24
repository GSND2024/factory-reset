using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DoorSpriteOpener : MonoBehaviour
{
    [Header("References")]
    [Tooltip("GameObject to watch. When this is disabled, the sprite swaps.")]
    public GameObject targetToCheck;

    [Tooltip("Sprite to use when the target is disabled.")]
    public Sprite disabledSprite;

    private SpriteRenderer _spriteRenderer;
    private Sprite _originalSprite;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalSprite = _spriteRenderer.sprite;
    }

    private void Update()
    {
        if (targetToCheck == null || disabledSprite == null)
            return;

        // If the target GameObject is disabled anywhere in the hierarchy
        bool targetIsDisabled = !targetToCheck.activeInHierarchy;

        // Swap sprite based on state
        _spriteRenderer.sprite = targetIsDisabled
            ? disabledSprite
            : _originalSprite;
    }
}