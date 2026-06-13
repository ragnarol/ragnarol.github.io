using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FadingSuns.Dialogue;
using FadingSuns.Characters;

namespace FadingSuns.UI
{
    /// <summary>
    /// Ultima-style dialogue panel.
    /// Left side: NPC portrait + speech bubble.
    /// Right side: keyword list (clickable) + text input for manual typing.
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("NPC Side")]
        public Image npcPortrait;
        public TextMeshProUGUI npcNameLabel;
        public TextMeshProUGUI npcSpeechText;

        [Header("Player Side")]
        public TMP_InputField keywordInput;
        public Button submitButton;
        public Transform keywordButtonContainer;
        public GameObject keywordButtonPrefab;

        [Header("Log")]
        public TextMeshProUGUI conversationLog;
        public ScrollRect logScrollRect;

        [Header("Panel")]
        public GameObject dialoguePanel;

        private List<string> logLines = new();
        private const int MaxLogLines = 20;

        void Start()
        {
            var dm = DialogueManager.Instance;
            dm.OnConversationStarted += Show;
            dm.OnKeywordResponse += AppendExchange;
            dm.OnConversationEnded += Hide;
            dm.OnKeywordsRevealed += AddKeywordButtons;

            submitButton.onClick.AddListener(SubmitInput);
            keywordInput.onSubmit.AddListener(_ => SubmitInput());
            dialoguePanel.SetActive(false);
        }

        private void Show(NPC npc, string greeting)
        {
            dialoguePanel.SetActive(true);
            npcPortrait.sprite = npc.npcData.portrait;
            npcNameLabel.text = $"{npc.npcData.title} {npc.npcData.displayName}";
            npcSpeechText.text = greeting;
            logLines.Clear();
            conversationLog.text = "";
            ClearKeywordButtons();

            // Always-present keywords
            AddKeywordButtons(new List<string> { "name", "job", "bye" });

            keywordInput.ActivateInputField();
        }

        private void AppendExchange(string keyword, string response)
        {
            npcSpeechText.text = response;

            logLines.Add($"<color=#aaaaff>> {keyword}</color>");
            logLines.Add(response);
            if (logLines.Count > MaxLogLines) logLines.RemoveRange(0, logLines.Count - MaxLogLines);

            conversationLog.text = string.Join("\n", logLines);
            Canvas.ForceUpdateCanvases();
            logScrollRect.verticalNormalizedPosition = 0f;

            keywordInput.text = "";
            keywordInput.ActivateInputField();
        }

        private void AddKeywordButtons(List<string> keywords)
        {
            foreach (var kw in keywords)
            {
                // Avoid duplicates
                foreach (Transform child in keywordButtonContainer)
                    if (child.GetComponentInChildren<TextMeshProUGUI>()?.text == kw) goto nextKw;

                var go = Instantiate(keywordButtonPrefab, keywordButtonContainer);
                go.GetComponentInChildren<TextMeshProUGUI>().text = kw;
                string captured = kw;
                go.GetComponent<Button>().onClick.AddListener(() =>
                    DialogueManager.Instance.SubmitKeyword(captured));

                nextKw:;
            }
        }

        private void ClearKeywordButtons()
        {
            foreach (Transform child in keywordButtonContainer)
                Destroy(child.gameObject);
        }

        private void SubmitInput()
        {
            string kw = keywordInput.text.Trim();
            if (string.IsNullOrEmpty(kw)) return;
            DialogueManager.Instance.SubmitKeyword(kw);
        }

        private void Hide()
        {
            dialoguePanel.SetActive(false);
            ClearKeywordButtons();
        }

        void OnDestroy()
        {
            var dm = DialogueManager.Instance;
            if (dm == null) return;
            dm.OnConversationStarted -= Show;
            dm.OnKeywordResponse -= AppendExchange;
            dm.OnConversationEnded -= Hide;
            dm.OnKeywordsRevealed -= AddKeywordButtons;
        }
    }
}
