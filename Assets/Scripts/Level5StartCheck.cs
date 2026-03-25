using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Level5StartCheck : MonoBehaviour
{
    public TMP_Text endingText;
    
    void Start()
    {
        // Check conditions and load appropriate ending
        // Also unlock the ending in EndingTracker
        
        if (GlobalGameState.destroyCount == 5)
        {
            EndingTracker.UnlockEnding("destroy");
            SceneManager.LoadScene("EndDestroy");
        }
        else if (GlobalGameState.HackAI)
        {
            if (GlobalGameState.hackCount == 8)
            {
                EndingTracker.UnlockEnding("hack");
                SceneManager.LoadScene("EndHack");
            }
            else
            {
                EndingTracker.UnlockEnding("control");
                SceneManager.LoadScene("EndControl");
            }
        }
        else
        {
            if (GlobalGameState.talkCount == 8 && GlobalGameState.hackCount == 0 && GlobalGameState.destroyCount == 0 && GlobalGameState.RootAI)
            {
                EndingTracker.UnlockEnding("talk");
                SceneManager.LoadScene("EndTalk");
            }
            else if (GlobalGameState.talkCount == 0 && GlobalGameState.hackCount == 0 && GlobalGameState.destroyCount == 0)
            {
                EndingTracker.UnlockEnding("escape");
                SceneManager.LoadScene("EndEscape");
            }
            else
            {
                EndingTracker.UnlockEnding("normal");
                SceneManager.LoadScene("EndNormal");
            }
        }
    }
}