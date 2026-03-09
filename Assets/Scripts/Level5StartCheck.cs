using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Level5StartCheck : MonoBehaviour
{
    public TMP_Text endingText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        /*
        if (GlobalGameState.isLowBranching)
        {
            if (GlobalGameState.isRobotHacked)
            {
                GlobalGameState.lazerHitRobot = true;
            }
            if (GlobalGameState.isRobotHacked2)
            {
                GlobalGameState.lazerHitRobot2 = true;
            }
        }

        if (!GlobalGameState.lazerHitRobot &&
            !GlobalGameState.lazerHitRobot2 &&
            GlobalGameState.isRobotSaved)
        {
            if (GlobalGameState.isRobotHacked &&
                GlobalGameState.isRobotHacked2)
            {
                endingText.text = "The mechanism activates and the door slides open. The two robots move to block your path, standing aligned, sensors tracking your movement";
            }
            else if (!GlobalGameState.isRobotHacked &&
                     !GlobalGameState.isRobotHacked2)
            {
                endingText.text = "The door unlocks with a steady click. Both robots fall in behind you, systems stable. Together, you cross the threshold";
            }
            else
            {
                endingText.text = "The final plate clicks into place and the door slides open. One robot turns toward you while the other remains still, their signals out of sync. You walk forward as their systems continue to process between themselves";
            }
        }
        else if (GlobalGameState.lazerHitRobot && (GlobalGameState.lazerHitRobot2 || !GlobalGameState.isRobotSaved))
        {
            endingText.text = "The last plate depresses and the door unlocks. No other machinery whirs or makes noise. You walk forward alone, the space behind empty and still";
        }
        else
        {
            if (GlobalGameState.isRobotHacked &&
                !GlobalGameState.lazerHitRobot)
            {
                endingText.text = "The door opens and the remaining robot stands still, sensors tracking your movement. The space feels emptier without the other unit, its loss noticeable only in the still air as you step through";
            }
            
            if (!GlobalGameState.isRobotHacked &&
                !GlobalGameState.lazerHitRobot)
            {
                endingText.text = "The door opens and the remaining robot falls in behind you. The space feels emptier without the other unit, its loss noticeable only in the still air as you step through";
            }

            if (GlobalGameState.isRobotHacked2 &&
                !GlobalGameState.lazerHitRobot2 &&
                GlobalGameState.isRobotSaved)
            {
                endingText.text = "The door opens and the remaining robot stands still, sensors tracking your movement. The space feels emptier without the other unit, its loss noticeable only in the still air as you step through";
            }
            
            if (!GlobalGameState.isRobotHacked2 &&
                !GlobalGameState.lazerHitRobot2 &&
                GlobalGameState.isRobotSaved)
            {
                endingText.text = "The door opens and the remaining robot falls in behind you. The space feels emptier without the other unit, its loss noticeable only in the still air as you step through";
            }
            

            
        }*/

        if (GlobalGameState.destroyCount == 5)
        {
            endingText.text = "Destroy End";
        }
        else if (GlobalGameState.HackAI)
        {
            if (GlobalGameState.hackCount == 8)
            {
                endingText.text = "Hack End";
            }
            else
            {
                endingText.text = "Control End";
            }
            
        }
        else
        {
            if (GlobalGameState.talkCount == 8 & GlobalGameState.hackCount == 0 & GlobalGameState.destroyCount == 0 & GlobalGameState.RootAI)
            {
                endingText.text = "Talk End";
            
            }
        
            else if (GlobalGameState.talkCount == 0 & GlobalGameState.hackCount == 0 & GlobalGameState.destroyCount == 0)
            {
                endingText.text = "Escape End";
            }

            else
            {
                endingText.text = "Normal End";
            }
        }
        


    }
    
}
