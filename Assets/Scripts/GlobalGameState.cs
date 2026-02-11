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
    public static bool isLevel0 = false;
    public static bool isLevel1 = false;
    public static bool isLevel2 = false;
    public static bool isLevel3 = false;
    public static bool isLevel4 = false;
    public static bool isLevel5 = false;
    public static bool isLevel6 = false;
    public static bool isLevel7 = false;
    public static bool isFinalLevel = false;
    public static bool HackAI = false;
    public static GameObject spaceUIRobot = null;
    public static bool[] stateSaver = new bool[5];
    
    // data
    public static bool isEachLevelTalked = false;
    public static bool isEachLevelTalked2 = false;
    public static bool isEachLevelHacked = false;
    public static bool isEachLevelHacked2 = false;
    public static int talkCount = 0;
    public static int hackCount = 0;
    public static int destroyCount = 0;
}