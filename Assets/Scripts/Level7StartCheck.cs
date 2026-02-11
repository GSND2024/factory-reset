using UnityEngine;

public class Level7StartCheck : MonoBehaviour
{
    private void Start()
    {
        GlobalGameState.isLevel7 = true;
        GlobalGameState.isEachLevelTalked = false;
        GlobalGameState.isEachLevelHacked = false;
    }
}
