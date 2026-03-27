using UnityEngine;
using System.Collections.Generic;

public static class GlobalGameState
{
    public static bool isLowBranching = false;
    public static bool lazerHitRobot = false;
    public static bool lazerHitRobot2 = false;
    public static bool dialogueActive = false;
    public static bool swallowNextSpace =  false;
    public static bool isRobotHacked =  false; //levels 1+2
    public static bool isRobotHacked2 =  false; //level 3
    public static bool isYellowHacked =  false; //level 5
    public static bool isPurpleHacked =  false; //level 6
    public static bool isWhiteHacked =  false; //level 0
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
    public static bool RootAI = false;
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
    public static int[] dataCountSaver = {0, 0, 0};
}