using UnityEngine;

public class redX : MonoBehaviour
{
    [SerializeField] private DialogueChoiceScene dialogueChoiceScene;
    [SerializeField] private GameObject targetObject;

    void Update()
    {
        if (dialogueChoiceScene == null || targetObject == null)
            return;

        bool shouldBeActive =
            dialogueChoiceScene.canMakeChoice &&
            !dialogueChoiceScene.canSelectYes;

        targetObject.SetActive(shouldBeActive);
    }
}