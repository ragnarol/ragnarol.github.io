using UnityEngine;

namespace FadingSuns.Core
{
    /// <summary>
    /// Bootstrap scene (scene index 0) that initialises all persistent singleton
    /// managers and then loads the starting scene. Avoids DontDestroyOnLoad clutter
    /// by spawning managers once here and keeping them alive for the session.
    /// </summary>
    public class SceneBootstrap : MonoBehaviour
    {
        [Header("Manager Prefabs")]
        public GameObject gameManagerPrefab;
        public GameObject partyManagerPrefab;
        public GameObject dialogueManagerPrefab;
        public GameObject combatManagerPrefab;
        public GameObject occultSystemPrefab;
        public GameObject itemManagerPrefab;
        public GameObject saveSystemPrefab;
        public GameObject audioManagerPrefab;
        public GameObject dialogueLoaderPrefab;

        [Header("Start")]
        [Tooltip("Scene name to load after bootstrap")]
        public string startSceneName = "City";
        public bool loadSaveIfExists = true;

        void Awake()
        {
            SpawnIfAbsent<GameManager>(gameManagerPrefab);
            SpawnIfAbsent<Characters.PartyManager>(partyManagerPrefab);
            SpawnIfAbsent<Dialogue.DialogueManager>(dialogueManagerPrefab);
            SpawnIfAbsent<Combat.CombatManager>(combatManagerPrefab);
            SpawnIfAbsent<Occult.OccultSystem>(occultSystemPrefab);
            SpawnIfAbsent<ItemManager>(itemManagerPrefab);
            SpawnIfAbsent<SaveSystem>(saveSystemPrefab);
            SpawnIfAbsent<Dialogue.DialogueLoader>(dialogueLoaderPrefab);
        }

        void Start()
        {
            if (loadSaveIfExists && SaveSystem.Instance != null)
                SaveSystem.Instance.Load();

            UnityEngine.SceneManagement.SceneManager.LoadScene(startSceneName);
        }

        private void SpawnIfAbsent<T>(GameObject prefab) where T : MonoBehaviour
        {
            if (FindObjectOfType<T>() != null) return;
            if (prefab != null)
                Instantiate(prefab);
            else
                new GameObject(typeof(T).Name).AddComponent<T>();
        }
    }
}
