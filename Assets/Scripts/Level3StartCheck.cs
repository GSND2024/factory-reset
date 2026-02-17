using DialogueScripts;
using UnityEngine;

public class Level3StartCheck : MonoBehaviour
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
        GlobalGameState.isLevel3 = true;
        GlobalGameState.isEachLevelTalked = false;
        GlobalGameState.isEachLevelHacked = false;
        GlobalGameState.isRobotHacked2 = false;
    }
}