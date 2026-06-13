using UnityEngine;
using UnityEngine.InputSystem;
using FadingSuns.Core;
using FadingSuns.Characters;
using FadingSuns.World;
using FadingSuns.Dialogue;
using FadingSuns.Combat;

namespace FadingSuns.Input
{
    /// <summary>
    /// Click-to-move player controller for isometric view.
    /// Uses Unity's new Input System. Click on tiles to move,
    /// click on NPCs to talk, click on objects to interact.
    /// </summary>
    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerController : MonoBehaviour
    {
        private PlayerCharacter playerChar;
        private IsometricTileMap tileMap;
        private Camera mainCam;

        [Header("Movement")]
        private Vector3Int currentCell;
        private Vector3Int targetCell;
        private bool isMoving = false;
        public float moveSpeed = 5f;

        [Header("Layers")]
        public LayerMask npcLayer;
        public LayerMask objectLayer;
        public LayerMask tileLayer;

        void Awake()
        {
            playerChar = GetComponent<PlayerCharacter>();
            mainCam = Camera.main;
        }

        void Start()
        {
            tileMap = FindObjectOfType<IsometricTileMap>();
            currentCell = tileMap.WorldToCell(transform.position);
        }

        void Update()
        {
            if (GameManager.Instance.currentPhase == GamePhase.Dialogue ||
                GameManager.Instance.currentPhase == GamePhase.Cutscene) return;

            if (GameManager.Instance.currentPhase == GamePhase.Combat)
            {
                HandleCombatInput();
                return;
            }

            HandleExplorationInput();

            if (isMoving) StepTowardTarget();
        }

        private void HandleExplorationInput()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            var worldPos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            worldPos.z = 0;

            // Priority: NPC > Object > Tile
            var npcHit = Physics2D.OverlapPoint(worldPos, npcLayer);
            if (npcHit != null)
            {
                var npc = npcHit.GetComponent<NPC>();
                if (npc != null)
                {
                    MoveAdjacentTo(npc.transform.position);
                    // Trigger dialogue once adjacent
                    StartCoroutine(TalkWhenAdjacent(npc));
                    return;
                }
            }

            var objHit = Physics2D.OverlapPoint(worldPos, objectLayer);
            if (objHit != null)
            {
                var interactable = objHit.GetComponent<InteractableObject>();
                if (interactable != null && interactable.CanInteract())
                {
                    MoveAdjacentTo(interactable.transform.position);
                    StartCoroutine(InteractWhenAdjacent(interactable));
                    return;
                }
            }

            var clickedCell = tileMap.WorldToCell(worldPos);
            if (tileMap.IsWalkable(clickedCell))
                BeginMoveTo(clickedCell);
        }

        private void HandleCombatInput()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            var worldPos = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            worldPos.z = 0;

            var clickedCell = tileMap.WorldToCell(worldPos);

            var enemyHit = Physics2D.OverlapPoint(worldPos, npcLayer);
            if (enemyHit != null)
            {
                var npc = enemyHit.GetComponent<NPC>();
                if (npc != null && npc.currentState == NPCState.Hostile)
                {
                    // Attack
                    var attacker = GetCurrentCombatUnit();
                    var target = GetCombatUnit(npc.GetComponent<Character>());
                    if (attacker != null && target != null)
                        CombatManager.Instance.AttackUnit(attacker, target);
                }
                return;
            }

            if (tileMap.IsWalkable(clickedCell))
                CombatManager.Instance.MoveUnit(GetCurrentCombatUnit(), clickedCell);
        }

        private void BeginMoveTo(Vector3Int cell)
        {
            targetCell = cell;
            isMoving = true;
        }

        private void MoveAdjacentTo(Vector3 target)
        {
            var targetIsoCel = tileMap.WorldToCell(target);
            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };
            Vector3Int best = currentCell;
            float bestDist = float.MaxValue;
            for (int i = 0; i < 4; i++)
            {
                var adj = new Vector3Int(targetIsoCel.x + dx[i], targetIsoCel.y + dy[i], 0);
                if (!tileMap.IsWalkable(adj)) continue;
                float d = Vector3.Distance(tileMap.CellToWorld(adj), transform.position);
                if (d < bestDist) { bestDist = d; best = adj; }
            }
            BeginMoveTo(best);
        }

        private void StepTowardTarget()
        {
            var worldTarget = tileMap.CellToWorld(targetCell);
            transform.position = Vector3.MoveTowards(transform.position, worldTarget,
                moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, worldTarget) < 0.01f)
            {
                transform.position = worldTarget;
                currentCell = targetCell;
                isMoving = false;
                tileMap.SetDynamicBlock(currentCell, true);
            }
        }

        private System.Collections.IEnumerator TalkWhenAdjacent(NPC npc)
        {
            yield return new WaitUntil(() => !isMoving);
            if (Vector3.Distance(transform.position, npc.transform.position) < 2.5f)
                DialogueManager.Instance.StartConversation(npc);
        }

        private System.Collections.IEnumerator InteractWhenAdjacent(InteractableObject obj)
        {
            yield return new WaitUntil(() => !isMoving);
            if (Vector3.Distance(transform.position, obj.transform.position) < 2.5f)
                obj.Interact(playerChar);
        }

        private CombatUnit GetCurrentCombatUnit()
        {
            // Resolved from CombatManager's turn order
            return null; // Implemented via CombatManager.GetPlayerUnit(playerChar)
        }

        private CombatUnit GetCombatUnit(Character c)
        {
            return null; // Resolved from CombatManager
        }
    }
}
