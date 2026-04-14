using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PressurePlate2D : MonoBehaviour
{
    [Header("What can press this plate")]
    [Tooltip("List of allowed tags that can press the plate.")]
    public List<string> allowedTags = new List<string> { "Player", "Pushable" };

    [Header("Is this plate locking?")]
    public bool enableLatching = true;

    [Tooltip("DoorController that has isOpen flag")]
    public DoorLatchController latchDoor;

    [Header("Optional feedback (Sprites)")]
    [Tooltip("SpriteRenderer to swap sprites on. (Usually the plate's own SpriteRenderer.)")]
    public SpriteRenderer indicator;

    [Tooltip("Sprite shown when not pressed.")]
    public Sprite idleSprite;

    [Tooltip("Sprite shown when pressed (but not locked).")]
    public Sprite pressedSprite;

    [Tooltip("Sprite shown when locked.")]
    public Sprite lockedSprite;

    // Public read-only state
    public bool IsPressed { get; private set; }

    // Fires whenever IsPressed changes (true/false)
    public event Action<bool> OnPressChanged;

    // Track current occupants that count
    private readonly HashSet<Collider2D> _pressing = new HashSet<Collider2D>();
    private bool _isLocked = false;

    private Collider2D _coll;
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pressClip;
    [SerializeField] private AudioClip releaseClip;
    [SerializeField] private AudioClip doorLock;
    [SerializeField] private float pushVolume = 1f;
    [Header("Laser")]
    [SerializeField] private LazerCollision laser;

    private void Reset()
    {
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;

        if (indicator == null) indicator = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        _coll = GetComponent<Collider2D>();
        if (_coll != null) _coll.isTrigger = true;

        ApplyIndicator();
    }

    private void OnEnable()
    {
        _pressing.Clear();
        _isLocked = false;
        SetPressed(false);
        ApplyIndicator();
    }

    private void OnDisable()
    {
        _pressing.Clear();
        _isLocked = false;
        SetPressed(false);
        ApplyIndicator();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isLocked) return;
        if (!Counts(other)) return;

        if (_pressing.Add(other))
        {
            if (!IsPressed) SetPressed(true);
        }

        TryLatch();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_isLocked) return;
        if (!Counts(other)) return;

        if (_pressing.Remove(other))
        {
            if (_pressing.Count == 0 && IsPressed) SetPressed(false);
        }
    }

    private bool Counts(Collider2D col)
    {
        if (allowedTags != null && allowedTags.Count > 0)
        {
            foreach (var t in allowedTags)
                if (col.CompareTag(t)) return true;
            return false;
        }
        return true;
    }

    private void SetPressed(bool value)
    {
        if (IsPressed == value) return;

        IsPressed = value;

        if (IsPressed && pressClip != null)
            audioSource.PlayOneShot(pressClip);
        else if (!IsPressed && releaseClip != null)
            audioSource.PlayOneShot(releaseClip);

        // Notify listeners (DoorLatchController may set isOpen=true here)
        OnPressChanged?.Invoke(IsPressed);

        // Only play door lock sound if the door was actually open AND the laser wasn't permanently killed
        if (!IsPressed && doorLock != null && !enableLatching && audioSource != null
            && latchDoor != null && latchDoor.isOpen
            && (laser == null || !laser.isPermakilled))
        {
            audioSource.PlayOneShot(doorLock, pushVolume);
        }

        // If pressing this plate caused the door to open, latch immediately.
        if (IsPressed && enableLatching && !_isLocked && latchDoor != null && latchDoor.isOpen)
        {
            LockNow();
            return;
        }

        ApplyIndicator();
    }

    private void ApplyIndicator()
    {
        if (indicator == null) return;

        Sprite target =
            _isLocked ? lockedSprite :
            IsPressed ? pressedSprite :
            idleSprite;

        if (target != null)
            indicator.sprite = target;
    }

    private void TryLatch()
    {
        if (!enableLatching) return;
        if (_isLocked) return;
        if (latchDoor == null) return;

        if (latchDoor.isOpen)
        {
            _isLocked = true;
            _pressing.Clear();
            SetPressed(true);
        }
    }

    private void LockNow()
    {
        if (_isLocked) return;

        _isLocked = true;
        _pressing.Clear();
        ApplyIndicator();
    }

    private void Update()
    {
        if (enableLatching && !_isLocked && IsPressed && latchDoor != null && latchDoor.isOpen)
        {
            _isLocked = true;
            _pressing.Clear();
            ApplyIndicator();
        }
    }
}