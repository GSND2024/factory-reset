using DialogueScripts;
using UnityEngine;

public class Level6StartCheck : MonoBehaviour
{
    private void Awake()
    {
        GlobalGameState.dataCountSaver[0] = GlobalGameState.talkCount;
        GlobalGameState.dataCountSaver[1] = GlobalGameState.hackCount;
        GlobalGameState.dataCountSaver[2] = GlobalGameState.destroyCount;
    }
    private void Start()
    {
        GlobalGameState.isLevel6 = true;
        GlobalGameState.isEachLevelTalked = false;
        GlobalGameState.isEachLevelHacked = false;
    }
}