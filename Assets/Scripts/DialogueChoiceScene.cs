using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DialogueChoiceScene : MonoBehaviour
{
    [Header("Dialogue Lines")]
    [Tooltip("Add all dialogue lines in order (including first and last fixed lines)")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    
    [Header("Buttons")]
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI yesButtonText;
    public TextMeshProUGUI noButtonText;
    
    [Header("Button Colors")]
    public Color normalColor = new Color(23f/255f, 217f/255f, 109f/255f); // Green
    public Color greyedOutColor = Color.grey;
    public Color selectedColor = Color.white;
    
    [Header("Typewriter Settings")]
    [Tooltip("Characters per second")]
    public float typeSpeed = 30f;
    
    [Tooltip("Delay between lines")]
    public float delayBetweenLines = 0.3f;
    
    private bool canSelectYes = false;
    private bool canMakeChoice = false;
    private int currentSelection = 1; // 0 = Yes, 1 = No (default to No)
    
    [System.Serializable]
    public class DialogueLine
    {
        public TextMeshProUGUI textComponent;
        public string content;
    }
    
    void Start()
    {
        // Initialize all text to empty
        foreach (var line in dialogueLines)
        {
            if (line.textComponent != null)
            {
                line.textComponent.text = "";
            }
        }
        
        // Hide buttons initially
        if (yesButton != null) yesButton.gameObject.SetActive(false);
        if (noButton != null) noButton.gameObject.SetActive(false);
        
        // Check if player can select Yes
        CheckYesButtonAvailability();
        
        // Start dialogue sequence
        StartCoroutine(PlayDialogueSequence());
    }
    
    void Update()
    {
        if (!canMakeChoice) return;
        
        // Handle input
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (canSelectYes)
            {
                currentSelection = 0; // Yes
                UpdateButtonSelection();
            }
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentSelection = 1; // No
            UpdateButtonSelection();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
        }
    }
    
    private void CheckYesButtonAvailability()
    {
        if (GlobalGameState.talkCount == 8 & GlobalGameState.hackCount == 0 & GlobalGameState.destroyCount == 0)
        {
            canSelectYes = true;
        }
        else
        {
            canSelectYes = false;
        }
        
        // If can't select Yes, default to No
        if (!canSelectYes)
        {
            currentSelection = 1;
        }
    }
    
    private IEnumerator PlayDialogueSequence()
    {
        // Type each line one by one
        foreach (var line in dialogueLines)
        {
            if (line.textComponent != null && !string.IsNullOrEmpty(line.content))
            {
                yield return StartCoroutine(TypeLine(line.textComponent, line.content));
                yield return new WaitForSeconds(delayBetweenLines);
            }
        }
        
        // Show buttons after all lines are typed
        ShowButtons();
    }
    
    private IEnumerator TypeLine(TextMeshProUGUI textComponent, string content)
    {
        if (textComponent == null) yield break;
        
        textComponent.text = "";
        
        foreach (char c in content)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(1f / typeSpeed);
        }
    }
    
    private void ShowButtons()
    {
        // Show buttons
        if (yesButton != null) yesButton.gameObject.SetActive(true);
        if (noButton != null) noButton.gameObject.SetActive(true);
        
        // Setup button states
        if (canSelectYes)
        {
            // Yes button is enabled
            if (yesButtonText != null) yesButtonText.color = normalColor;
            if (yesButton != null) yesButton.interactable = true;
        }
        else
        {
            // Yes button is greyed out
            if (yesButtonText != null) yesButtonText.color = greyedOutColor;
            if (yesButton != null) yesButton.interactable = false;
        }
        
        // No button is always enabled
        if (noButtonText != null) noButtonText.color = normalColor;
        if (noButton != null) noButton.interactable = true;
        
        // Setup button click events
        if (yesButton != null) yesButton.onClick.AddListener(() => SelectOption(0));
        if (noButton != null) noButton.onClick.AddListener(() => SelectOption(1));
        
        // Update initial selection
        UpdateButtonSelection();
        
        // Enable choice
        canMakeChoice = true;
    }
    
    private void UpdateButtonSelection()
    {
        if (currentSelection == 0 && canSelectYes)
        {
            // Yes selected
            if (yesButtonText != null) yesButtonText.color = selectedColor;
            if (noButtonText != null) noButtonText.color = normalColor;
        }
        else
        {
            // No selected (or Yes not available)
            if (canSelectYes && yesButtonText != null)
            {
                yesButtonText.color = normalColor;
            }
            if (noButtonText != null) noButtonText.color = selectedColor;
            currentSelection = 1;
        }
    }
    
    private void SelectOption(int option)
    {
        if (option == 0 && !canSelectYes) return;
        
        currentSelection = option;
        ConfirmSelection();
    }
    
    private void ConfirmSelection()
    {
        if (!canMakeChoice) return;
        
        canMakeChoice = false;
        
        // If Yes is selected, set RootAI to true
        if (currentSelection == 0 && canSelectYes)
        {
                GlobalGameState.RootAI = true;
                Debug.Log("RootAI set to true");

        }
        
        // Both options lead to same scene
        LoadNextScene();
    }
    
    private void LoadNextScene()
    {
        // Use scene transition if available
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadSceneWithFade("PrototypeLevel5");
        }
        else
        {
            SceneManager.LoadScene("PrototypeLevel5");
        }
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (yesButton != null) yesButton.onClick.RemoveAllListeners();
        if (noButton != null) noButton.onClick.RemoveAllListeners();
    }
}