using UnityEngine;
using System.Collections.Generic;

namespace FadingSuns.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Systems")]
        public MilestoneManager milestoneManager;
        public WorldState worldState;
        public FactionManager factionManager;

        [Header("Game State")]
        public GamePhase currentPhase = GamePhase.Exploration;
        public string currentRegion = "City";

        public event System.Action<GamePhase> OnPhaseChanged;
        public event System.Action<string> OnRegionChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            milestoneManager = GetComponentInChildren<MilestoneManager>();
            worldState = GetComponentInChildren<WorldState>();
            factionManager = GetComponentInChildren<FactionManager>();
        }

        public void ChangePhase(GamePhase newPhase)
        {
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(newPhase);
        }

        public void TravelToRegion(string regionId)
        {
            currentRegion = regionId;
            OnRegionChanged?.Invoke(regionId);
            UnityEngine.SceneManagement.SceneManager.LoadScene(regionId);
        }
    }

    public enum GamePhase { Exploration, Dialogue, Combat, Cutscene, Menu }
}
