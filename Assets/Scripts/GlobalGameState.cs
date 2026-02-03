using UnityEngine;
using System.Collections.Generic;

public static class GlobalGameState
{
    public static bool isLowBranching = false;
    public static bool lazerHitRobot = false;
    public static bool lazerHitRobot2 = false;
    public static bool dialogueActive = false;
    public static bool swallowNextSpace =  false;
    public static bool isRobotHacked =  false;
    public static bool isRobotHacked2 =  false;
    public static bool isRobotSaved = false;
    public static bool isLevel1 ;
    public static bool isLevel2 = false;
    public static bool isLevel3 = false;
    public static bool isLevel4 = false;
    public static bool isLevel6 = false;
    public static GameObject spaceUIRobot = null;
    public static bool[] stateSaver = new bool[5];
    
    // data
    public static bool isRobotTalked = false;
    public static bool isRobotTalked2 = false;
    public static int talkCount = 0;
    public static int hackCount = 0;
}