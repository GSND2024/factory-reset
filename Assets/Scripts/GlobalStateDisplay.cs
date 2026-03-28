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
    public Color talkNormalColor = new Color(0.7f, 0.9f, 1f, 1f);   // 浅蓝色
    public Color talkMaxColor = new Color(0.2f, 0.5f, 0.9f, 1f);    // 深蓝色
    public Color hackNormalColor = new Color(1f, 0.7f, 0.7f, 1f);   // 浅红色
    public Color hackMaxColor = new Color(0.9f, 0.2f, 0.2f, 1f);    // 深红色
    
    [Header("Display Settings")]
    [Tooltip("Hide state display in these scenes")]
    public List<string> hiddenScenes = new List<string>();
    public int canvasSortOrder = 998;
    
    [Header("Sound Settings")]
    public AudioClip maxCountSound;
    [Range(0f, 1f)]
    public float soundVolume = 1f;
    
    [Header("Update Settings")]
    [Tooltip("Update display every frame (recommended for real-time updates)")]
    public bool updateEveryFrame = true;
    
    private bool isVisibleInCurrentScene = true;
    private int lastTalkCount = -1;
    private int lastHackCount = -1;
    
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
        SceneManager.sceneLoaded += OnSceneLoaded;
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
    
    private void CheckCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        isVisibleInCurrentScene = !hiddenScenes.Contains(currentSceneName);
        
        if (stateCanvas != null)
        {
            stateCanvas.gameObject.SetActive(isVisibleInCurrentScene);
        }
    }
    
    public void UpdateDisplay()
    {
        // Update Talk image：正常浅蓝，满8深蓝
        int talkCount = Mathf.Clamp(GlobalGameState.talkCount, 0, 8);
        if (talkImage != null && numberSprites.Length > talkCount && numberSprites[talkCount] != null)
        {
            talkImage.sprite = numberSprites[talkCount];
            talkImage.color = (talkCount == 8) ? talkMaxColor : talkNormalColor;
        }
        if (talkCount == 8 && lastTalkCount != 8)
            PlayMaxSound();
        lastTalkCount = talkCount;
        
        // Update Hack image：正常浅红，满8深红
        int hackCount = Mathf.Clamp(GlobalGameState.hackCount, 0, 8);
        if (hackImage != null && numberSprites.Length > hackCount && numberSprites[hackCount] != null)
        {
            hackImage.sprite = numberSprites[hackCount];
            hackImage.color = (hackCount == 8) ? hackMaxColor : hackNormalColor;
        }
        if (hackCount == 8 && lastHackCount != 8)
            PlayMaxSound();
        lastHackCount = hackCount;
    }
    
    private void PlayMaxSound()
    {
        if (maxCountSound != null)
            AudioManager.PlaySFXAtPoint(maxCountSound, Camera.main.transform.position, soundVolume);
    }
    
    public void RefreshDisplay()
    {
        UpdateDisplay();
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}