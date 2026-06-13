using UnityEngine;
using System.IO;
using System.Collections.Generic;
using FadingSuns.Data;

namespace FadingSuns.Dialogue
{
    /// <summary>
    /// Loads dialogue profiles from StreamingAssets/Dialogue/*.json at runtime.
    /// This decouples dialogue content from Unity asset compilation, letting
    /// writers edit JSON files without reopening the Editor.
    /// </summary>
    public class DialogueLoader : MonoBehaviour
    {
        public static DialogueLoader Instance { get; private set; }

        private Dictionary<string, DialogueProfile> loadedProfiles = new();
        private string dialoguePath;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            dialoguePath = Path.Combine(Application.streamingAssetsPath, "Dialogue");
            LoadAll();
        }

        private void LoadAll()
        {
            if (!Directory.Exists(dialoguePath))
            {
                Debug.LogWarning($"[DialogueLoader] Dialogue directory not found: {dialoguePath}");
                return;
            }

            foreach (var file in Directory.GetFiles(dialoguePath, "*.json"))
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var dto = JsonUtility.FromJson<DialogueProfileDTO>(json);
                    var profile = BuildProfile(dto);
                    loadedProfiles[dto.npcId] = profile;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[DialogueLoader] Failed to load {file}: {ex.Message}");
                }
            }

            Debug.Log($"[DialogueLoader] Loaded {loadedProfiles.Count} dialogue profile(s).");
        }

        public DialogueProfile GetProfile(string npcId) =>
            loadedProfiles.TryGetValue(npcId, out var p) ? p : null;

        private DialogueProfile BuildProfile(DialogueProfileDTO dto)
        {
            var profile = ScriptableObject.CreateInstance<DialogueProfile>();
            profile.greetingDefault = dto.greetingDefault;
            profile.greetingFriendly = dto.greetingFriendly;
            profile.greetingHostile = dto.greetingHostile;
            profile.farewellDefault = dto.farewellDefault;
            profile.unknownKeywordResponse = dto.unknownKeywordResponse;
            profile.keywords = new List<DialogueKeyword>();

            if (dto.keywords == null) return profile;

            foreach (var kw in dto.keywords)
            {
                profile.keywords.Add(new DialogueKeyword
                {
                    keyword = kw.keyword,
                    aliases = kw.aliases ?? new List<string>(),
                    response = kw.response,
                    requiredFlag = kw.requiredFlag ?? "",
                    requiredMilestone = kw.requiredMilestone ?? "",
                    requiredMinReputation = kw.requiredMinReputation,
                    reputationFaction = kw.reputationFaction ?? "",
                    grantsKeywords = kw.grantsKeywords ?? new List<string>(),
                    setsFlag = kw.setsFlag ?? "",
                    completeMilestoneId = kw.completeMilestoneId ?? "",
                    reputationDelta = kw.reputationDelta,
                    reputationFactionTarget = kw.reputationFactionTarget ?? "",
                    endsConversation = kw.endsConversation
                });
            }

            return profile;
        }

        // ── JSON DTOs ─────────────────────────────────────────────────────────

        [System.Serializable]
        private class DialogueProfileDTO
        {
            public string npcId;
            public string greetingDefault;
            public string greetingFriendly;
            public string greetingHostile;
            public string farewellDefault;
            public string unknownKeywordResponse;
            public List<KeywordDTO> keywords;
        }

        [System.Serializable]
        private class KeywordDTO
        {
            public string keyword;
            public List<string> aliases;
            public string response;
            public string requiredFlag;
            public string requiredMilestone;
            public int requiredMinReputation = -100;
            public string reputationFaction;
            public List<string> grantsKeywords;
            public string setsFlag;
            public string completeMilestoneId;
            public int reputationDelta;
            public string reputationFactionTarget;
            public bool endsConversation;
        }
    }
}
