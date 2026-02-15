using UnityEngine;

public class Level7StartCheck : MonoBehaviour
{
    private void Awake()
    {
        GlobalGameState.dataCountSaver[0] = GlobalGameState.talkCount;
        GlobalGameState.dataCountSaver[1] = GlobalGameState.hackCount;
        GlobalGameState.dataCountSaver[2] = GlobalGameState.destroyCount;
    }
    private void Start()
    {
        GlobalGameState.isLevel7 = true;
        GlobalGameState.isEachLevelTalked = false;
        GlobalGameState.isEachLevelHacked = false;
    }
}
