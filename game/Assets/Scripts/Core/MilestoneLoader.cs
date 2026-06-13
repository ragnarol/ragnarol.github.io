using UnityEngine;
using System.IO;
using System.Collections.Generic;
using FadingSuns.Data;

namespace FadingSuns.Core
{
    /// <summary>
    /// Loads milestone definitions from StreamingAssets/Milestones/milestones.json.
    /// Called by SceneBootstrap before the first scene loads.
    /// </summary>
    public class MilestoneLoader : MonoBehaviour
    {
        public static MilestoneLoader Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public List<MilestoneData> Load()
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Milestones", "milestones.json");
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[MilestoneLoader] milestones.json not found at {path}");
                return new List<MilestoneData>();
            }

            var wrapper = JsonUtility.FromJson<MilestoneListDTO>(File.ReadAllText(path));
            var result = new List<MilestoneData>();

            foreach (var dto in wrapper.milestones)
            {
                var m = ScriptableObject.CreateInstance<MilestoneData>();
                m.id = dto.id;
                m.displayName = dto.displayName;
                m.description = dto.description;
                m.prerequisites = dto.prerequisites ?? new List<string>();
                m.rewards = new List<MilestoneReward>();

                if (dto.rewards != null)
                    foreach (var r in dto.rewards)
                        m.rewards.Add(new MilestoneReward
                        {
                            type = System.Enum.Parse<RewardType>(r.type),
                            stringValue = r.stringValue ?? "",
                            intValue = r.intValue
                        });

                result.Add(m);
            }

            Debug.Log($"[MilestoneLoader] Loaded {result.Count} milestone(s).");
            return result;
        }

        [System.Serializable] private class MilestoneListDTO { public List<MilestoneDTO> milestones; }
        [System.Serializable] private class MilestoneDTO
        {
            public string id, displayName, description;
            public List<string> prerequisites;
            public List<RewardDTO> rewards;
        }
        [System.Serializable] private class RewardDTO
        {
            public string type, stringValue;
            public int intValue;
        }
    }
}
