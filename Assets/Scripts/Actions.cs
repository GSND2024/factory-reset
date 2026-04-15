using System;
using DialogueScripts;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Actions : MonoBehaviour
{
    public Button talkButton;
    public Button hackButton;
    public GameObject hiddenButton;
    public Image portrait;
    public string robotColor;
    public Dialogue dialogue;
    private Transform player;
    //public Dialogue dialogue;
    [SerializeField] private DialogueManager dialogueManager;

    public GridMovement npcMovement;                 // set by PanelToggleUI.SetTarget(...)
    [SerializeField] private PanelToggleUI panel;    // drag your PanelToggleUI here
    private bool _moveRobot = false;
    private bool _hiddenActivated = false;
    private bool _goToPressurePlate= false;
    private bool dieDialogue = false;

    void Start()
    {
        if (hackButton) hackButton.onClick.AddListener(OnHack);
        if (talkButton) talkButton.onClick.AddListener(OnTalk);
        player = GameObject.FindGameObjectWithTag("Player").transform; 
    }

    public void SetTarget(GridMovement npc)
    {
        npcMovement = npc;

        // NEW: grab the Dialogue from the NPC you just targeted
        var holder = npc ? npc.GetComponentInParent<DialogueHolder>() : null;
    }

    public void BindToTarget(GridMovement npc)
    {
        npcMovement = npc;

        // Pull Dialogue from this NPC
        var holder = npc ? npc.GetComponentInParent<DialogueHolder>() : null;
        dialogue = holder ? holder.dialogue : null;

        // Rebind buttons to the real handlers (keep your side-effects intact)
        if (talkButton)
        {
            talkButton.onClick.RemoveAllListeners();
            talkButton.onClick.AddListener(OnTalk);
            talkButton.interactable = (dialogue != null);
        }

        if (hackButton)
        {
            hackButton.onClick.RemoveAllListeners();
            hackButton.onClick.AddListener(OnHack);
            hackButton.interactable = (npcMovement != null);
        }

        // Focus default
        var es = EventSystem.current;
        if (es && talkButton) es.SetSelectedGameObject(talkButton.gameObject);
    }



    private void Update()
    {
        if (hiddenButton && !GlobalGameState.dialogueActive && _hiddenActivated && !GlobalGameState.isRobotHacked)
            hiddenButton.SetActive(true);

        if (GlobalGameState.isLevel7)
        {
            if (!dieDialogue && !GlobalGameState.isPurpleHacked && !GlobalGameState.dialogueActive && _goToPressurePlate)
            {
                transform.position = new Vector3(-4.56f, 2.1f, 0f);
            }
            if (!dieDialogue && !GlobalGameState.dialogueActive && GlobalGameState.destroyCount > 2 && positionCheck(transform.position, new Vector3(-4.56f, 2.1f, 0f)) && player.transform.position.x > 0)
            {
                dieDialogue = true;
                transform.position = new Vector3(-3.862f, 2.1f, 0f);
                dialogue.sentences = dialogue.purpleSentences;
                dialogue.hackedSentences = dialogue.purpleSentences;
                dialogueManager.StartDialogue(dialogue);
            }
        }
        else if (GlobalGameState.isLevel3)
        {
            if (!GlobalGameState.isRobotHacked2 && !GlobalGameState.dialogueActive && _goToPressurePlate)
            {
                if (isDestinationOccupied(GlobalGameState.teleportTarget))
                {
                    if (Vector3.Distance(GlobalGameState.teleportTarget, new Vector3(2.465f, -2.137f, 0f)) < 0.01f)
                    {
                        GlobalGameState.teleportTarget = new Vector3(3.8725f, -2.126f, 0f);
                    } else
                    {
                        dialogue.sentences = new string[] { "Oh, looks like there's already something on the pressure plates." };
                        GlobalGameState.teleportTarget = new Vector3(2.465f, -2.137f, 0f);
                    }
                } else
                {
                    transform.position = GlobalGameState.teleportTarget;
                    dialogue.sentences = new string[] {"Go ahead!"};
                    _goToPressurePlate = false;
                }
            }
        }
        
        else if (GlobalGameState.isLevel2)
        {
            if (!GlobalGameState.isRobotHacked && !GlobalGameState.dialogueActive && _goToPressurePlate)
            {
                Vector3 target = new Vector3(3.9f, .7f, 0f);
                if (isDestinationOccupied(target))
                {
                    dialogue.sentences = new string[] { "Oh, looks like there's already something on the pressure plate." };
                } else
                {
                    transform.position = target;
                    dialogue.sentences = new string[] {"Go ahead!"};
                    _goToPressurePlate = false;
                }
            }
        }

        else if (GlobalGameState.isLevel0)
        {
            if (!GlobalGameState.isWhiteHacked && !GlobalGameState.dialogueActive && _moveRobot)
            {
                transform.position = new Vector3(3.15f, -0.684f, 0f);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(new Vector3(2.466f, -2.137f, 0f), .2f);
    }
    private bool isDestinationOccupied(Vector3 target)
    {
        Debug.Log("Checking");
        LayerMask blockingLayers = LayerMask.GetMask("Blocking", "Player");
        bool occupied = false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(target, .2f, blockingLayers);
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Robot"))
                    continue;

                // This is a valid hit
                Debug.Log("Valid hit: " + hit.name);
                occupied = true;
                break;
            }            
        }
        return occupied;
    }

    private bool positionCheck(Vector3 current, Vector3 target, float tolerance = 0.05f)
    {
        return Vector3.Distance(current, target) <= tolerance;
    }

    private void OnTalk()
    {
        if(portrait){
            Color newColor;
            if (ColorUtility.TryParseHtmlString(robotColor, out newColor)){
                portrait.color = newColor;}}
        Debug.Log($"[Actions] Talk with dialogue: {dialogue.name}");

        if (!GlobalGameState.isFinalLevel)
        {
            dialogueManager.StartDialogue(dialogue);
            Debug.Log("[Actions] Talk");
        }

        panel?.ClosePanel("[Actions] Close after Talk");

        if (GlobalGameState.isFinalLevel)
        {
            if (GlobalGameState.destroyCount == 5)
            {
                dialogueManager.StartDialogue(dialogue);
                dialogueManager.OnDialogueEnd += FinalLevelTalkLogic;
            }
            else
            {
                SceneManager.LoadScene("Terminal");
                Debug.Log("Term");
            }

            return;
        }

        if (GlobalGameState.isLevel0) { _moveRobot = true; }
        if (GlobalGameState.isLevel2 || GlobalGameState.isLevel3 || GlobalGameState.isLevel7) { _goToPressurePlate = true; }
        DialogueHolder DH = gameObject.GetComponent<DialogueHolder>();

        if (GlobalGameState.isEachLevelTalked == false & GlobalGameState.isLevel4 == false & DH.dialogue.hacked == false)
        {
                GlobalGameState.isEachLevelTalked = true;
                GlobalGameState.talkCount += 1;
        }
        
        if (gameObject.name == "Robot" & GlobalGameState.isEachLevelTalked == false & GlobalGameState.isLevel4 & DH.dialogue.hacked == false)
        {
            GlobalGameState.isEachLevelTalked = true;
            GlobalGameState.talkCount += 1;
        }
        if (gameObject.name == "Robot (1)" & GlobalGameState.isEachLevelTalked2 == false & GlobalGameState.isLevel4 & DH.dialogue.hacked == false)
        {
            GlobalGameState.isEachLevelTalked2 = true;
            GlobalGameState.talkCount += 1;
        }
        
        
        
        
        Debug.Log($"TalkCount: {GlobalGameState.talkCount}, HackCount: {GlobalGameState.hackCount}, , destroyCount: {GlobalGameState.destroyCount}");
    }

    private void OnHack()
    {
        dialogue.hacked = true;
        if (GlobalGameState.isFinalLevel)
        {
            if (GlobalGameState.destroyCount == 5)
            {
                dialogueManager.StartDialogue(dialogue);
                panel?.ClosePanel("[Actions] Close after Talk");
                dialogueManager.OnDialogueEnd += FinalLevelHackLogic;
            }
            else
            {
                GlobalGameState.HackAI = true;
                SceneManager.LoadScene("PrototypeLevel5");
            }

            return;
        }
        
        if (GlobalGameState.isEachLevelHacked == false & GlobalGameState.isLevel4 == false)
        {
            GlobalGameState.isEachLevelHacked = true;
            GlobalGameState.hackCount += 1;
        }
        
        if (gameObject.name == "Robot" & GlobalGameState.isEachLevelHacked == false & GlobalGameState.isLevel4)
        {
            GlobalGameState.isEachLevelHacked = true;
            GlobalGameState.hackCount += 1;
        }
        if (gameObject.name == "Robot (1)" & GlobalGameState.isEachLevelHacked2 == false & GlobalGameState.isLevel4) 
        {
            GlobalGameState.isEachLevelHacked2 = true;
            GlobalGameState.hackCount += 1;
        }
        
        if (gameObject.name == "Robot" & GlobalGameState.isRobotHacked == false)
        {
            GlobalGameState.isRobotHacked = true;
        }
        if (gameObject.name == "Robot (1)" & GlobalGameState.isRobotHacked2 == false) 
        {
            GlobalGameState.isRobotHacked2 = true;
        }
        if (gameObject.name == "Purple" & GlobalGameState.isPurpleHacked == false) 
        {
            GlobalGameState.isPurpleHacked = true;
        }
        if (gameObject.name == "Yellow" & GlobalGameState.isYellowHacked == false) 
        {
            GlobalGameState.isYellowHacked = true;
        }
        if (gameObject.name == "White" & GlobalGameState.isWhiteHacked == false) 
        {
            GlobalGameState.isWhiteHacked = true;
        }

        if (!HackManager.Instance) { Debug.LogWarning("[Actions] HackManager missing."); return; }
        if (!npcMovement) { Debug.LogWarning("[Actions] No npcMovement set for hack."); return; }

        panel?.ClosePanel("[Actions] Close before Hack");
        HackManager.Instance.BeginHack(npcMovement);
        
        Debug.Log($"TalkCount: {GlobalGameState.talkCount}, HackCount: {GlobalGameState.hackCount}, , destroyCount: {GlobalGameState.destroyCount}");
    }

    // === Space bar submit support ===
    public void SubmitCurrentSelection()
    {
        var es = EventSystem.current;
        if (es && es.currentSelectedGameObject)
        {
            var btn = es.currentSelectedGameObject.GetComponent<Button>();
            if (btn)
            {
                btn.onClick.Invoke();     // triggers either Talk or Hack depending on selection
                return;
            }
        }

        // Fallback (if nothing is selected): prefer Hack; otherwise Talk
        if (hackButton) { hackButton.onClick.Invoke(); return; }
        if (talkButton) { talkButton.onClick.Invoke(); return; }
    }
    
    void FinalLevelTalkLogic()
    {
        dialogueManager.OnDialogueEnd -= FinalLevelTalkLogic;
        SceneManager.LoadScene("PrototypeLevel5");
    }
    
    void FinalLevelHackLogic()
    {
        dialogueManager.OnDialogueEnd -= FinalLevelHackLogic;
        SceneManager.LoadScene("PrototypeLevel5");
    }
}

    