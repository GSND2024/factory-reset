using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class GlobalStateDisplay : MonoBehaviour
{
    public static GlobalStateDisplay Instance { get; private set; }
    
    [Header("UI References")]
    public Canvas stateCanvas;
    public TextMeshProUGUI stateDisplayText;
    
    [Header("Display Settings")]
    [Tooltip("Hide state display in these scenes")]
    public List<string> hiddenScenes = new List<string>();
    public int canvasSortOrder = 998; // Below settings menu (999)
    
    [Header("Update Settings")]
    [Tooltip("Update display every frame (recommended for real-time updates)")]
    public bool updateEveryFrame = true;
    
    private bool isVisibleInCurrentScene = true;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject); // Keep entire root object
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Setup canvas sort order
        SetupCanvas();
        
        // Subscribe to scene load event
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Initial update
        UpdateDisplay();
        CheckCurrentScene();
    }
    
    private void SetupCanvas()
    {
        if (stateCanvas != null)
        {
            stateCanvas.sortingOrder = canvasSortOrder;
            
            if (stateCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                stateCanvas.overrideSorting = true;
                stateCanvas.sortingOrder = canvasSortOrder;
            }
        }
    }
    
    void Update()
    {
        // Update display every frame for real-time changes
        if (updateEveryFrame)
        {
            UpdateDisplay();
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentScene();
        UpdateDisplay();
    }
    
    // Check if state display should be visible in current scene
    private void CheckCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        isVisibleInCurrentScene = !hiddenScenes.Contains(currentSceneName);
        
        // Show or hide the canvas
        if (stateCanvas != null)
        {
            stateCanvas.gameObject.SetActive(isVisibleInCurrentScene);
        }
    }
    
    // Update all text displays by reading from GlobalGameState
    public void UpdateDisplay()
    {
        
        // Update single text display with both values
        if (stateDisplayText != null)
        {
            stateDisplayText.text = $"Talk: {GlobalGameState.talkCount} Hack: {GlobalGameState.hackCount}";
        }
    }
    
    // Manual refresh (call this if updateEveryFrame is false)
    public void RefreshDisplay()
    {
        UpdateDisplay();
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}