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

    [Header("Optional feedback")]
    public SpriteRenderer indicator;     // change color when pressed (optional)
    public Color idleColor = Color.gray;
    public Color pressedColor = Color.green;
    public Color lockedColor = Color.cyan;

    // Public read-only state
    public bool IsPressed { get; private set; }

    // Fires whenever IsPressed changes (true/false)
    public event Action<bool> OnPressChanged;

    // Track current occupants that count
    private readonly HashSet<Collider2D> _pressing = new HashSet<Collider2D>();
    private bool _isLocked = false;

    private Collider2D _coll;

    private void Reset()
    {
        // Make sure the collider is a trigger
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    private void Awake()
    {
        _coll = GetComponent<Collider2D>();
        if (_coll != null) _coll.isTrigger = true;
        ApplyIndicator();
    }

    private void OnEnable()
    {
        // Clean slate
        _pressing.Clear();
        SetPressed(false);
    }

    private void OnDisable()
    {
        _pressing.Clear();
        SetPressed(false);
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
        // Tag filter
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
        OnPressChanged?.Invoke(IsPressed);
    }

    private void ApplyIndicator()
    {
        if (indicator == null) return;

        if (_isLocked)
            indicator.color = lockedColor;
        else
            indicator.color = IsPressed ? pressedColor : idleColor;
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

    private void Update()
    {
        // If we're currently pressed and the door is open, lock permanently.
        if (enableLatching && !_isLocked && IsPressed && latchDoor != null && latchDoor.isOpen)
        {
            _isLocked = true;
            _pressing.Clear();      // optional: stops caring about occupants
            // If you want listeners to know it latched (if not already pressed), you could:
            // SetPressed(true);
        }
        ApplyIndicator();
    }
}
