using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Characters;
using FadingSuns.World;
using FadingSuns.Core;

namespace FadingSuns.Combat
{
    /// <summary>
    /// Turn-based tactical combat on the isometric grid.
    /// Initiative is determined by Spirit + 1d10; turns cycle until one side is defeated.
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [Header("Combat State")]
        private List<CombatUnit> turnOrder = new();
        private int currentTurnIndex = 0;
        private bool inCombat = false;

        [Header("Settings")]
        public int defaultMovePoints = 4;
        public int defaultActionPoints = 2;

        private IsometricTileMap tileMap;

        public event System.Action<CombatUnit> OnTurnStarted;
        public event System.Action<CombatUnit, CombatUnit, int> OnAttackResolved; // attacker, target, damage
        public event System.Action<bool> OnCombatEnded; // true = player won

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartCombat(List<Character> playerSide, List<NPC> enemySide)
        {
            tileMap = FindObjectOfType<IsometricTileMap>();
            turnOrder.Clear();
            inCombat = true;

            foreach (var c in playerSide)
                turnOrder.Add(new CombatUnit(c, true));
            foreach (var e in enemySide)
                turnOrder.Add(new CombatUnit(e, false));

            // Sort by initiative: Spirit + 1d10
            foreach (var u in turnOrder)
                u.initiative = u.character.GetAttribute(AttributeType.Spirit) + Random.Range(1, 11);
            turnOrder.Sort((a, b) => b.initiative.CompareTo(a.initiative));

            GameManager.Instance.ChangePhase(GamePhase.Combat);
            Debug.Log("Combat started.");
            StartTurn(0);
        }

        private void StartTurn(int index)
        {
            currentTurnIndex = index % turnOrder.Count;
            var unit = turnOrder[currentTurnIndex];
            unit.movePointsLeft = defaultMovePoints;
            unit.actionPointsLeft = defaultActionPoints;
            OnTurnStarted?.Invoke(unit);

            if (!unit.isPlayerControlled)
                StartCoroutine(RunEnemyTurn(unit));
        }

        public void MoveUnit(CombatUnit unit, Vector3Int targetCell)
        {
            if (!unit.isPlayerControlled || unit.movePointsLeft <= 0) return;

            var path = tileMap.FindPath(
                tileMap.WorldToCell(unit.character.transform.position), targetCell);

            if (path == null || path.Count > unit.movePointsLeft) return;

            unit.movePointsLeft -= path.Count;
            unit.character.transform.position = tileMap.CellToWorld(targetCell);

            tileMap.ClearHighlights();
        }

        public void AttackUnit(CombatUnit attacker, CombatUnit target)
        {
            if (attacker.actionPointsLeft <= 0) return;
            attacker.actionPointsLeft--;

            // Roll: Melee 2d10 + skill vs target Body + 5
            var roll = attacker.character.RollSkill("melee");
            int defense = target.character.GetAttribute(AttributeType.Body) + 5;
            int margin = roll.total - defense;

            int damage = 0;
            if (roll.criticalFailure)
            {
                Debug.Log("Critical failure!");
            }
            else if (margin >= 0)
            {
                damage = Mathf.Max(1, margin + attacker.character.GetAttribute(AttributeType.Body));
                if (roll.criticalSuccess) damage *= 2;
                target.character.TakeDamage(damage);
            }

            OnAttackResolved?.Invoke(attacker, target, damage);
            CheckCombatEnd();
        }

        public void EndPlayerTurn()
        {
            if (!turnOrder[currentTurnIndex].isPlayerControlled) return;
            AdvanceTurn();
        }

        private void AdvanceTurn()
        {
            // Remove dead enemies
            turnOrder.RemoveAll(u => u.character.isIncapacitated && !u.isPlayerControlled);

            if (!inCombat) return;
            StartTurn(currentTurnIndex + 1);
        }

        private System.Collections.IEnumerator RunEnemyTurn(CombatUnit enemy)
        {
            yield return new UnityEngine.WaitForSeconds(0.5f);

            // Simple AI: move toward nearest player, attack if adjacent
            var nearest = GetNearestPlayer(enemy);
            if (nearest != null)
            {
                var targetCell = tileMap.WorldToCell(nearest.character.transform.position);
                var enemyCell = tileMap.WorldToCell(enemy.character.transform.position);
                var path = tileMap.FindPath(enemyCell, targetCell);

                if (path != null && path.Count <= enemy.movePointsLeft + 1)
                {
                    // Move adjacent
                    if (path.Count > 1)
                    {
                        int steps = Mathf.Min(path.Count - 1, enemy.movePointsLeft);
                        enemy.character.transform.position = tileMap.CellToWorld(path[steps - 1]);
                        enemy.movePointsLeft -= steps;
                    }
                    AttackUnit(enemy, nearest);
                }
            }

            yield return new UnityEngine.WaitForSeconds(0.3f);
            AdvanceTurn();
        }

        private CombatUnit GetNearestPlayer(CombatUnit enemy)
        {
            CombatUnit nearest = null;
            float minDist = float.MaxValue;
            foreach (var u in turnOrder)
            {
                if (!u.isPlayerControlled || u.character.isIncapacitated) continue;
                float d = Vector3.Distance(enemy.character.transform.position,
                                           u.character.transform.position);
                if (d < minDist) { minDist = d; nearest = u; }
            }
            return nearest;
        }

        private void CheckCombatEnd()
        {
            bool enemiesAlive = turnOrder.Exists(u => !u.isPlayerControlled && !u.character.isIncapacitated);
            bool playersAlive = turnOrder.Exists(u => u.isPlayerControlled && !u.character.isIncapacitated);

            if (!enemiesAlive) EndCombat(true);
            else if (!playersAlive) EndCombat(false);
        }

        private void EndCombat(bool playerWon)
        {
            inCombat = false;
            tileMap.ClearHighlights();
            GameManager.Instance.ChangePhase(GamePhase.Exploration);
            OnCombatEnded?.Invoke(playerWon);
            Debug.Log(playerWon ? "Victory!" : "Defeat.");
        }
    }

    [System.Serializable]
    public class CombatUnit
    {
        public Character character;
        public bool isPlayerControlled;
        public int initiative;
        public int movePointsLeft;
        public int actionPointsLeft;

        public CombatUnit(Character c, bool playerControlled)
        {
            character = c;
            isPlayerControlled = playerControlled;
        }
    }
}
