using DialogueScripts;
using UnityEngine;

public class SignInteractable : MonoBehaviour
{
    [Tooltip("DialogueHolder on this sign (or its children). If left blank, it will auto-find.")]
    public DialogueHolder dialogueHolder;

    public Dialogue GetDialogue()
    {
        if (!dialogueHolder) dialogueHolder = GetComponentInChildren<DialogueHolder>();
        return dialogueHolder ? dialogueHolder.dialogue : null;
    }
}
