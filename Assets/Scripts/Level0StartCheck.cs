using UnityEngine;

public class Level0StartCheck : MonoBehaviour
{
    private void Start()
    {
        GlobalGameState.isLevel0 = true;
        GlobalGameState.lazerHitRobot = false;
        GlobalGameState.isRobotHacked = false;
    }

}
