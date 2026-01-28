using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 0.5f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.5f;
    
    // 存储每个AudioSource的原始音量
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();
    
    // 音频源的标签
    private const string MUSIC_TAG = "Music";
    private const string SFX_TAG = "SFX";
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject); // 保持整个根对象
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // 订阅场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 应用音量到当前场景
        ApplyVolumeToAllAudioSources();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 新场景加载后，重新应用音量设置
        ApplyVolumeToAllAudioSources();
    }
    
    // 设置总音量
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
        ApplyVolumeToAllAudioSources();
    }
    
    // 设置音乐音量
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
        ApplyVolumeToAllAudioSources();
    }
    
    // 设置音效音量
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
        ApplyVolumeToAllAudioSources();
    }
    
    // 应用音量到所有AudioSource
    private void ApplyVolumeToAllAudioSources()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            // 记录原始音量（如果还没记录）
            if (!originalVolumes.ContainsKey(audioSource))
            {
                originalVolumes[audioSource] = audioSource.volume;
            }
            
            // 根据Tag应用不同的音量
            if (audioSource.CompareTag(MUSIC_TAG))
            {
                audioSource.volume = originalVolumes[audioSource] * musicVolume * masterVolume;
            }
            else if (audioSource.CompareTag(SFX_TAG))
            {
                audioSource.volume = originalVolumes[audioSource] * sfxVolume * masterVolume;
            }
            else
            {
                // 如果没有标签，默认作为SFX处理
                audioSource.volume = originalVolumes[audioSource] * sfxVolume * masterVolume;
            }
        }
    }
    
    // 为新创建的AudioSource应用音量
    public void RegisterAudioSource(AudioSource audioSource, bool isMusic = false)
    {
        if (audioSource == null) return;
        
        // 记录原始音量
        if (!originalVolumes.ContainsKey(audioSource))
        {
            originalVolumes[audioSource] = audioSource.volume;
        }
        
        // 应用音量
        if (isMusic)
        {
            audioSource.volume = originalVolumes[audioSource] * musicVolume * masterVolume;
        }
        else
        {
            audioSource.volume = originalVolumes[audioSource] * sfxVolume * masterVolume;
        }
    }
    
    // 保存音量设置到PlayerPrefs
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    // 从PlayerPrefs加载音量设置
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    }
    
    // 清理字典中已销毁的AudioSource
    private void CleanupDestroyedAudioSources()
    {
        List<AudioSource> toRemove = new List<AudioSource>();
        foreach (var key in originalVolumes.Keys)
        {
            if (key == null)
            {
                toRemove.Add(key);
            }
        }
        
        foreach (var key in toRemove)
        {
            originalVolumes.Remove(key);
        }
    }
    
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // 定期清理（可选）
    void Update()
    {
        // 每5秒清理一次
        if (Time.frameCount % 300 == 0)
        {
            CleanupDestroyedAudioSources();
        }
    }
}