using System;
using UnityEngine;

public class Level0StartCheck : MonoBehaviour
{
    private void Awake()
    {
        GlobalGameState.dataCountSaver[0] = GlobalGameState.talkCount;
        GlobalGameState.dataCountSaver[1] = GlobalGameState.hackCount;
        GlobalGameState.dataCountSaver[2] = GlobalGameState.destroyCount;
    }

    private void Start()
    {
        GlobalGameState.isLevel0 = true;
        GlobalGameState.lazerHitRobot = false;
        GlobalGameState.isRobotHacked = false;
        GlobalGameState.isEachLevelTalked = false;
        GlobalGameState.isEachLevelHacked = false;
    }

}
