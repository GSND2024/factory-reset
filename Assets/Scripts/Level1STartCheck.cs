using UnityEngine;

public class Level1StartCheck : MonoBehaviour
{
    private void Start()
    {
        GlobalGameState.isLevel1 = true;
        GlobalGameState.lazerHitRobot = false;
        GlobalGameState.isRobotHacked = false;
        GlobalGameState.isEachLevelTalked = false;
        GlobalGameState.isEachLevelHacked = false;
    }
}