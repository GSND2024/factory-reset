using UnityEngine;

public class TurnSystemInit : MonoBehaviour
{
    public LayerMask platformLayerMask;
    public float platformDetectRadius = 0.05f;

    private void Awake()
    {
        TurnSystem.platformLayerMask = platformLayerMask;
        TurnSystem.platformDetectRadius = platformDetectRadius;
    }
}
