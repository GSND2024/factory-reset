using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EndSceneManager : MonoBehaviour
{
    [Header("Text Lines")]
    [Tooltip("Add all text lines in order (excluding 'Press Space' line)")]
    public List<TextMeshProUGUI> textLines = new List<TextMeshProUGUI>();
    
    [Header("Continue Prompt")]
    public TextMeshProUGUI continueText;
    public string continueMessage = "Press Space to Continue";
    
    [Header("Timing Settings")]
    [Tooltip("Time for each line to fade in")]
    public float fadeInDuration = 1.5f;
    
    [Tooltip("Delay between lines")]
    public float delayBetweenLines = 0.5f;
    
    [Tooltip("Delay before showing continue prompt after last line")]
    public float delayBeforeContinue = 1.0f;
    
    [Tooltip("Blinking speed for continue prompt")]
    public float blinkSpeed = 0.5f;
    
    [Header("Skip Settings")]
    [Tooltip("Allow skipping fade-in effect with Space")]
    public bool allowSkip = true;
    
    private bool canContinue = false;
    private bool isBlinking = false;
    private bool isFading = false;
    
    void Start()
    {
        // Initialize all text to transparent
        foreach (var textLine in textLines)
        {
            if (textLine != null)
            {
                Color color = textLine.color;
                color.a = 0f;
                textLine.color = color;
            }
        }
        
        // Initialize continue text
        if (continueText != null)
        {
            continueText.text = continueMessage;
            Color color = continueText.color;
            color.a = 0f;
            continueText.color = color;
        }
        
        // Start the sequence
        StartCoroutine(PlayOpeningSequence());
    }
    
    void Update()
    {
        // Allow skipping all remaining fade-ins with Space
        if (allowSkip && isFading && Input.GetKeyDown(KeyCode.Space))
        {
            SkipAllFadeIns();
            return;
        }
        
        // Check for space key press to continue
        if (canContinue && Input.GetKeyDown(KeyCode.Space))
        {
            LoadNextScene();
        }
    }
    
    // Skip all remaining fade-ins and show continue prompt immediately
    private void SkipAllFadeIns()
    {
        // Stop all fade coroutines
        StopAllCoroutines();
        
        // Show all text lines immediately
        foreach (var textLine in textLines)
        {
            if (textLine != null)
            {
                Color color = textLine.color;
                color.a = 1f;
                textLine.color = color;
            }
        }
        
        isFading = false;
        
        // Show continue prompt immediately
        if (continueText != null)
        {
            canContinue = true;
            StartCoroutine(BlinkContinueText());
        }
        else
        {
            canContinue = true;
        }
    }
    
    private IEnumerator PlayOpeningSequence()
    {
        isFading = true;
        
        // Fade in each text line one by one
        foreach (var textLine in textLines)
        {
            if (textLine != null)
            {
                yield return StartCoroutine(FadeInText(textLine));
                yield return new WaitForSeconds(delayBetweenLines);
            }
        }
        
        isFading = false;
        
        // Wait before showing continue prompt
        yield return new WaitForSeconds(delayBeforeContinue);
        
        // Show and blink continue prompt
        if (continueText != null)
        {
            canContinue = true;
            StartCoroutine(BlinkContinueText());
        }
        else
        {
            // If no continue text, allow continuing immediately
            canContinue = true;
        }
    }
    
    private IEnumerator FadeInText(TextMeshProUGUI textComponent)
    {
        float elapsedTime = 0f;
        Color color = textComponent.color;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeInDuration);
            textComponent.color = color;
            yield return null;
        }
        
        // Ensure fully opaque
        color.a = 1f;
        textComponent.color = color;
    }
    
    private IEnumerator BlinkContinueText()
    {
        isBlinking = true;
        Color color = continueText.color;
        
        while (isBlinking)
        {
            // Fade in
            float elapsedTime = 0f;
            while (elapsedTime < blinkSpeed)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Clamp01(elapsedTime / blinkSpeed);
                continueText.color = color;
                yield return null;
            }
            
            // Fade out
            elapsedTime = 0f;
            while (elapsedTime < blinkSpeed)
            {
                elapsedTime += Time.deltaTime;
                color.a = 1f - Mathf.Clamp01(elapsedTime / blinkSpeed);
                continueText.color = color;
                yield return null;
            }
        }
    }
    
    private void LoadNextScene()
    {
        // Stop blinking
        isBlinking = false;
        canContinue = false;
        ResetAllState();
        
        // Destroy SettingsManager before loading MainMenu
        if (SettingsManager.Instance != null)
        {
            Destroy(SettingsManager.Instance.transform.root.gameObject);
        }
        
        // Load MainMenu directly (without transition since we destroyed it)
        SceneManager.LoadScene("MainMenu");
    }
    
    private void ResetAllState()
    {
        GlobalGameState.isLowBranching = false;
        GlobalGameState.lazerHitRobot = false;
        GlobalGameState.lazerHitRobot2 = false;
        GlobalGameState.dialogueActive = false;
        GlobalGameState.swallowNextSpace = false;
        GlobalGameState.isRobotHacked = false;
        GlobalGameState.isRobotHacked2 = false;
        GlobalGameState.isYellowHacked = false;
        GlobalGameState.isPurpleHacked = false;
        GlobalGameState.isWhiteHacked = false;
        GlobalGameState.isRobotSaved = false;
        GlobalGameState.isLevel0 = false;
        GlobalGameState.isLevel1 = false;
        GlobalGameState.isLevel2 = false;
        GlobalGameState.isLevel3 = false;
        GlobalGameState.isLevel4 = false;
        GlobalGameState.isLevel5 = false;
        GlobalGameState.isLevel6 = false;
        GlobalGameState.isLevel7 = false;
        GlobalGameState.isFinalLevel = false;
        GlobalGameState.HackAI = false;
        GlobalGameState.RootAI = false;
        GlobalGameState.spaceUIRobot = null;
        GlobalGameState.stateSaver = new bool[5];

        GlobalGameState.isEachLevelTalked = false;
        GlobalGameState.isEachLevelTalked2 = false;
        GlobalGameState.isEachLevelHacked = false;
        GlobalGameState.isEachLevelHacked2 = false;
        GlobalGameState.talkCount = 0;
        GlobalGameState.hackCount = 0;
        GlobalGameState.destroyCount = 0;
        GlobalGameState.dataCountSaver = new int[] { 0, 0, 0 };
    }
}