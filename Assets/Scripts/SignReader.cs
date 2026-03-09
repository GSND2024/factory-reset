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
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private string robotTag = "Robot";
    [SerializeField] private string signTag = "Sign";

    [Header("Optional UI Hint")]
    [Tooltip("If your sign has a 'Press Space' child object, put its index here (like you did for robots). -1 = don't toggle.")]
    [SerializeField] private int signHintChildIndex = -1;


    private bool _pausedForSignDialogue = false;
    private GameObject _lastHintedSign = null;

    private void Reset()
    {
        player = transform;
        playerMovement = GetComponent<GridMovement>();
    }

    private void Update()
    {
        if (!player) player = transform;

        // Don’t interfere during hacking/UI suppression
        if (HackManager.IsHacking || HackManager.SuppressUI) return;

        // Optional: show/hide sign "press space" hint (only when NO robot is adjacent)
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

            // Pause player while reading (DialogueManager may also do this; this is safe)
            if (playerMovement) playerMovement.SetPaused(true);
            _pausedForSignDialogue = true;

            StartCoroutine(StartSignDialogueNextFrame(signDialogue));
        }
    }

    private System.Collections.IEnumerator StartSignDialogueNextFrame(Dialogue dialogue)
{
    // Optional: pause movement immediately so you don't step away
    if (playerMovement) playerMovement.SetPaused(true);
    _pausedForSignDialogue = true;

    // Wait until next frame so the original Space keydown can't also advance text
    yield return null;

    // (Optional) Wait until Space is released, extra-safe if your manager uses GetKey (held)
    while (Input.GetKey(interactKey))
        yield return null;

    dialogueManager.SetPortraitVisible(false);
    dialogueManager.StartDialogue(dialogue);
    dialogueManager.SetPortraitVisible(true);
}


    private GridMovement FindAdjacentRobot()
    {
        float interactRadius = gridSize * 1.1f;
        var hits = Physics2D.OverlapCircleAll(player.position, interactRadius);

        GridMovement best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            if (!h) continue;
            var gm = h.GetComponentInParent<GridMovement>();
            if (!gm) continue;
            if (!gm.CompareTag(robotTag)) continue;

            float dist = Vector2.Distance(player.position, gm.transform.position);
            if (dist <= interactRadius && dist < bestDist)
            {
                best = gm;
                bestDist = dist;
            }
        }
        return best;
    }

    private SignInteractable FindAdjacentSign()
    {
        float interactRadius = gridSize * 1.1f;
        var hits = Physics2D.OverlapCircleAll(player.position, interactRadius);

        SignInteractable best = null;
        float bestDist = float.MaxValue;

        foreach (var h in hits)
        {
            if (!h) continue;

            // Either tag-based or component-based detection (supports both)
            var sign = h.GetComponentInParent<SignInteractable>();
            if (!sign)
            {
                var root = h.transform.root;
                if (!root) continue;
                if (!root.CompareTag(signTag)) continue;
                sign = root.GetComponentInChildren<SignInteractable>();
            }

            if (!sign) continue;

            float dist = Vector2.Distance(player.position, sign.transform.position);
            if (dist <= interactRadius && dist < bestDist)
            {
                best = sign;
                bestDist = dist;
            }
        }

        return best;
    }

    private void UpdateSignHint()
    {
        // Clean previous hint if needed
        void HideLast()
        {
            if (_lastHintedSign && signHintChildIndex >= 0 && signHintChildIndex < _lastHintedSign.transform.childCount)
                _lastHintedSign.transform.GetChild(signHintChildIndex).gameObject.SetActive(false);
            _lastHintedSign = null;
        }

        if (signHintChildIndex < 0) return; // hint toggling disabled

        // Robot nearby? hide sign hint always (robot has priority)
        if (FindAdjacentRobot() != null)
        {
            HideLast();
            return;
        }

        // Show hint on the closest sign
        var sign = FindAdjacentSign();
        if (!sign)
        {
            HideLast();
            return;
        }

        var signGO = sign.gameObject;
        if (_lastHintedSign != signGO)
        {
            HideLast();
            _lastHintedSign = signGO;

            if (signHintChildIndex >= 0 && signHintChildIndex < signGO.transform.childCount)
                signGO.transform.GetChild(signHintChildIndex).gameObject.SetActive(true);
        }
    }
}
