using UnityEngine;

public class EndingTracker : MonoBehaviour
{
    // Static flags for each ending (persistent across scenes)
    public static bool hasReachedControl = false;
    public static bool hasReachedDestroy = false;
    public static bool hasReachedEscape = false;
    public static bool hasReachedHack = false;
    public static bool hasReachedTalk = false;
    public static bool hasReachedNormal = false;
    
    // PlayerPrefs keys
    private const string KEY_CONTROL = "Ending_Control";
    private const string KEY_DESTROY = "Ending_Destroy";
    private const string KEY_ESCAPE = "Ending_Escape";
    private const string KEY_HACK = "Ending_Hack";
    private const string KEY_TALK = "Ending_Talk";
    private const string KEY_NORMAL = "Ending_Normal";
    
    void Awake()
    {
        // Load saved endings from PlayerPrefs
        LoadEndingProgress();
        
        // Debug: Log initial state
        Debug.Log("=== Ending Tracker Loaded ===");
        Debug.Log($"Control: {hasReachedControl}");
        Debug.Log($"Destroy: {hasReachedDestroy}");
        Debug.Log($"Escape: {hasReachedEscape}");
        Debug.Log($"Hack: {hasReachedHack}");
        Debug.Log($"Talk: {hasReachedTalk}");
        Debug.Log($"Normal: {hasReachedNormal}");
    }
    
    // Mark an ending as reached
    public static void UnlockEnding(string endingName)
    {
        switch (endingName.ToLower())
        {
            case "control":
                hasReachedControl = true;
                PlayerPrefs.SetInt(KEY_CONTROL, 1);
                break;
            case "destroy":
                hasReachedDestroy = true;
                PlayerPrefs.SetInt(KEY_DESTROY, 1);
                break;
            case "escape":
                hasReachedEscape = true;
                PlayerPrefs.SetInt(KEY_ESCAPE, 1);
                break;
            case "hack":
                hasReachedHack = true;
                PlayerPrefs.SetInt(KEY_HACK, 1);
                break;
            case "talk":
                hasReachedTalk = true;
                PlayerPrefs.SetInt(KEY_TALK, 1);
                break;
            case "normal":
                hasReachedNormal = true;
                PlayerPrefs.SetInt(KEY_NORMAL, 1);
                break;
        }
        PlayerPrefs.Save();
    }
    
    // Load ending progress from PlayerPrefs
    private void LoadEndingProgress()
    {
        hasReachedControl = PlayerPrefs.GetInt(KEY_CONTROL, 0) == 1;
        hasReachedDestroy = PlayerPrefs.GetInt(KEY_DESTROY, 0) == 1;
        hasReachedEscape = PlayerPrefs.GetInt(KEY_ESCAPE, 0) == 1;
        hasReachedHack = PlayerPrefs.GetInt(KEY_HACK, 0) == 1;
        hasReachedTalk = PlayerPrefs.GetInt(KEY_TALK, 0) == 1;
        hasReachedNormal = PlayerPrefs.GetInt(KEY_NORMAL, 0) == 1;
    }
    
    // Reset all endings (for testing)
    public static void ResetAllEndings()
    {
        hasReachedControl = false;
        hasReachedDestroy = false;
        hasReachedEscape = false;
        hasReachedHack = false;
        hasReachedTalk = false;
        hasReachedNormal = false;
        
        PlayerPrefs.DeleteKey(KEY_CONTROL);
        PlayerPrefs.DeleteKey(KEY_DESTROY);
        PlayerPrefs.DeleteKey(KEY_ESCAPE);
        PlayerPrefs.DeleteKey(KEY_HACK);
        PlayerPrefs.DeleteKey(KEY_TALK);
        PlayerPrefs.DeleteKey(KEY_NORMAL);
        PlayerPrefs.Save();
    }
    
    // Get total number of endings reached
    public static int GetEndingsReachedCount()
    {
        int count = 0;
        if (hasReachedControl) count++;
        if (hasReachedDestroy) count++;
        if (hasReachedEscape) count++;
        if (hasReachedHack) count++;
        if (hasReachedTalk) count++;
        if (hasReachedNormal) count++;
        return count;
    }
}