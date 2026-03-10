using System;
using UnityEngine;

public class Level4StartCheck : MonoBehaviour
{
    public GameObject robot;
    public GameObject robot2;

    private void Awake()
    {
        GlobalGameState.dataCountSaver[0] = GlobalGameState.talkCount;
        GlobalGameState.dataCountSaver[1] = GlobalGameState.hackCount;
        GlobalGameState.dataCountSaver[2] = GlobalGameState.destroyCount;
    }
    private void Start()
    {
        GlobalGameState.isLevel3 = false;
        GlobalGameState.isLevel4 = true;
        GlobalGameState.lazerHitRobot = GlobalGameState.stateSaver[0];
        GlobalGameState.isRobotHacked = GlobalGameState.stateSaver[1];
        GlobalGameState.lazerHitRobot2 = GlobalGameState.stateSaver[2];
        GlobalGameState.isRobotHacked2 = GlobalGameState.stateSaver[3];
        GlobalGameState.isRobotSaved = GlobalGameState.stateSaver[4];
        GlobalGameState.isEachLevelTalked = false;
        GlobalGameState.isEachLevelHacked = false;
        
        if (GlobalGameState.lazerHitRobot)
        {
            if (robot)
            {
                Destroy(robot);
            }
        }

        if (GlobalGameState.isRobotHacked)
        {
            if (GlobalGameState.isLowBranching)
            {
                if (robot)
                {
                    Destroy(robot);
                }
            }
            
            if (robot)
            {
                Actions actions = robot.GetComponentInChildren<Actions>();
                DialogueHolder dialHol = robot.GetComponentInChildren<DialogueHolder>();
                dialHol.dialogue.hacked = true;
            }
        }
        
        if (GlobalGameState.lazerHitRobot2)
        {
            if (robot2)
            {
                Destroy(robot2);
            }
        }
        
        if (!GlobalGameState.isRobotSaved)
        {
            if (robot2)
            {
                Destroy(robot2);
            }
        }

        if (GlobalGameState.isRobotHacked2)
        {
            if (GlobalGameState.isLowBranching)
            {
                if (robot2)
                {
                    Destroy(robot2);
                }
            }
            
            if (robot2)
            {
                Actions actions = robot2.GetComponentInChildren<Actions>();
                DialogueHolder dialHol = robot2.GetComponentInChildren<DialogueHolder>();
                dialHol.dialogue.hacked = true;
            }
        }
    }
}