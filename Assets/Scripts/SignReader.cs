using DialogueScripts;
using UnityEngine;

public class SignReader : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.Space;

    [Header("References")]
    [SerializeField] private Transform player;                 // usually this.transform
    [SerializeField] private GridMovement playerMovement;      // your player's GridMovement
    [SerializeField] private DialogueManager dialogueManager;  // same one you use for robots

    [Header("Detection")]
    [SerializeField] private float gridSize = .704f;
    [SerializeField] private string robotTag = "Robot";
    [SerializeField] private string signTag = "Sign";

    [Header("UI Hint")]
    [Tooltip("Assign your SpaceBar UI GameObject here. It will be shown when adjacent to a sign (and no robot is nearby) and hidden otherwise.")]
    [SerializeField] private GameObject spaceBarHintUI;

    // The four cardinal directions only — no diagonals
    private static readonly Vector2[] CardinalDirections = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    private bool _pausedForSignDialogue = false;

    private void Reset()
    {
        player = transform;
        playerMovement = GetComponent<GridMovement>();
    }

    private void Update()
    {
        if (!player) player = transform;

        // Don't interfere during hacking/UI suppression
        if (HackManager.IsHacking || HackManager.SuppressUI) return;

        // Show/hide the SpaceBar hint UI based on adjacency
        UpdateSignHint();

        // Unpause after dialogue ends (if we paused for sign reading)
        if (_pausedForSignDialogue && !GlobalGameState.dialogueActive)
        {
            _pausedForSignDialogue = false;
            if (playerMovement) playerMovement.SetPaused(false);
        }

        // Start reading sign
        if (Input.GetKeyDown(interactKey) && !GlobalGameState.dialogueActive)
        {
            // Priority rule: if robot is adjacent, do nothing (robot system handles Space)
            if (FindAdjacentRobot() != null) return;

            var sign = FindAdjacentSign();
            if (sign == null) return;

            var signDialogue = sign.GetDialogue();
            if (signDialogue == null)
            {
                Debug.LogWarning($"[SignReader] Sign '{sign.name}' has no Dialogue assigned.");
                return;
            }

            if (!dialogueManager)
            {
                Debug.LogWarning("[SignReader] dialogueManager not assigned.");
                return;
            }

            if (playerMovement) playerMovement.SetPaused(true);
            _pausedForSignDialogue = true;

            StartCoroutine(StartSignDialogueNextFrame(signDialogue));
        }
    }

    private System.Collections.IEnumerator StartSignDialogueNextFrame(Dialogue dialogue)
    {
        if (playerMovement) playerMovement.SetPaused(true);
        _pausedForSignDialogue = true;

        yield return null;

        while (Input.GetKey(interactKey))
            yield return null;

        dialogueManager.SetPortraitVisible(false);
        dialogueManager.StartDialogue(dialogue, DialogueTheme.Sign);
        dialogueManager.SetPortraitVisible(true);
    }

    // Checks only the 4 cardinal cells — diagonals are excluded
    private GridMovement FindAdjacentRobot()
    {
        foreach (var dir in CardinalDirections)
        {
            Vector2 checkPos = (Vector2)player.position + dir * gridSize;
            var hits = Physics2D.OverlapCircleAll(checkPos, gridSize * 0.4f);

            foreach (var h in hits)
            {
                if (!h) continue;
                var gm = h.GetComponentInParent<GridMovement>();
                if (gm && gm.CompareTag(robotTag))
                    return gm;
            }
        }
        return null;
    }

    // Checks only the 4 cardinal cells — diagonals are excluded
    private SignInteractable FindAdjacentSign()
    {
        foreach (var dir in CardinalDirections)
        {
            Vector2 checkPos = (Vector2)player.position + dir * gridSize;
            var hits = Physics2D.OverlapCircleAll(checkPos, gridSize * 0.4f);

            foreach (var h in hits)
            {
                if (!h) continue;

                var sign = h.GetComponentInParent<SignInteractable>();
                if (!sign)
                {
                    var root = h.transform.root;
                    if (!root || !root.CompareTag(signTag)) continue;
                    sign = root.GetComponentInChildren<SignInteractable>();
                }

                if (sign) return sign;
            }
        }
        return null;
    }

    private void UpdateSignHint()
    {
        if (!spaceBarHintUI) return;

        bool robotNearby = FindAdjacentRobot() != null;
        bool signNearby  = !robotNearby && FindAdjacentSign() != null;

        spaceBarHintUI.SetActive(signNearby);
    }
}