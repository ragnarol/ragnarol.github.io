using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Data;
using FadingSuns.Core;
using FadingSuns.Characters;

namespace FadingSuns.Dialogue
{
    /// <summary>
    /// Ultima-style keyword dialogue. Player types or clicks a keyword;
    /// the NPC looks it up in their profile and responds. Keywords can
    /// reveal more keywords, change world state, or complete milestones.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [Header("Active Conversation")]
        private NPC activeNPC;
        private DialogueProfile activeProfile;
        private HashSet<string> revealedKeywords = new(); // Keywords the NPC has hinted at

        public event System.Action<NPC, string> OnConversationStarted; // NPC, greeting
        public event System.Action<string, string> OnKeywordResponse;  // keyword, response
        public event System.Action OnConversationEnded;
        public event System.Action<List<string>> OnKeywordsRevealed;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartConversation(NPC npc)
        {
            if (npc.npcData == null || npc.npcData.dialogueProfile == null) return;

            activeNPC = npc;
            activeProfile = npc.npcData.dialogueProfile;
            revealedKeywords.Clear();

            // Track that the player has spoken with this NPC
            var rel = WorldState.Instance.GetNPCRelationship(npc.npcData.npcId);
            rel.hasSpoken = true;

            string greeting = GetGreeting(npc.npcData.npcId);

            // Always available keywords
            revealedKeywords.Add("name");
            revealedKeywords.Add("job");
            revealedKeywords.Add("bye");

            GameManager.Instance.ChangePhase(Core.GamePhase.Dialogue);
            OnConversationStarted?.Invoke(npc, greeting);
        }

        private string GetGreeting(string npcId)
        {
            var rel = WorldState.Instance.GetNPCRelationship(npcId);
            if (rel.trust >= 50) return activeProfile.greetingFriendly;
            if (rel.trust <= -30) return activeProfile.greetingHostile;
            return activeProfile.greetingDefault;
        }

        public void SubmitKeyword(string input)
        {
            if (activeNPC == null || activeProfile == null) return;

            string normalised = input.Trim().ToLowerInvariant();

            if (normalised == "bye" || normalised == "farewell" || normalised == "goodbye")
            {
                EndConversation();
                return;
            }

            DialogueKeyword match = FindKeyword(normalised);

            if (match == null)
            {
                OnKeywordResponse?.Invoke(normalised, activeProfile.unknownKeywordResponse);
                return;
            }

            if (!IsKeywordAccessible(match))
            {
                OnKeywordResponse?.Invoke(normalised, activeProfile.unknownKeywordResponse);
                return;
            }

            // Apply consequences
            ApplyKeywordConsequences(match);

            // Reveal new keywords mentioned
            if (match.grantsKeywords != null)
            {
                var newKeywords = new List<string>();
                foreach (var kw in match.grantsKeywords)
                {
                    if (!revealedKeywords.Contains(kw))
                    {
                        revealedKeywords.Add(kw);
                        newKeywords.Add(kw);
                    }
                }
                if (newKeywords.Count > 0)
                    OnKeywordsRevealed?.Invoke(newKeywords);
            }

            OnKeywordResponse?.Invoke(normalised, match.response);

            if (match.endsConversation) EndConversation();
        }

        private DialogueKeyword FindKeyword(string input)
        {
            foreach (var kw in activeProfile.keywords)
            {
                if (kw.keyword == input) return kw;
                if (kw.aliases != null && kw.aliases.Contains(input)) return kw;
            }
            return null;
        }

        private bool IsKeywordAccessible(DialogueKeyword kw)
        {
            if (!string.IsNullOrEmpty(kw.requiredFlag) && !WorldState.Instance.GetFlag(kw.requiredFlag))
                return false;

            if (!string.IsNullOrEmpty(kw.requiredMilestone) &&
                !GameManager.Instance.milestoneManager.IsMilestoneComplete(kw.requiredMilestone))
                return false;

            if (!string.IsNullOrEmpty(kw.reputationFaction))
            {
                int rep = WorldState.Instance.GetReputation(kw.reputationFaction);
                if (rep < kw.requiredMinReputation) return false;
            }

            return true;
        }

        private void ApplyKeywordConsequences(DialogueKeyword kw)
        {
            if (!string.IsNullOrEmpty(kw.setsFlag))
                WorldState.Instance.SetFlag(kw.setsFlag, true);

            if (!string.IsNullOrEmpty(kw.completeMilestoneId))
                GameManager.Instance.milestoneManager.CompleteAMilestone(kw.completeMilestoneId);

            if (kw.reputationDelta != 0 && !string.IsNullOrEmpty(kw.reputationFactionTarget))
                FactionManager.Instance.ModifyReputationWithConsequences(
                    kw.reputationFactionTarget, kw.reputationDelta);

            // Update NPC trust
            if (kw.reputationDelta != 0)
            {
                var rel = WorldState.Instance.GetNPCRelationship(activeNPC.npcData.npcId);
                rel.trust = Mathf.Clamp(rel.trust + kw.reputationDelta, -100, 100);
            }
        }

        public List<string> GetRevealedKeywords() => new List<string>(revealedKeywords);

        public void EndConversation()
        {
            OnKeywordResponse?.Invoke("bye", activeProfile.farewellDefault);
            activeNPC = null;
            activeProfile = null;
            revealedKeywords.Clear();
            GameManager.Instance.ChangePhase(Core.GamePhase.Exploration);
            OnConversationEnded?.Invoke();
        }
    }
}
