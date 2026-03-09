using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConditionalButton : MonoBehaviour
{
    [Header("Text Component")]
    public TextMeshProUGUI buttonText;
    
    [Header("Default State")]
    public string defaultText = "Hack";
    public Color defaultColor = Color.white;
    
    [Header("Changed State")]
    public string changedText = "Root";
    public Color changedColor = Color.red;
    
    [Header("Condition")]
    [Tooltip("HackCount value required to change the button")]
    public int requiredHackCount = 8;
    
    private bool isChanged = false;
    
    void Awake()
    {
        // Auto-assign text component if not set
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    
    void Start()
    {
        // Initialize to default state
        SetButtonState(false);
        
        // Check condition at start
        CheckAndUpdate();
    }
    
    void Update()
    {
        // Continuously check condition
        CheckAndUpdate();
    }
    
    private void CheckAndUpdate()
    {
        
        // Check if hackCount meets requirement
        bool shouldChange = GlobalGameState.hackCount >= requiredHackCount;
        
        // Update button state if changed
        if (shouldChange != isChanged)
        {
            isChanged = shouldChange;
            SetButtonState(isChanged);
        }
    }
    
    private void SetButtonState(bool changed)
    {
        if (buttonText != null)
        {
            buttonText.text = changed ? changedText : defaultText;
            buttonText.color = changed ? changedColor : defaultColor;
        }
    }
}