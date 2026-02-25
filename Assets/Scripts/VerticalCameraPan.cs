using UnityEngine;

public class VerticalCameraPan : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Settings")]
    public float thresholdY = 3f;
    public float panAmount = 5.75f;
    public float smoothSpeed = 5f;

    private Vector3 _originalPosition;
    private Vector3 _targetPosition;
    private bool _isUpperZone = false;

    void Start()
    {
        if (!player)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        _originalPosition = transform.position;
        _targetPosition = _originalPosition;
    }

    void Update()
    {
        if (!player) return;

        // Detect crossing upward
        if (!_isUpperZone && player.position.y > thresholdY)
        {
            _isUpperZone = true;
            _targetPosition = _originalPosition + new Vector3(0f, panAmount, 0f);
        }
        // Detect crossing downward
        else if (_isUpperZone && player.position.y < thresholdY)
        {
            _isUpperZone = false;
            _targetPosition = _originalPosition;
        }

        // Smoothly move camera
        transform.position = Vector3.Lerp(
            transform.position,
            _targetPosition,
            Time.deltaTime * smoothSpeed
        );
    }
}