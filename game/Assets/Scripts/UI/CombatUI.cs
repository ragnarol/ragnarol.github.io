using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FadingSuns.Combat;
using FadingSuns.Characters;

namespace FadingSuns.UI
{
    /// <summary>
    /// Combat HUD: shows whose turn it is, action/move points, and combat log.
    /// The player ends their turn via a button; AI turns resolve automatically.
    /// </summary>
    public class CombatUI : MonoBehaviour
    {
        [Header("Turn Info")]
        public TextMeshProUGUI turnLabel;
        public TextMeshProUGUI movePointsLabel;
        public TextMeshProUGUI actionPointsLabel;

        [Header("Buttons")]
        public Button endTurnButton;
        public Button attackButton;

        [Header("Combat Log")]
        public TextMeshProUGUI combatLog;
        public ScrollRect logScroll;

        [Header("Panel")]
        public GameObject combatPanel;

        private CombatUnit currentUnit;

        void Start()
        {
            var cm = CombatManager.Instance;
            cm.OnTurnStarted += OnTurnStarted;
            cm.OnAttackResolved += OnAttackResolved;
            cm.OnCombatEnded += OnCombatEnded;

            endTurnButton.onClick.AddListener(CombatManager.Instance.EndPlayerTurn);
            combatPanel.SetActive(false);
        }

        private void OnTurnStarted(CombatUnit unit)
        {
            combatPanel.SetActive(true);
            currentUnit = unit;

            bool isPlayer = unit.isPlayerControlled;
            turnLabel.text = isPlayer
                ? $"Your Turn — {unit.character.data.displayName}"
                : $"{unit.character.data.displayName}'s Turn";

            movePointsLabel.text = $"Move: {unit.movePointsLeft}";
            actionPointsLabel.text = $"Actions: {unit.actionPointsLeft}";
            endTurnButton.interactable = isPlayer;
            attackButton.interactable = isPlayer;
        }

        private void OnAttackResolved(CombatUnit attacker, CombatUnit target, int damage)
        {
            string line = damage > 0
                ? $"{attacker.character.data.displayName} hits {target.character.data.displayName} for {damage}."
                : $"{attacker.character.data.displayName} misses {target.character.data.displayName}.";

            AppendLog(line);

            if (target.character.isIncapacitated)
                AppendLog($"{target.character.data.displayName} falls!");
        }

        private void AppendLog(string line)
        {
            combatLog.text += "\n" + line;
            Canvas.ForceUpdateCanvases();
            logScroll.verticalNormalizedPosition = 0f;
        }

        private void OnCombatEnded(bool playerWon)
        {
            AppendLog(playerWon ? "\n--- Victory! ---" : "\n--- Defeated. ---");
            endTurnButton.interactable = false;
            attackButton.interactable = false;
            Invoke(nameof(HidePanel), 3f);
        }

        private void HidePanel()
        {
            combatPanel.SetActive(false);
            combatLog.text = "";
        }

        void OnDestroy()
        {
            var cm = CombatManager.Instance;
            if (cm == null) return;
            cm.OnTurnStarted -= OnTurnStarted;
            cm.OnAttackResolved -= OnAttackResolved;
            cm.OnCombatEnded -= OnCombatEnded;
        }
    }
}
