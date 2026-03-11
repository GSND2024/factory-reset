using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Canvas References")]
    public GameObject mainMenuCanvas;
    public GameObject mainMenuSettingsCanvas; // MainMenu自己的Settings界面
    public GameObject achievementCanvas;
    public GameObject creditsCanvas;
    
    [Header("Buttons")]
    public Button startGameButton;
    public Button settingsButton;
    public Button achievementButton;
    public Button creditsButton;
    public Button quitButton;
    
    [Header("Back Buttons")]
    public Button mainMenuSettingsBackButton;
    public Button achievementBackButton;
    public Button creditsBackButton;
    
    [Header("Scene Settings")]
    [Tooltip("Index of the first level scene in Build Settings")]
    public int firstLevelSceneIndex = 1;
    
    void Start()
    {
        // Bind button events
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }
        
        if (settingsButton != null)
        {
            //settingsButton.onClick.AddListener(OpenSettings);
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
        
        if (achievementBackButton != null)
        {
            achievementBackButton.onClick.AddListener(CloseAchievements);
        }
        
        if (creditsBackButton != null)
        {
            creditsBackButton.onClick.AddListener(CloseCredits);
        }
        
        if (mainMenuSettingsBackButton != null)
        {
            mainMenuSettingsBackButton.onClick.AddListener(CloseMainMenuSettings);
        }
        
        // Show main menu by default
        ShowMainMenu();
    }
    
    private void ShowMainMenu()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        if (mainMenuSettingsCanvas != null) mainMenuSettingsCanvas.SetActive(false);
        if (achievementCanvas != null) achievementCanvas.SetActive(false);
        if (creditsCanvas != null) creditsCanvas.SetActive(false);
    }
    
    public void StartGame()
    {
        // Use scene transition if available
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadSceneWithFade(firstLevelSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(firstLevelSceneIndex);
        }
    }
    
    public void OpenSettings()
    {
        // Try to use global SettingsManager if it exists (during gameplay)
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OpenSettings();
        }
        // Otherwise use MainMenu's own settings canvas
        else if (mainMenuSettingsCanvas != null)
        {
            if (mainMenuCanvas != null) mainMenuCanvas.SetActive(false);
            mainMenuSettingsCanvas.SetActive(true);
        }
        else
        {
            Debug.LogWarning("No settings available!");
        }
    }
    
    public void CloseMainMenuSettings()
    {
        if (mainMenuSettingsCanvas != null) mainMenuSettingsCanvas.SetActive(false);
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
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
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (startGameButton != null) startGameButton.onClick.RemoveListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
        if (achievementButton != null) achievementButton.onClick.RemoveListener(OpenAchievements);
        if (creditsButton != null) creditsButton.onClick.RemoveListener(OpenCredits);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
        if (achievementBackButton != null) achievementBackButton.onClick.RemoveListener(CloseAchievements);
        if (creditsBackButton != null) creditsBackButton.onClick.RemoveListener(CloseCredits);
        if (mainMenuSettingsBackButton != null) mainMenuSettingsBackButton.onClick.RemoveListener(CloseMainMenuSettings);
    }
}