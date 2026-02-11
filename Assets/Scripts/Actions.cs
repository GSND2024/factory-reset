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
    public Dialogue dialogue;
    //public Dialogue dialogue;
    [SerializeField] private DialogueManager dialogueManager;

    public GridMovement npcMovement;                 // set by PanelToggleUI.SetTarget(...)
    [SerializeField] private PanelToggleUI panel;    // drag your PanelToggleUI here
    private bool _moveRobot = false;
    private bool _hiddenActivated = false;
    private bool _goToPressurePlate= false;

    void Start()
    {
        if (hackButton) hackButton.onClick.AddListener(OnHack);
        if (talkButton) talkButton.onClick.AddListener(OnTalk);
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

        if (GlobalGameState.isLevel3)
        {
            if (!GlobalGameState.isRobotHacked2 && !GlobalGameState.dialogueActive && _goToPressurePlate)
            {
                transform.position = new Vector3(4.584f, 0f, 0f);
            }
        }
        
        else if (GlobalGameState.isLevel2)
        {
            if (!GlobalGameState.isRobotHacked && !GlobalGameState.dialogueActive && _goToPressurePlate)
            {
                transform.position = new Vector3(3.9f, 0f, 0f);
            }
        }

        else if (GlobalGameState.isLevel0)
        {
            if (!GlobalGameState.isRobotHacked && !GlobalGameState.dialogueActive && _moveRobot)
            {
                transform.position = new Vector3(3.85f, -0.684f, 0f);
            }
        }
    }

    private void OnTalk()
    {
        Debug.Log($"[Actions] Talk with dialogue: {dialogue.name}");
        dialogueManager.StartDialogue(dialogue);
        Debug.Log("[Actions] Talk");
        panel?.ClosePanel("[Actions] Close after Talk");
        if (GlobalGameState.isFinalLevel)
        {
            SceneManager.LoadScene("PrototypeLevel5");
        }

        if (GlobalGameState.isLevel0) { _moveRobot = true; }
        if (GlobalGameState.isLevel1) { _hiddenActivated = true; }
        if (GlobalGameState.isLevel2 || GlobalGameState.isLevel3) { _goToPressurePlate = true; }
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
            GlobalGameState.HackAI = true;
            SceneManager.LoadScene("PrototypeLevel5");
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
}
