using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro; // 添加TextMeshPro命名空间

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
    [Tooltip("确保Canvas的Sort Order最高")]
    public Canvas settingsCanvas;
    public int topSortOrder = 999;
    
    [Header("Scene Settings")]
    [Tooltip("在这些场景中禁用设置菜单")]
    public List<string> disabledScenes = new List<string>();
    
    private bool isSettingsOpen = false;
    private bool isEnabledInCurrentScene = true;
    
    void Awake()
    {
        // 单例模式 - 确保只有一个实例存在
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 场景切换时不销毁
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // 初始化 - 隐藏设置面板
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // 绑定返回按钮事件
        if (backButton != null)
        {
            backButton.onClick.AddListener(CloseSettings);
        }
        
        // 绑定音量滑块事件
        SetupVolumeSliders();
        
        // 设置Canvas的Sort Order确保在最上层
        SetupCanvas();
        
        // 订阅场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 检查当前场景
        CheckCurrentScene();
    }
    
    private void SetupVolumeSliders()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found! Please add AudioManager to the scene.");
            return;
        }
        
        // 设置滑块初始值（将0-1转换为0-100）
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
    
    // 更新音量显示文本
    private void UpdateVolumeText(TextMeshProUGUI volumeText, float value)
    {
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(value).ToString();
        }
    }
    
    // 音量滑块回调函数（将0-100转换回0-1）
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
    
    private void SetupCanvas()
    {
        // 如果没有手动分配Canvas，尝试自动获取
        if (settingsCanvas == null)
        {
            settingsCanvas = GetComponentInChildren<Canvas>();
        }
        
        if (settingsCanvas != null)
        {
            // 设置为最高的Sort Order
            settingsCanvas.sortingOrder = topSortOrder;
            
            // 确保使用正确的Render Mode
            if (settingsCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Overlay模式自动在最上层
                settingsCanvas.overrideSorting = true;
                settingsCanvas.sortingOrder = topSortOrder;
            }
        }
    }
    
    void Update()
    {
        // 只有在允许的场景中才能按ESC键切换设置面板
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
    
    // 场景加载时的回调
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentScene();
    }
    
    // 检查当前场景是否应该启用设置菜单
    private void CheckCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // 检查当前场景是否在禁用列表中
        isEnabledInCurrentScene = !disabledScenes.Contains(currentSceneName);
        
        // 如果在禁用的场景中，强制关闭设置面板
        if (!isEnabledInCurrentScene && isSettingsOpen)
        {
            CloseSettings();
        }
        
        // 隐藏或显示整个UI（可选）
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
            
            // 确保Canvas在最上层
            EnsureOnTop();
            
            // 暂停游戏时间，但不影响音频
            Time.timeScale = 0f;
            
            // 禁用玩家输入（如果有PlayerController脚本）
            DisablePlayerInput();
        }
    }
    
    // 确保Canvas始终在最上层
    private void EnsureOnTop()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.sortingOrder = topSortOrder;
            
            // 如果有其他Canvas，确保我们的在最上面
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
        
        // 确保SettingsPanel是Canvas的最后一个子对象（在Hierarchy中最下面 = 渲染在最上面）
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
            
            // 恢复游戏时间
            Time.timeScale = 1f;
            
            // 启用玩家输入
            EnablePlayerInput();
        }
    }
    
    private void DisablePlayerInput()
    {
        // 查找并禁用玩家控制器
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 禁用所有MonoBehaviour脚本（除了这个管理器）
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
        // 查找并启用玩家控制器
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
        // 清理事件监听
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(CloseSettings);
        }
        
        // 清理音量滑块事件
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
        
        // 取消订阅场景加载事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        // 确保退出时恢复时间缩放
        Time.timeScale = 1f;
    }
}