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
    
    // Store original volume of each AudioSource
    private Dictionary<AudioSource, float> originalVolumes = new Dictionary<AudioSource, float>();
    
    // Audio source tags
    private const string MUSIC_TAG = "Music";
    private const string SFX_TAG = "SFX";
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject); // Keep entire root object
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
        // Subscribe to scene load event
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Clear original volumes to ensure fresh start
        originalVolumes.Clear();
        
        // Apply volume to current scene
        ApplyVolumeToAllAudioSources();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Clear original volumes on each new scene
        // This ensures we record the fresh volume of new AudioSources
        originalVolumes.Clear();
        
        // After new scene loads, reapply volume settings
        ApplyVolumeToAllAudioSources();
    }
    
    // Set master volume
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
        ApplyVolumeToAllAudioSources();
    }
    
    // Set music volume
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
        ApplyVolumeToAllAudioSources();
    }
    
    // Set SFX volume
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveVolumeSettings();
        ApplyVolumeToAllAudioSources();
    }
    
    // Apply volume to all AudioSources
    private void ApplyVolumeToAllAudioSources()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        
        foreach (AudioSource audioSource in allAudioSources)
        {
            ApplyVolumeToSource(audioSource);
        }
    }
    
    // Apply volume to a single AudioSource
    private void ApplyVolumeToSource(AudioSource audioSource)
    {
        if (audioSource == null) return;
        
        // Record original volume (if not recorded yet)
        if (!originalVolumes.ContainsKey(audioSource))
        {
            originalVolumes[audioSource] = audioSource.volume;
        }
        
        // Apply different volumes based on Tag or loop status
        if (audioSource.CompareTag(MUSIC_TAG))
        {
            // Has Music tag
            audioSource.volume = originalVolumes[audioSource] * musicVolume * masterVolume;
        }
        else if (audioSource.CompareTag(SFX_TAG))
        {
            // Has SFX tag
            audioSource.volume = originalVolumes[audioSource] * sfxVolume * masterVolume;
        }
        else if (audioSource.loop)
        {
            // No tag but is looping - assume it's music
            audioSource.volume = originalVolumes[audioSource] * musicVolume * masterVolume;
        }
        else
        {
            // No tag, not looping - assume it's SFX
            audioSource.volume = originalVolumes[audioSource] * sfxVolume * masterVolume;
        }
    }
    
    // For dynamically created AudioSources - call this when creating new audio
    public void RegisterAudioSource(AudioSource audioSource, bool isMusic = false)
    {
        if (audioSource == null) return;
        
        // Record original volume
        if (!originalVolumes.ContainsKey(audioSource))
        {
            originalVolumes[audioSource] = audioSource.volume;
        }
        
        // Apply volume
        if (isMusic)
        {
            audioSource.volume = originalVolumes[audioSource] * musicVolume * masterVolume;
        }
        else
        {
            audioSource.volume = originalVolumes[audioSource] * sfxVolume * masterVolume;
        }
    }
    
    // Play one-shot audio with proper volume (for AudioSource.PlayClipAtPoint replacement)
    public static void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1.0f)
    {
        if (Instance == null || clip == null) return;
        
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = position;
        
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volumeScale * Instance.sfxVolume * Instance.masterVolume;
        audioSource.Play();
        
        Destroy(tempGO, clip.length);
    }
    
    // Play one-shot music with proper volume
    public static void PlayMusicAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1.0f)
    {
        if (Instance == null || clip == null) return;
        
        GameObject tempGO = new GameObject("TempMusic");
        tempGO.transform.position = position;
        
        AudioSource audioSource = tempGO.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volumeScale * Instance.musicVolume * Instance.masterVolume;
        audioSource.Play();
        
        Destroy(tempGO, clip.length);
    }
    
    // Save volume settings to PlayerPrefs
    private void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    // Load volume settings from PlayerPrefs
    private void LoadVolumeSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
    }
    
    // Clean up destroyed AudioSources from dictionary
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
    
    // Periodic cleanup (optional)
    void Update()
    {
        // Clean up every 5 seconds
        if (Time.frameCount % 300 == 0)
        {
            CleanupDestroyedAudioSources();
        }
    }
}