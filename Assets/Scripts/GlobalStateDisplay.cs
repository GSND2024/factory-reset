using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

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
    public Color talkNormalColor = new Color(0.7f, 0.9f, 1f, 1f);
    public Color talkMaxColor = new Color(0.2f, 0.5f, 0.9f, 1f);
    public Color hackNormalColor = new Color(1f, 0.7f, 0.7f, 1f);
    public Color hackMaxColor = new Color(0.9f, 0.2f, 0.2f, 1f);

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

    [Header("Gain Animation Settings")]
    [Tooltip("Play center-to-corner animation when a count increases")]
    public bool animateOnIncrease = true;

    [Tooltip("How long the image stays paused in the center before moving")]
    public float holdDuration = 0.2f;

    [Tooltip("How long the fly animation lasts after the pause")]
    public float flyDuration = 0.45f;

    [Tooltip("Scale of the temporary image at the center of the screen")]
    public float startScale = 2.2f;

    [Tooltip("Scale of the temporary image when it reaches the counter")]
    public float endScale = 1f;

    [Tooltip("Optional fade out during the fly")]
    public bool fadeDuringFly = false;

    [Tooltip("Use unscaled time so animation still works if timescale changes")]
    public bool useUnscaledTime = true;

    private bool isVisibleInCurrentScene = true;
    private int lastTalkCount = -1;
    private int lastHackCount = -1;

    private RectTransform canvasRect;

    private Coroutine talkAnimCoroutine;
    private Coroutine hackAnimCoroutine;

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

            canvasRect = stateCanvas.GetComponent<RectTransform>();
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
        SetupCanvas();
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
        int talkCount = Mathf.Clamp(GlobalGameState.talkCount, 0, 8);
        int hackCount = Mathf.Clamp(GlobalGameState.hackCount, 0, 8);

        bool talkIncreased = lastTalkCount >= 0 && talkCount > lastTalkCount;
        bool hackIncreased = lastHackCount >= 0 && hackCount > lastHackCount;

        // Update the real HUD images first
        if (talkImage != null && numberSprites.Length > talkCount && numberSprites[talkCount] != null)
        {
            talkImage.sprite = numberSprites[talkCount];
            talkImage.color = (talkCount == 8) ? talkMaxColor : talkNormalColor;
        }

        if (hackImage != null && numberSprites.Length > hackCount && numberSprites[hackCount] != null)
        {
            hackImage.sprite = numberSprites[hackCount];
            hackImage.color = (hackCount == 8) ? hackMaxColor : hackNormalColor;
        }

        // Play animation only on increase
        if (animateOnIncrease && isVisibleInCurrentScene)
        {
            if (talkIncreased)
            {
                if (talkAnimCoroutine != null)
                    StopCoroutine(talkAnimCoroutine);

                if (talkImage != null)
                    talkImage.enabled = true;

                talkAnimCoroutine = StartCoroutine(AnimateCounterGain(
                    numberSprites[talkCount],
                    (talkCount == 8) ? talkMaxColor : talkNormalColor,
                    talkImage
                ));
            }

            if (hackIncreased)
            {
                if (hackAnimCoroutine != null)
                    StopCoroutine(hackAnimCoroutine);

                if (hackImage != null)
                    hackImage.enabled = true;

                hackAnimCoroutine = StartCoroutine(AnimateCounterGain(
                    numberSprites[hackCount],
                    (hackCount == 8) ? hackMaxColor : hackNormalColor,
                    hackImage
                ));
            }
        }

        if (talkCount == 8 && lastTalkCount != 8)
            PlayMaxSound();

        if (hackCount == 8 && lastHackCount != 8)
            PlayMaxSound();

        lastTalkCount = talkCount;
        lastHackCount = hackCount;
    }

    private IEnumerator AnimateCounterGain(Sprite sprite, Color color, Image targetImage)
    {
        if (stateCanvas == null || canvasRect == null || targetImage == null || sprite == null)
            yield break;

        // Hide the real HUD image during animation
        targetImage.enabled = false;

        GameObject tempObj = new GameObject("FlyingCounterImage");
        tempObj.transform.SetParent(stateCanvas.transform, false);

        RectTransform tempRect = tempObj.AddComponent<RectTransform>();
        Image tempImage = tempObj.AddComponent<Image>();

        tempImage.sprite = sprite;
        tempImage.color = color;
        tempImage.raycastTarget = false;
        tempImage.preserveAspect = true;

        RectTransform targetRect = targetImage.rectTransform;
        tempRect.sizeDelta = targetRect.rect.size;

        // Start in center of canvas
        tempRect.anchoredPosition = Vector2.zero;
        tempRect.localScale = Vector3.one * startScale;

        // Hold in center
        float holdElapsed = 0f;
        while (holdElapsed < holdDuration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            holdElapsed += dt;
            yield return null;
        }

        // Fly to target
        Vector2 startPos = tempRect.anchoredPosition;
        Vector2 endPos = GetAnchoredPositionInCanvas(targetRect);

        Vector3 startScaleVec = Vector3.one * startScale;
        Vector3 endScaleVec = Vector3.one * endScale;

        float flyElapsed = 0f;
        Color startColor = tempImage.color;

        while (flyElapsed < flyDuration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            flyElapsed += dt;

            float t = Mathf.Clamp01(flyElapsed / flyDuration);
            float eased = EaseOutCubic(t);

            tempRect.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            tempRect.localScale = Vector3.Lerp(startScaleVec, endScaleVec, eased);

            if (fadeDuringFly)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, eased);
                tempImage.color = c;
            }

            yield return null;
        }

        tempRect.anchoredPosition = endPos;
        tempRect.localScale = endScaleVec;

        // Re-show the real HUD image once animation completes
        if (targetImage != null)
            targetImage.enabled = true;

        Destroy(tempObj);
    }

    private Vector2 GetAnchoredPositionInCanvas(RectTransform targetRect)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
            stateCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : stateCanvas.worldCamera,
            targetRect.position
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            stateCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : stateCanvas.worldCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }

    private float EaseOutCubic(float t)
    {
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private void PlayMaxSound()
    {
        if (maxCountSound != null && Camera.main != null)
            AudioManager.PlaySFXAtPoint(maxCountSound, Camera.main.transform.position, soundVolume);
    }

    public void RefreshDisplay()
    {
        UpdateDisplay();
    }

    void OnDestroy()
    {
        if (talkImage != null)
            talkImage.enabled = true;

        if (hackImage != null)
            hackImage.enabled = true;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}