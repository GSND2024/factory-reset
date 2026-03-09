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

    [Header("References")]
    [SerializeField] private Transform player;         // Player transform
                                                       // add near your other fields
    [SerializeField] private GridMovement playerMovement; // drag your Player's GridMovement here


    [Header("Interaction Rules")]
    [SerializeField] private string npcTag = "Robot";  // Tag placed on hackable NPC roots
    [SerializeField] private float gridSize = 1f;      // Your grid step size
    [Tooltip("World-space origin of your grid. If your grid doesn't start at (0,0), set this to the bottom-left/world origin of your board.")]
    [SerializeField] private Vector2 gridOrigin = Vector2.zero;

    [Header("Debug")]
    [SerializeField] private bool debug = false;
    [SerializeField] private float probeRadius = 0.22f; // small circle at neighbor-cell center

    [SerializeField] private GridMovement owner;

    private bool _isOpen;
    //private GameObject spaceUI;
    void Update()
    {
        if (HackManager.IsHacking || HackManager.SuppressUI)
        {
            if (_isOpen) ClosePanel("[PTUI] Closing because hacking/suppress is active.");
            return;
        }
        //Shuchen fix here:
        
        var best = FindAdjacentTaggedNPC();

        if (best)
        {
            GameObject robot = best.gameObject;
            GlobalGameState.spaceUIRobot = robot;
            robot.transform.GetChild (1).gameObject.SetActive(true);
        }
        else
        {
            if (GlobalGameState.spaceUIRobot)
            {
                GlobalGameState.spaceUIRobot.transform.GetChild (1).gameObject.SetActive(false);
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
                if (target == null) { if (_isOpen) ClosePanel("[PTUI] No NPC nearby."); return; }

                // >>> Only the panel whose owner IS the target handles this press <<<
                if (target != owner) return;

                if (!_isOpen)
                {
                    if (actions == null) { Debug.LogWarning("[PTUI] actions not set."); return; }
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
    }


    private void OpenPanel(string reasonLog = null)
    {
        if (panelRoot == null) { Debug.LogWarning("[PTUI] panelRoot not set."); return; }
        if (reasonLog != null) Debug.Log(reasonLog);

        _isOpen = true;
        panelRoot.SetActive(true);

        // Pause player while deciding
        if (playerMovement) playerMovement.SetPaused(true);

        // Set a default selected button so Space will work immediately
        if (actions && actions.hackButton)      // or actions.talkButton if you prefer Talk first
            EventSystem.current?.SetSelectedGameObject(actions.talkButton.gameObject);
    }


    public void ClosePanel(string reasonLog = null)
    {
        if (panelRoot == null) return;
        if (reasonLog != null) Debug.Log(reasonLog);

        _isOpen = false;
        panelRoot.SetActive(false);

        // UNPAUSE PLAYER (safe even if we’re about to hack)
        if (playerMovement) playerMovement.SetPaused(false);
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

    // Convert grid cell index back to world center (uses origin + size)
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

    // Leave for compatibility (no-op unless you wired it to do something)
    public void SubmitCurrentSelection() { }
}
