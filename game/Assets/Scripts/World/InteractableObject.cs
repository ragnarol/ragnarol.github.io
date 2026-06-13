using UnityEngine;
using FadingSuns.Core;

namespace FadingSuns.World
{
    /// <summary>
    /// Doors, containers, shrines, terminals — anything the player can interact with.
    /// </summary>
    public class InteractableObject : MonoBehaviour
    {
        [Header("Interaction")]
        public string objectId;
        public string displayName;
        public InteractableType type;
        public bool requiresItem;
        public string requiredItemId;
        public string requiredFlag;

        [Header("State")]
        public bool isOpen = false;
        public bool isLocked = false;
        public bool isActivated = false;

        [Header("Consequences")]
        public string setsFlag;
        public string completeMilestoneId;
        public string lootTableId;

        private SpriteRenderer spriteRenderer;

        [Header("Sprites")]
        public Sprite spriteOpen;
        public Sprite spriteClosed;
        public Sprite spriteLocked;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateSprite();
        }

        public bool CanInteract()
        {
            if (!string.IsNullOrEmpty(requiredFlag) && !WorldState.Instance.GetFlag(requiredFlag))
                return false;
            return true;
        }

        public void Interact(Characters.Character actor)
        {
            if (!CanInteract()) return;

            if (isLocked)
            {
                TryUnlock(actor);
                return;
            }

            switch (type)
            {
                case InteractableType.Door:
                    ToggleDoor();
                    break;
                case InteractableType.Container:
                    OpenLoot(actor);
                    break;
                case InteractableType.Shrine:
                    ActivateShrine(actor);
                    break;
                case InteractableType.Terminal:
                    ActivateTerminal(actor);
                    break;
                case InteractableType.TravelGate:
                    TravelGate();
                    break;
            }

            if (!string.IsNullOrEmpty(setsFlag))
                WorldState.Instance.SetFlag(setsFlag, true);

            if (!string.IsNullOrEmpty(completeMilestoneId))
                GameManager.Instance.milestoneManager.CompleteAMilestone(completeMilestoneId);
        }

        private void TryUnlock(Characters.Character actor)
        {
            if (requiresItem && actor.inventory.HasItem(requiredItemId))
            {
                actor.inventory.RemoveItem(requiredItemId);
                isLocked = false;
                Debug.Log($"{displayName} unlocked.");
            }
            else
            {
                Debug.Log($"{displayName} is locked.");
            }
        }

        private void ToggleDoor()
        {
            isOpen = !isOpen;
            GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>()?.enabled;
            UpdateSprite();

            // Tell the tilemap about the walkability change
            var tileMap = FindObjectOfType<IsometricTileMap>();
            if (tileMap != null)
            {
                var cell = tileMap.WorldToCell(transform.position);
                tileMap.SetDynamicBlock(cell, !isOpen);
            }
        }

        private void OpenLoot(Characters.Character actor)
        {
            if (isActivated) { Debug.Log("Already looted."); return; }
            isActivated = true;
            // LootTable would be resolved here by an ItemManager
            Debug.Log($"Searching {displayName}... (loot table: {lootTableId})");
        }

        private void ActivateShrine(Characters.Character actor)
        {
            // Church shrines restore Wyrd; noble shrines give blessings
            actor.Heal(10);
            isActivated = true;
            Debug.Log($"{actor.data.displayName} communes with the shrine of {displayName}.");
        }

        private void ActivateTerminal(Characters.Character actor)
        {
            // Open a UI panel; logic depends on terminal type
            isActivated = true;
        }

        private void TravelGate()
        {
            // Handled by the RegionGate component
        }

        private void UpdateSprite()
        {
            if (spriteRenderer == null) return;
            if (isLocked && spriteLocked != null) { spriteRenderer.sprite = spriteLocked; return; }
            spriteRenderer.sprite = isOpen ? spriteOpen : spriteClosed;
        }
    }

    public enum InteractableType { Door, Container, Shrine, Terminal, TravelGate, NPC }
}
