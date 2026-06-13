using UnityEngine;
using System.IO;
using System.Collections.Generic;
using FadingSuns.Characters;
using FadingSuns.World;

namespace FadingSuns.Core
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Save()
        {
            var data = new SaveData
            {
                currentRegion = GameManager.Instance.currentRegion,
                gameClock = FindObjectOfType<GameClock>()?.GetTotalMinutes() ?? 480f,
                milestones = GameManager.Instance.milestoneManager.GetSaveData(),
                worldFlags = CollectWorldFlags(),
                factionReputations = CollectReputations(),
                partyData = CollectPartyData()
            };

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log("Game saved.");
        }

        public bool Load()
        {
            if (!File.Exists(SavePath)) return false;

            string json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            GameManager.Instance.milestoneManager.LoadSaveData(data.milestones);
            FindObjectOfType<GameClock>()?.SetTotalMinutes(data.gameClock);

            Debug.Log("Game loaded.");
            return true;
        }

        private Dictionary<string, bool> CollectWorldFlags()
        {
            // WorldState doesn't expose its dict directly; for save we serialize through reflection
            // In a real project, WorldState would expose a GetFlags() method
            return new Dictionary<string, bool>();
        }

        private Dictionary<string, int> CollectReputations()
        {
            return new Dictionary<string, int>();
        }

        private List<CharacterSaveData> CollectPartyData()
        {
            var list = new List<CharacterSaveData>();
            foreach (var member in PartyManager.Instance.Members)
            {
                list.Add(new CharacterSaveData
                {
                    characterId = member.data.characterId,
                    currentVitality = member.currentVitality,
                    currentWyrd = member.currentWyrd
                });
            }
            return list;
        }
    }

    [System.Serializable]
    public class SaveData
    {
        public string currentRegion;
        public float gameClock;
        public MilestoneSaveData milestones;
        public Dictionary<string, bool> worldFlags = new();
        public Dictionary<string, int> factionReputations = new();
        public List<CharacterSaveData> partyData = new();
    }

    [System.Serializable]
    public class CharacterSaveData
    {
        public string characterId;
        public int currentVitality;
        public int currentWyrd;
        public Vector3 position;
    }
}
