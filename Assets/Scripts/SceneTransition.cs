using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }
    
    [Header("UI References")]
    public Canvas transitionCanvas;
    public Image fadeImage;
    
    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public Color fadeColor = Color.black;
    
    private bool isFading = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        SetupCanvas();
        
        // Start with transparent
        if (fadeImage != null)
        {
            Color color = fadeColor;
            color.a = 0f;
            fadeImage.color = color;
        }
        
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Fade in when game starts
        StartCoroutine(FadeIn());
    }
    
    // Called every time a scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Fade in after scene loads
        StartCoroutine(FadeIn());
    }
    
    private void SetupCanvas()
    {
        if (transitionCanvas != null)
        {
            // Highest sort order to be above everything
            transitionCanvas.sortingOrder = 1000;
            
            if (transitionCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                transitionCanvas.overrideSorting = true;
                transitionCanvas.sortingOrder = 1000;
            }
        }
        
        if (fadeImage != null)
        {
            fadeImage.color = fadeColor;
            fadeImage.raycastTarget = false; // Don't block clicks when transparent
        }
    }
    
    // Fade to black (fade out)
    public IEnumerator FadeOut()
    {
        if (isFading || fadeImage == null) yield break;
        
        isFading = true;
        float elapsedTime = 0f;
        Color color = fadeColor;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to work even when Time.timeScale = 0
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 1f;
        fadeImage.color = color;
        isFading = false;
    }
    
    // Fade from black (fade in)
    public IEnumerator FadeIn()
    {
        if (isFading || fadeImage == null) yield break;
        
        isFading = true;
        float elapsedTime = 0f;
        Color color = fadeColor;
        color.a = 1f;
        fadeImage.color = color;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        
        color.a = 0f;
        fadeImage.color = color;
        isFading = false;
    }
    
    // Restart current scene with fade
    public void RestartSceneWithFade()
    {
        if (!isFading)
        {
            StartCoroutine(RestartSceneCoroutine());
        }
    }
    
    private IEnumerator RestartSceneCoroutine()
    {
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Load scene
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.buildIndex);
        
        // Fade in will happen automatically in OnSceneLoaded()
    }
    
    // Load specific scene with fade
    public void LoadSceneWithFade(int sceneIndex)
    {
        if (!isFading)
        {
            StartCoroutine(LoadSceneCoroutine(sceneIndex));
        }
    }
    
    public void LoadSceneWithFade(string sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }
    }
    
    private IEnumerator LoadSceneCoroutine(int sceneIndex)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneIndex);
    }
    
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}