using UnityEngine;
using UnityEngine.SceneManagement;

public class ConditionalDoor : MonoBehaviour
{
    [Header("Sprite References")]
    [Tooltip("Closed door sprite (default)")]
    public Sprite closedSprite;
    
    [Tooltip("Open door sprite (when all counts are 0)")]
    public Sprite openSprite;
    
    [Header("Scene Settings")]
    [Tooltip("Scene index to load when player enters door")]
    public int targetSceneIndex = 10; // PrototypeLevel5
    
    [Header("Components (Auto-assigned if empty)")]
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;
    
    [Header("Optional Settings")]
    [Tooltip("Show debug messages")]
    public bool showDebugMessages = false;
    
    private bool isDoorOpen = false;
    
    void Awake()
    {
        // Auto-assign components if not set
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider2D>();
        }
        
        // Store closed sprite if not set
        if (closedSprite == null && spriteRenderer != null)
        {
            closedSprite = spriteRenderer.sprite;
        }
    }
    
    void Start()
    {
        // Initialize to closed state
        SetDoorState(false);
        
        // Check condition at start
        CheckAndOpenDoor();
    }
    
    void Update()
    {
        // Continuously check condition
        CheckAndOpenDoor();
    }
    
    private void CheckAndOpenDoor()
    {
        
        // Check if all counts are 0
        bool shouldOpen = GlobalGameState.talkCount == 0 
                       && GlobalGameState.hackCount == 0 
                       && GlobalGameState.destroyCount == 0;
        
        // Update door state if changed
        if (shouldOpen != isDoorOpen)
        {
            isDoorOpen = shouldOpen;
            SetDoorState(isDoorOpen);
            
            if (showDebugMessages)
            {
                if (isDoorOpen)
                {
                    Debug.Log($"Door opened! All counts are 0");
                }
                else
                {
                    Debug.Log($"Door closed! Counts - Talk:{GlobalGameState.talkCount}, Hack:{GlobalGameState.hackCount}, Destroy:{GlobalGameState.destroyCount}");
                }
            }
        }
    }
    
    private void SetDoorState(bool open)
    {
        // Change sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = open ? openSprite : closedSprite;
        }
        
        // Change collider trigger state
        if (boxCollider != null)
        {
            boxCollider.isTrigger = open;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger if door is open and player enters
        if (isDoorOpen && other.CompareTag("Player"))
        {
            if (showDebugMessages)
            {
                Debug.Log($"Player entered door. Loading scene {targetSceneIndex}");
            }
            
            LoadTargetScene();
        }
    }
    
    private void LoadTargetScene()
    {
        // Use scene transition if available
        if (SceneTransition.Instance != null)
        {
            SceneTransition.Instance.LoadSceneWithFade(targetSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(targetSceneIndex);
        }
    }
    
    // Public method to check if door is open
    public bool IsDoorOpen()
    {
        return isDoorOpen;
    }
}