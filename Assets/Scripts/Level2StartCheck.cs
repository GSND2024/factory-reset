using DialogueScripts;
using UnityEngine;

public class Level2StartCheck : MonoBehaviour
{
    public GameObject robot;

    private void Awake()
    {
        GlobalGameState.dataCountSaver[0] = GlobalGameState.talkCount;
        GlobalGameState.dataCountSaver[1] = GlobalGameState.hackCount;
        GlobalGameState.dataCountSaver[2] = GlobalGameState.destroyCount;
    }
    private void Start()
    {
        GlobalGameState.isLevel2 = true;
        GlobalGameState.lazerHitRobot = GlobalGameState.stateSaver[0];
        GlobalGameState.isRobotHacked = GlobalGameState.stateSaver[1];
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
                DialogueHolder dialHol = robot.GetComponentInChildren<DialogueHolder>();
                dialHol.dialogue.hacked = true; 
            }
        }
    }
}