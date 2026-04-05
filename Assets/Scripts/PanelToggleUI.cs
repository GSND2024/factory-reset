using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PanelToggleUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Space;
    [SerializeField] private GameObject panelRoot;     // Talk/Hack panel root
    [SerializeField] private Actions actions;          // Actions component on the panel
    [SerializeField] private GameObject spaceUI;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GridMovement playerMovement;

    [Header("Interaction Rules")]
    [SerializeField] private string npcTag = "Robot";
    [SerializeField] private float gridSize = 1f;
    [Tooltip("World-space origin of your grid. If your grid doesn't start at (0,0), set this to the bottom-left/world origin of your board.")]
    [SerializeField] private Vector2 gridOrigin = Vector2.zero;

    [Header("World Space Panel Position")]
    [SerializeField] private Transform panelTransform;   // Usually ActionMenu transform
    [SerializeField] private Camera mainCam;
    [SerializeField] private float verticalOffsetTiles = 1.5f;
    [SerializeField] private float edgePaddingTiles = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [SerializeField] private float probeRadius = 0.22f;

    [SerializeField] private GridMovement owner;
    public static bool IsPanelOpen { get; private set; }

    private bool _isOpen;
    private GridMovement _currentTarget;

    private void Awake()
    {
        if (panelRoot != null && panelTransform == null)
            panelTransform = panelRoot.transform;

        if (panelRoot != null && actions == null)
            actions = panelRoot.GetComponent<Actions>();

        if (mainCam == null)
            mainCam = Camera.main;
    }

    private void Update()
    {
        if (HackManager.IsHacking || HackManager.SuppressUI)
        {
            if (_isOpen) ClosePanel("[PTUI] Closing because hacking/suppress is active.");
            return;
        }

        // Shuchen fix here:
        var best = FindAdjacentTaggedNPC();

        if (best && best == owner)
        {
            GameObject robot = best.gameObject;
            GlobalGameState.spaceUIRobot = robot;

            if (spaceUI != null)
            {
                spaceUI.SetActive(true);
            }
        }
        else
        {
            if (spaceUI != null)
            {
                spaceUI.SetActive(false);
            }
            if (GlobalGameState.spaceUIRobot == owner?.gameObject)
            {
                GlobalGameState.spaceUIRobot = null;
            }
        }

        if (Input.GetKeyDown(toggleKey) && !GlobalGameState.dialogueActive)
        {
            if (GlobalGameState.isFinalLevel &&
                (GlobalGameState.talkCount == 0 & GlobalGameState.hackCount == 0 & GlobalGameState.destroyCount == 0))
            {
                Debug.Log("Nothing happened");
            }
            else
            {
                var target = FindAdjacentTaggedNPC();
                if (target == null)
                {
                    if (_isOpen) ClosePanel("[PTUI] No NPC nearby.");
                    return;
                }

                // Only the panel whose owner IS the target handles this press
                if (target != owner) return;

                if (!_isOpen)
                {
                    if (actions == null && panelRoot != null)
                        actions = panelRoot.GetComponent<Actions>();

                    if (actions == null)
                    {
                        Debug.LogWarning("[PTUI] actions not set and could not be found on panelRoot.");
                        return;
                    }

                    _currentTarget = target;
                    actions.BindToTarget(target);
                    OpenPanel("[PTUI] OpenPanel (owner matched): " + target.name);
                }
                else
                {
                    actions?.SubmitCurrentSelection();
                    return;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) && _isOpen)
            {
                ClosePanel("[PTUI] Closed via Escape.");
                return; // consume the input so SettingsManager doesn't see it
            }

        // Keep panel following current target while open
        if (_isOpen && _currentTarget != null)
        {
            PositionPanel(_currentTarget.transform);
        }
    }

    private void OpenPanel(string reasonLog = null)
    {
        if (panelRoot == null)
        {
            Debug.LogWarning("[PTUI] panelRoot not set.");
            return;
        }

        if (reasonLog != null) Debug.Log(reasonLog);

        IsPanelOpen = true;
        _isOpen = true;
        panelRoot.SetActive(true);

        if (_currentTarget != null)
            PositionPanel(_currentTarget.transform);

        // Pause player while deciding
        if (playerMovement) playerMovement.SetPaused(true);

        // Set a default selected button so Space will work immediately
        if (actions && actions.talkButton)
            EventSystem.current?.SetSelectedGameObject(actions.talkButton.gameObject);
    }

    public void ClosePanel(string reasonLog = null)
    {
        if (panelRoot == null) return;
        if (reasonLog != null) Debug.Log(reasonLog);

        IsPanelOpen = false;
        _isOpen = false;
        panelRoot.SetActive(false);
        _currentTarget = null;

        // UNPAUSE PLAYER
        if (playerMovement) playerMovement.SetPaused(false);
    }

    private void PositionPanel(Transform target)
    {
        if (target == null) return;

        if (panelTransform == null)
            panelTransform = panelRoot != null ? panelRoot.transform : null;

        if (panelTransform == null) return;

        if (mainCam == null)
            mainCam = Camera.main;

        if (mainCam == null)
        {
            Debug.LogWarning("[PTUI] No camera found for panel positioning.");
            return;
        }

        float offset = verticalOffsetTiles * gridSize;
        float padding = edgePaddingTiles * gridSize;

        Vector3 targetPos = target.position;

        float camHalfHeight = mainCam.orthographicSize;
        float camTop = mainCam.transform.position.y + camHalfHeight;
        float camBottom = mainCam.transform.position.y - camHalfHeight;

        float panelHalfHeight = GetPanelHalfHeightWorld();

        float aboveY = targetPos.y + offset;
        float belowY = targetPos.y - offset;

        bool aboveFits = (aboveY + panelHalfHeight + padding) <= camTop;
        bool belowFits = (belowY - panelHalfHeight - padding) >= camBottom;

        Vector3 newPos = panelTransform.position;

        // Keep panel centered on the target in X
        newPos.x = targetPos.x;
        newPos.z = panelTransform.position.z;

        // Prefer above unless it would go off the top
        if (aboveFits)
        {
            newPos.y = aboveY;
        }
        else if (belowFits)
        {
            newPos.y = belowY;
        }
        else
        {
            // Clamp inside camera bounds if neither cleanly fits
            newPos.y = Mathf.Clamp(
                aboveY,
                camBottom + panelHalfHeight + padding,
                camTop - panelHalfHeight - padding
            );
        }

        panelTransform.position = newPos;

        if (debug)
        {
            Debug.Log(
                $"[PTUI] target={target.name}, aboveFits={aboveFits}, belowFits={belowFits}, " +
                $"targetY={targetPos.y:F2}, panelY={newPos.y:F2}, camTop={camTop:F2}, camBottom={camBottom:F2}"
            );
        }
    }

    private float GetPanelHalfHeightWorld()
    {
        RectTransform rect = panelTransform as RectTransform;
        if (rect == null)
            return 0.5f * gridSize;

        float worldHeight = rect.rect.height * panelTransform.lossyScale.y;
        return worldHeight * 0.5f;
    }

    // === Core detection ===

    private GridMovement FindAdjacentTaggedNPC()
    {
        if (!player) return null;

        float interactRadius = gridSize * 1.1f;
        var hits = Physics2D.OverlapCircleAll(player.position, interactRadius);

        GridMovement best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            if (!h) continue;

            var gm = h.GetComponentInParent<GridMovement>();
            if (!gm) continue;
            if (!gm.CompareTag(npcTag)) continue;

            float dist = Vector2.Distance(player.position, gm.transform.position);
            if ((dist <= interactRadius && dist < bestDist) || gm.name == "AI")
            {
                best = gm;
                bestDist = dist;
            }
        }

        return best;
    }

    // Snap a world position to grid cell indices, honoring gridOrigin and gridSize
    private Vector2Int WorldToGrid(Vector3 p)
    {
        float lx = (p.x - gridOrigin.x) / gridSize;
        float ly = (p.y - gridOrigin.y) / gridSize;
        int gx = Mathf.RoundToInt(lx);
        int gy = Mathf.RoundToInt(ly);
        return new Vector2Int(gx, gy);
    }

    // Convert grid cell index back to world center
    private Vector2 GridToWorldCenter(Vector2Int cell)
    {
        float wx = gridOrigin.x + cell.x * gridSize;
        float wy = gridOrigin.y + cell.y * gridSize;
        return new Vector2(wx, wy);
    }

    // === Gizmos to visualize probes ===
    private void OnDrawGizmosSelected()
    {
        if (!player) return;

        Gizmos.color = Color.yellow;
        var gp = WorldToGrid(player.position);

        Vector2Int[] dirs =
        {
            new Vector2Int( 1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int( 0, 1),
            new Vector2Int( 0,-1),
        };

        foreach (var d in dirs)
        {
            var cell = gp + d;
            var world = GridToWorldCenter(cell);
            Gizmos.DrawWireSphere(world, probeRadius);
        }
    }

    // Leave for compatibility
    public void SubmitCurrentSelection() { }
}