using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DialogueScripts
{
    // 新增：对话主题枚举，方便以后扩展
    public enum DialogueTheme
    {
        Default,
        Sign
    }

    public class DialogueManager : MonoBehaviour {

        public TMP_Text nameText;
        public TMP_Text dialogueText;
        public GameObject dialogueBox;
        public GameObject portrait;
        private bool portraitOn = true;

        // 新增：在 Inspector 里拖入 dialogueBox 的 Image 组件
        public Image dialogueBoxImage;

        // 新增：预设颜色
        private static readonly Color SignBoxColor = new Color(1f, 1f, 1f, 1f);           // 白底
        private static readonly Color SignTextColor = new Color(0f, 0f, 0f, 1f);          // 黑字

        // 新增：缓存默认颜色（从 Inspector 初始值读取）
        private Color _defaultBoxColor;
        private Color _defaultTextColor;

        private Queue<string> _sentences;
    
        public event Action OnDialogueEnd;

        void Start () {
            _sentences = new Queue<string>();

            // 如果 Inspector 没有手动赋值，自动从 dialogueBox 上查找（包括子物体）
            if (!dialogueBoxImage && dialogueBox)
                dialogueBoxImage = dialogueBox.GetComponentInChildren<Image>();

            // 缓存默认颜色
            if (dialogueBoxImage) _defaultBoxColor = dialogueBoxImage.color;
            if (dialogueText)     _defaultTextColor = dialogueText.color;
        }

        // 原有重载保持不变，默认主题
        public void StartDialogue(Dialogue dialogue)
        {
            StartDialogue(dialogue, DialogueTheme.Default);
        }

        // 新增：带主题参数的重载
        public void StartDialogue(Dialogue dialogue, DialogueTheme theme)
        {
            GlobalGameState.dialogueActive = true;
            Time.timeScale = 0f;

            if (dialogueBox)  dialogueBox.SetActive(true);
            if (portrait && portraitOn) portrait.SetActive(true);

            // 新增：根据主题应用颜色
            ApplyTheme(theme);

            nameText.text = dialogue.name;
            _sentences.Clear();

            if (dialogue.hacked)
            {
                foreach (string sentence in dialogue.hackedSentences)
                    _sentences.Enqueue(sentence);
            }
            else
            {
                foreach (string sentence in dialogue.sentences)
                    _sentences.Enqueue(sentence);
            }

            DisplayNextSentence();
        }

        // 新增：颜色主题应用
        private void ApplyTheme(DialogueTheme theme)
        {
            switch (theme)
            {
                case DialogueTheme.Sign:
                    if (dialogueBoxImage) dialogueBoxImage.color = SignBoxColor;
                    if (dialogueText)     dialogueText.color     = SignTextColor;
                    if (nameText)         nameText.color         = SignTextColor;
                    break;

                case DialogueTheme.Default:
                default:
                    if (dialogueBoxImage) dialogueBoxImage.color = _defaultBoxColor;
                    if (dialogueText)     dialogueText.color     = _defaultTextColor;
                    if (nameText)         nameText.color         = _defaultTextColor;
                    break;
            }
        }

        public void DisplayNextSentence ()
        {
            if (_sentences.Count == 0)
            {
                EndDialogue();
                return;
            }
            string sentence = _sentences.Dequeue();
            StopAllCoroutines();
            StartCoroutine(TypeSentence(sentence));
        }

        IEnumerator TypeSentence (string sentence)
        {
            dialogueText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                dialogueText.text += letter;
                yield return null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && GlobalGameState.dialogueActive)
                DisplayNextSentence();
        }

        void EndDialogue()
        {
            GlobalGameState.dialogueActive = false;
            Time.timeScale = 1f;
            if (dialogueBox) dialogueBox.SetActive(false);
            if (portrait)    portrait.SetActive(false);
            Input.ResetInputAxes();
            OnDialogueEnd?.Invoke(); 
        }

        public void SetPortraitVisible(bool visible)
        {
            portraitOn = visible;
        }
    }
}