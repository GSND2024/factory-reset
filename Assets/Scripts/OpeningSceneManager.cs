using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class OpeningSceneManager : MonoBehaviour
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
    
    [Header("Next Scene")]
    [Tooltip("Index of the next scene to load")]
    public int nextSceneIndex = 2; // PrototypeLevel0
    
    private bool canContinue = false;
    private bool isBlinking = false;
    
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
        // Check for space key press
        if (canContinue && Input.GetKeyDown(KeyCode.Space))
        {
            LoadNextScene();
        }
    }
    
    private IEnumerator PlayOpeningSequence()
    {
        // Fade in each text line one by one
        foreach (var textLine in textLines)
        {
            if (textLine != null)
            {
                yield return StartCoroutine(FadeInText(textLine));
                yield return new WaitForSeconds(delayBetweenLines);
            }
        }
        
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
        
        // Load next scene with transition if available
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadSceneWithFade(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }
}