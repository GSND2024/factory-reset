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
    
    [Header("Button Outlines (Optional - will auto-find if empty)")]
    [Tooltip("Outline component for selection border")]
    public Outline yesButtonOutline;
    public Outline noButtonOutline;
    
    [Tooltip("Or use Image component as border (if no Outline)")]
    public Image yesButtonBorderImage;
    public Image noButtonBorderImage;
    
    [Header("Button Colors")]
    public Color yesNormalColor = new Color(23f/255f, 217f/255f, 109f/255f); // Green
    public Color noNormalColor = new Color(23f/255f, 217f/255f, 109f/255f); // Green
    public Color greyedOutColor = Color.grey;
    public Color selectedColor = Color.white;
    
    [Header("Typewriter Settings")]
    [Tooltip("Characters per second")]
    public float typeSpeed = 30f;
    
    [Tooltip("Delay between lines")]
    public float delayBetweenLines = 0.3f;
    
    [Header("Audio Settings")]
    [Tooltip("Typewriter sound effect")]
    public AudioClip typewriterSound;
    
    [Tooltip("Volume for typewriter sound")]
    [Range(0f, 1f)]
    public float typewriterVolume = 1f;
    
    private AudioSource audioSource;
    
    [Header("Skip Settings")]
    [Tooltip("Allow skipping typewriter effect with Space")]
    public bool allowSkip = true;
    
    [Header("Condition")]
    [Tooltip("Required talkCount to enable Yes button")]
    public int requiredTalkCount = 8;
    
    [Header("Next Scene")]
    [Tooltip("Name of the next scene to load")]
    public string nextSceneName = "PrototypeLevel5";
    
    // Public state variables (accessible by other scripts like redX.cs)
    [HideInInspector] public bool canSelectYes = false;
    [HideInInspector] public bool canMakeChoice = false;
    
    private int currentSelection = 1; // 0 = Yes, 1 = No (default to No)
    private bool isTyping = false;
    private bool skipTyping = false;
    private bool isSceneReady = false;
    
    [System.Serializable]
    public class DialogueLine
    {
        public TextMeshProUGUI textComponent;
        public string content;
    }
    
    void Start()
    {
        // Wait for scene transition to complete before accepting input
        StartCoroutine(WaitForSceneReady());
        
        // Initialize all text to empty
        foreach (var line in dialogueLines)
        {
            if (line.textComponent != null)
            {
                line.textComponent.text = "";
            }
        }
        
        // Setup audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // Loop the typewriter sound
        audioSource.playOnAwake = false;
        
        // Apply volume control from AudioManager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.RegisterAudioSource(audioSource, false); // false = SFX
        }
        
        // Auto-find Outline or Image components if not assigned
        if (yesButtonOutline == null && yesButtonBorderImage == null && yesButton != null)
        {
            // Try to find Outline component first
            yesButtonOutline = yesButton.GetComponent<Outline>();
            // If no Outline, try to find an Image child named "Outline"
            if (yesButtonOutline == null)
            {
                Transform outlineTransform = yesButton.transform.Find("Outline");
                if (outlineTransform != null)
                {
                    yesButtonBorderImage = outlineTransform.GetComponent<Image>();
                }
            }
        }
        
        if (noButtonOutline == null && noButtonBorderImage == null && noButton != null)
        {
            noButtonOutline = noButton.GetComponent<Outline>();
            if (noButtonOutline == null)
            {
                Transform outlineTransform = noButton.transform.Find("Outline");
                if (outlineTransform != null)
                {
                    noButtonBorderImage = outlineTransform.GetComponent<Image>();
                }
            }
        }
        
        // Hide buttons initially
        if (yesButton != null) yesButton.gameObject.SetActive(false);
        if (noButton != null) noButton.gameObject.SetActive(false);
        
        // Hide outlines/borders initially
        SetOutlineEnabled(yesButtonOutline, yesButtonBorderImage, false);
        SetOutlineEnabled(noButtonOutline, noButtonBorderImage, false);
        
        // Check if player can select Yes
        CheckYesButtonAvailability();
    }
    
    // Wait for scene transition fade-in to complete
    private IEnumerator WaitForSceneReady()
    {
        // Ensure time scale is normal (in case we came from paused state)
        Time.timeScale = 1f;
        
        // Wait a bit for SceneTransition to finish fade-in
        // Use WaitForSecondsRealtime to work even if Time.timeScale = 0
        yield return new WaitForSecondsRealtime(1.0f); // Slightly longer than fade duration
        
        isSceneReady = true;
        
        // Start dialogue sequence after scene is ready
        StartCoroutine(PlayDialogueSequence());
    }
    
    // Helper method to enable/disable outline or border image
    private void SetOutlineEnabled(Outline outline, Image borderImage, bool enabled)
    {
        if (borderImage != null)
        {
            // Use Image border (preferred method)
            borderImage.gameObject.SetActive(enabled);
        }
        else if (outline != null)
        {
            // Use Outline component (fallback)
            outline.enabled = enabled;
        }
    }
    
    void Update()
    {
        // Don't allow any input until scene is ready (after fade-in)
        if (!isSceneReady) return;
        
        // Allow skipping all remaining dialogue with Space
        if (allowSkip && isTyping && Input.GetKeyDown(KeyCode.Space))
        {
            SkipAllDialogue();
            return;
        }
        
        // Only allow button selection when choices are available
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
    
    // Skip all remaining dialogue and show buttons immediately
    private void SkipAllDialogue()
    {
        // Stop all typewriter coroutines
        StopAllCoroutines();
        
        // Stop typewriter sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Show all dialogue lines immediately
        foreach (var line in dialogueLines)
        {
            if (line.textComponent != null && !string.IsNullOrEmpty(line.content))
            {
                line.textComponent.text = line.content;
            }
        }
        
        isTyping = false;
        
        // Show buttons immediately
        ShowButtons();
    }
    
    private void CheckYesButtonAvailability()
    {
        canSelectYes = (GlobalGameState.talkCount >= requiredTalkCount) && 
                       (GlobalGameState.hackCount == 0) &&
                       (GlobalGameState.destroyCount == 0);
        
        // If can't select Yes, default to No
        if (!canSelectYes)
        {
            currentSelection = 1;
        }
    }
    
    private IEnumerator PlayDialogueSequence()
    {
        // Play typewriter sound at the start
        if (typewriterSound != null && audioSource != null)
        {
            audioSource.clip = typewriterSound;
            audioSource.volume = typewriterVolume;
            audioSource.Play();
        }
        
        // Type each line one by one
        foreach (var line in dialogueLines)
        {
            if (line.textComponent != null && !string.IsNullOrEmpty(line.content))
            {
                skipTyping = false; // Reset skip flag for each line
                yield return StartCoroutine(TypeLine(line.textComponent, line.content));
                
                // Only wait between lines if not skipped
                if (!skipTyping)
                {
                    yield return new WaitForSeconds(delayBetweenLines);
                }
            }
        }
        
        // Stop typewriter sound when done
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        // Show buttons after all lines are typed
        ShowButtons();
    }
    
    private IEnumerator TypeLine(TextMeshProUGUI textComponent, string content)
    {
        if (textComponent == null) yield break;
        
        textComponent.text = "";
        isTyping = true;
        skipTyping = false;
        
        for (int i = 0; i < content.Length; i++)
        {
            // If skip is triggered, show all remaining text immediately
            if (skipTyping)
            {
                textComponent.text = content;
                break;
            }
            
            textComponent.text += content[i];
            yield return new WaitForSeconds(1f / typeSpeed);
        }
        
        isTyping = false;
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
            if (yesButtonText != null) yesButtonText.color = yesNormalColor;
            if (yesButton != null) yesButton.interactable = true;
        }
        else
        {
            // Yes button is greyed out
            if (yesButtonText != null) yesButtonText.color = greyedOutColor;
            if (yesButton != null) yesButton.interactable = false;
        }
        
        // No button is always enabled
        if (noButtonText != null) noButtonText.color = noNormalColor;
        if (noButton != null) noButton.interactable = true;
        
        // Setup button click events
        if (yesButton != null) yesButton.onClick.AddListener(() => SelectOption(0));
        if (noButton != null) noButton.onClick.AddListener(() => SelectOption(1));
        
        // Debug: Check if outlines were found
        Debug.Log($"Yes Outline found: {yesButtonOutline != null}, Yes Border Image found: {yesButtonBorderImage != null}");
        Debug.Log($"No Outline found: {noButtonOutline != null}, No Border Image found: {noButtonBorderImage != null}");
        
        // Don't enable outlines here - let UpdateButtonSelection handle it
        // This prevents Yes outline from showing when greyed out
        
        // Update initial selection
        UpdateButtonSelection();
        
        // Enable choice
        canMakeChoice = true;
    }
    
    private void UpdateButtonSelection()
    {
        Debug.Log($"UpdateButtonSelection called - currentSelection: {currentSelection}, canSelectYes: {canSelectYes}");
        
        // Update text colors and outlines
        if (currentSelection == 0 && canSelectYes)
        {
            // Yes selected (and available)
            if (yesButtonText != null) yesButtonText.color = selectedColor;
            if (noButtonText != null) noButtonText.color = noNormalColor;
            
            // Show Yes outline, hide No outline
            SetOutlineEnabled(yesButtonOutline, yesButtonBorderImage, true);
            SetOutlineEnabled(noButtonOutline, noButtonBorderImage, false);
            Debug.Log("Yes selected - Yes outline ON, No outline OFF");
        }
        else
        {
            // No selected (or Yes not available)
            if (canSelectYes && yesButtonText != null)
            {
                // Yes is available but not selected
                yesButtonText.color = yesNormalColor;
            }
            else if (!canSelectYes && yesButtonText != null)
            {
                // Yes is greyed out
                yesButtonText.color = greyedOutColor;
            }
            
            if (noButtonText != null) noButtonText.color = selectedColor;
            currentSelection = 1;
            
            // Always hide Yes outline when not selected
            SetOutlineEnabled(yesButtonOutline, yesButtonBorderImage, false);
            // Always show No outline when No is selected
            SetOutlineEnabled(noButtonOutline, noButtonBorderImage, true);
            Debug.Log("No selected - Yes outline OFF, No outline ON");
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
            SceneTransition.Instance.LoadSceneWithFade(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (yesButton != null) yesButton.onClick.RemoveAllListeners();
        if (noButton != null) noButton.onClick.RemoveAllListeners();
    }
}