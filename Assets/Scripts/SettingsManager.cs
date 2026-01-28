using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }
    
    [Header("UI References")]
    public GameObject settingsPanel;
    public Button backButton;
    
    [Header("Volume Sliders")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    
    [Header("Volume Display Text")]
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    
    [Header("Canvas Settings")]
    [Tooltip("Ensure Canvas has the highest Sort Order")]
    public Canvas settingsCanvas;
    public int topSortOrder = 999;
    
    [Header("Scene Settings")]
    [Tooltip("Disable settings menu in these scenes")]
    public List<string> disabledScenes = new List<string>();
    
    private bool isSettingsOpen = false;
    private bool isEnabledInCurrentScene = true;
    
    void Awake()
    {
        // Singleton pattern - ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Don't destroy on scene load
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Initialize - hide settings panel
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Bind back button event
        if (backButton != null)
        {
            backButton.onClick.AddListener(CloseSettings);
        }
        
        // Bind volume slider events
        SetupVolumeSliders();
        
        // Setup Canvas Sort Order to ensure it's on top
        SetupCanvas();
        
        // Subscribe to scene load event
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Check current scene
        CheckCurrentScene();
    }
    
    private void SetupCanvas()
    {
        // If Canvas not manually assigned, try to get it automatically
        if (settingsCanvas == null)
        {
            settingsCanvas = GetComponentInChildren<Canvas>();
        }
        
        if (settingsCanvas != null)
        {
            // Set to highest Sort Order
            settingsCanvas.sortingOrder = topSortOrder;
            
            // Ensure using correct Render Mode
            if (settingsCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Overlay mode is automatically on top
                settingsCanvas.overrideSorting = true;
                settingsCanvas.sortingOrder = topSortOrder;
            }
        }
    }
    
    private void SetupVolumeSliders()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found! Please add AudioManager to the scene.");
            return;
        }
        
        // Set slider initial values (convert 0-1 to 0-100)
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioManager.Instance.masterVolume * 100f;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            UpdateVolumeText(masterVolumeText, masterVolumeSlider.value);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = AudioManager.Instance.musicVolume * 100f;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            UpdateVolumeText(musicVolumeText, musicVolumeSlider.value);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = AudioManager.Instance.sfxVolume * 100f;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            UpdateVolumeText(sfxVolumeText, sfxVolumeSlider.value);
        }
    }
    
    // Update volume display text
    private void UpdateVolumeText(TextMeshProUGUI volumeText, float value)
    {
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(value).ToString();
        }
    }
    
    // Volume slider callback functions (convert 0-100 back to 0-1)
    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value / 100f);
            UpdateVolumeText(masterVolumeText, value);
        }
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value / 100f);
            UpdateVolumeText(musicVolumeText, value);
        }
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value / 100f);
            UpdateVolumeText(sfxVolumeText, value);
        }
    }
    
    void Update()
    {
        // Only allow ESC key to toggle settings panel in enabled scenes
        if (!isEnabledInCurrentScene)
            return;
            
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isSettingsOpen)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
    }
    
    // Callback when scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentScene();
    }
    
    // Check if settings menu should be enabled in current scene
    private void CheckCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // Check if current scene is in disabled list
        isEnabledInCurrentScene = !disabledScenes.Contains(currentSceneName);
        
        // If in disabled scene, force close settings panel
        if (!isEnabledInCurrentScene && isSettingsOpen)
        {
            CloseSettings();
        }
        
        // Hide or show entire UI (optional)
        if (settingsPanel != null && settingsPanel.transform.parent != null)
        {
            settingsPanel.transform.parent.gameObject.SetActive(isEnabledInCurrentScene);
        }
    }
    
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            isSettingsOpen = true;
            
            // Ensure Canvas is on top
            EnsureOnTop();
            
            // Pause game time, but don't affect audio
            Time.timeScale = 0f;
            
            // Disable player input (if PlayerController script exists)
            DisablePlayerInput();
        }
    }
    
    // Ensure Canvas is always on top
    private void EnsureOnTop()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.sortingOrder = topSortOrder;
            
            // If there are other Canvases, ensure ours is on top
            Canvas[] allCanvases = FindObjectsOfType<Canvas>();
            int maxOrder = topSortOrder;
            foreach (Canvas c in allCanvases)
            {
                if (c != settingsCanvas && c.sortingOrder >= maxOrder)
                {
                    maxOrder = c.sortingOrder + 1;
                }
            }
            settingsCanvas.sortingOrder = maxOrder;
        }
        
        // Ensure SettingsPanel is last child in Canvas (bottom in Hierarchy = renders on top)
        if (settingsPanel != null)
        {
            settingsPanel.transform.SetAsLastSibling();
        }
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            isSettingsOpen = false;
            
            // Resume game time
            Time.timeScale = 1f;
            
            // Enable player input
            EnablePlayerInput();
        }
    }
    
    private void DisablePlayerInput()
    {
        // Find and disable player controller
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Disable all MonoBehaviour scripts (except this manager)
            var scripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script != null && script.enabled)
                {
                    script.enabled = false;
                }
            }
        }
    }
    
    private void EnablePlayerInput()
    {
        // Find and enable player controller
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var scripts = player.GetComponents<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up event listeners
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(CloseSettings);
        }
        
        // Clean up volume slider events
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }
        
        // Unsubscribe from scene load event
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // Ensure time scale is restored on exit
        Time.timeScale = 1f;
    }
}