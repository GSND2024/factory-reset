using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class GlobalStateDisplay : MonoBehaviour
{
    public static GlobalStateDisplay Instance { get; private set; }
    
    [Header("UI References")]
    public Canvas stateCanvas;
    public Image talkImage;
    public Image hackImage;
    
    [Header("Number Sprites (000.png to 008.png)")]
    public Sprite[] numberSprites = new Sprite[9];
    
    [Header("Color Settings")]
    public Color normalColor = Color.white;
    public Color talkMaxColor = new Color(0.7f, 0.9f, 1f, 1f); // Light blue
    public Color hackMaxColor = new Color(1f, 0.7f, 0.7f, 1f); // Light red
    
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
    
    // Update image displays by reading from GlobalGameState
    public void UpdateDisplay()
    {
        
        // Update Talk image
        int talkCount = Mathf.Clamp(GlobalGameState.talkCount, 0, 8);
        if (talkImage != null && numberSprites.Length > talkCount && numberSprites[talkCount] != null)
        {
            talkImage.sprite = numberSprites[talkCount];
            // Change color to light blue when count is 8
            talkImage.color = (talkCount == 8) ? talkMaxColor : normalColor;
        }
        
        // Update Hack image
        int hackCount = Mathf.Clamp(GlobalGameState.hackCount, 0, 8);
        if (hackImage != null && numberSprites.Length > hackCount && numberSprites[hackCount] != null)
        {
            hackImage.sprite = numberSprites[hackCount];
            // Change color to light red when count is 8
            hackImage.color = (hackCount == 8) ? hackMaxColor : normalColor;
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