using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mainMenuCanvas;
    public GameObject settingsCanvas;
    public GameObject achievementCanvas;
    public GameObject creditsCanvas;
    
    [Header("Main Menu Buttons")]
    public Button startGameButton;
    public Button settingsButton;
    public Button achievementButton;
    public Button creditsButton;
    public Button quitButton;
    
    [Header("Settings - Volume Sliders")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    
    [Header("Settings - Volume Display Text")]
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;
    
    [Header("Back Buttons")]
    public Button settingsBackButton;
    public Button achievementBackButton;
    public Button creditsBackButton;
    
    [Header("Special Buttons")]
    public Button cleanButton; // Reset all endings
    
    [Header("Scene Settings")]
    [Tooltip("Index of the opening scene (OpenScene)")]
    public int openingSceneIndex = 1;
    
    void Start()
    {
        // Bind main menu buttons
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }
        
        if (settingsButton != null)
        {
            settingsButton.onClick.AddListener(OpenSettings);
        }
        
        if (achievementButton != null)
        {
            achievementButton.onClick.AddListener(OpenAchievements);
        }
        
        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OpenCredits);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
        
        // Bind back buttons
        if (settingsBackButton != null)
        {
            settingsBackButton.onClick.AddListener(CloseSettings);
        }
        
        if (achievementBackButton != null)
        {
            achievementBackButton.onClick.AddListener(CloseAchievements);
        }
        
        if (creditsBackButton != null)
        {
            creditsBackButton.onClick.AddListener(CloseCredits);
        }
        
        if (cleanButton != null)
        {
            cleanButton.onClick.AddListener(CleanAllEndings);
        }
        
        // Setup volume sliders
        SetupVolumeSliders();
        
        // Show main menu by default
        ShowMainMenu();
    }
    
    private void SetupVolumeSliders()
    {
        // Load volume settings from PlayerPrefs (same as AudioManager)
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        
        // Set slider initial values (convert 0-1 to 0-100)
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = masterVolume * 100f;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            UpdateVolumeText(masterVolumeText, masterVolumeSlider.value);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume * 100f;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            UpdateVolumeText(musicVolumeText, musicVolumeSlider.value);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVolume * 100f;
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
    
    // Volume slider callback functions
    private void OnMasterVolumeChanged(float value)
    {
        float normalizedValue = value / 100f;
        
        // If AudioManager exists (shouldn't in MainMenu, but just in case)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(normalizedValue);
        }
        else
        {
            // Save directly to PlayerPrefs
            PlayerPrefs.SetFloat("MasterVolume", normalizedValue);
            PlayerPrefs.Save();
            ApplyVolumeToMainMenuAudio();
        }
        
        UpdateVolumeText(masterVolumeText, value);
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        float normalizedValue = value / 100f;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(normalizedValue);
        }
        else
        {
            PlayerPrefs.SetFloat("MusicVolume", normalizedValue);
            PlayerPrefs.Save();
            ApplyVolumeToMainMenuAudio();
        }
        
        UpdateVolumeText(musicVolumeText, value);
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        float normalizedValue = value / 100f;
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(normalizedValue);
        }
        else
        {
            PlayerPrefs.SetFloat("SFXVolume", normalizedValue);
            PlayerPrefs.Save();
            ApplyVolumeToMainMenuAudio();
        }
        
        UpdateVolumeText(sfxVolumeText, value);
    }
    
    // Apply volume to MainMenu audio sources
    private void ApplyVolumeToMainMenuAudio()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            // Check if it's music (looping) or SFX
            if (audioSource.loop)
            {
                audioSource.volume = musicVolume * masterVolume;
            }
            else
            {
                audioSource.volume = sfxVolume * masterVolume;
            }
        }
    }
    
    private void ShowMainMenu()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
        if (achievementCanvas != null) achievementCanvas.SetActive(false);
        if (creditsCanvas != null) creditsCanvas.SetActive(false);
    }
    
    public void StartGame()
    {
        // Use scene transition if available
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadSceneWithFade(openingSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(openingSceneIndex);
        }
    }
    
    public void OpenSettings()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (settingsCanvas != null) settingsCanvas.SetActive(true);
    }
    
    public void CloseSettings()
    {
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        
        // Refresh ending display after closing settings
        EndingDisplayUI endingDisplay = FindObjectOfType<EndingDisplayUI>();
        if (endingDisplay != null)
        {
            endingDisplay.UpdateEndingDisplay();
        }
    }
    
    public void OpenAchievements()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (achievementCanvas != null) achievementCanvas.SetActive(true);
    }
    
    public void CloseAchievements()
    {
        if (achievementCanvas != null) achievementCanvas.SetActive(false);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
    }
    
    public void OpenCredits()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
        if (creditsCanvas != null) creditsCanvas.SetActive(true);
    }
    
    public void CloseCredits()
    {
        if (creditsCanvas != null) creditsCanvas.SetActive(false);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
    }
    
    public void QuitGame()
    {
        PlayerPrefs.Save();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void CleanAllEndings()
    {
        // Reset all endings
        EndingTracker.ResetAllEndings();
        
        // Find and refresh EndingDisplayUI
        EndingDisplayUI endingDisplay = FindObjectOfType<EndingDisplayUI>();
        if (endingDisplay != null)
        {
            endingDisplay.UpdateEndingDisplay();
        }
        
        Debug.Log("All endings have been reset!");
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (startGameButton != null) startGameButton.onClick.RemoveListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
        if (achievementButton != null) achievementButton.onClick.RemoveListener(OpenAchievements);
        if (creditsButton != null) creditsButton.onClick.RemoveListener(OpenCredits);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
        if (settingsBackButton != null) settingsBackButton.onClick.RemoveListener(CloseSettings);
        if (achievementBackButton != null) achievementBackButton.onClick.RemoveListener(CloseAchievements);
        if (creditsBackButton != null) creditsBackButton.onClick.RemoveListener(CloseCredits);
        if (cleanButton != null) cleanButton.onClick.RemoveListener(CleanAllEndings);
        
        // Clean up volume slider listeners
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }
}