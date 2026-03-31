using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndingDisplayUI : MonoBehaviour
{
    [System.Serializable]
    public class EndingEntry
    {
        public string endingName;
        public TextMeshProUGUI labelText;
        public Image checkboxImage;
    }
    
    [Header("Ending Entries")]
    public EndingEntry controlEnding;
    public EndingEntry destroyEnding;
    public EndingEntry escapeEnding;
    public EndingEntry hackEnding;
    public EndingEntry talkEnding;
    public EndingEntry normalEnding;
    
    [Header("Checkbox Sprites")]
    public Sprite uncheckedSprite;
    public Sprite checkedSprite;
    
    [Header("Text Colors")]
    public Color unlockedColor = Color.white;
    public Color lockedColor = Color.gray;
    
    void Start()
    {
        UpdateEndingDisplay();
    }
    
    void Update()
    {
        #if UNITY_EDITOR
        // Press Q to reset all endings (only in Unity Editor)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            EndingTracker.ResetAllEndings();
            UpdateEndingDisplay();
            Debug.Log("All endings have been reset!");
        }
        #endif
    }
    
    // Public method to refresh display (can be called from other scripts)
    public void UpdateEndingDisplay()
    {
        UpdateEndingEntry(controlEnding, EndingTracker.hasReachedControl);
        UpdateEndingEntry(destroyEnding, EndingTracker.hasReachedDestroy);
        UpdateEndingEntry(escapeEnding, EndingTracker.hasReachedEscape);
        UpdateEndingEntry(hackEnding, EndingTracker.hasReachedHack);
        UpdateEndingEntry(talkEnding, EndingTracker.hasReachedTalk);
        UpdateEndingEntry(normalEnding, EndingTracker.hasReachedNormal);
    }
    
    private void UpdateEndingEntry(EndingEntry entry, bool isUnlocked)
    {
        if (entry.checkboxImage != null)
        {
            // Change checkbox sprite
            if (isUnlocked && checkedSprite != null)
            {
                entry.checkboxImage.sprite = checkedSprite;
            }
            else if (!isUnlocked && uncheckedSprite != null)
            {
                entry.checkboxImage.sprite = uncheckedSprite;
            }
            
            // Or use color/alpha
            Color checkColor = entry.checkboxImage.color;
            checkColor.a = isUnlocked ? 1f : 0.3f;
            entry.checkboxImage.color = checkColor;
        }
        
        if (entry.labelText != null)
        {
            entry.labelText.color = isUnlocked ? unlockedColor : lockedColor;
        }
    }
}