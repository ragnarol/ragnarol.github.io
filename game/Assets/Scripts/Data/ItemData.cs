using UnityEngine;

namespace FadingSuns.Data
{
    [CreateAssetMenu(fileName = "ItemData", menuName = "FadingSuns/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Identity")]
        public string itemId;
        public string displayName;
        [TextArea(1, 3)]
        public string description;
        public Sprite icon;
        public ItemCategory category;

        [Header("Economy")]
        public int firebirds = 0;       // Fading Suns currency

        [Header("Weapon Stats")]
        public int damage = 0;
        public int damageBonus = 0;
        public int range = 1;           // 1 = melee
        public WeaponType weaponType = WeaponType.None;
        public bool isTechHeresy = false; // Energy weapons attract Church scrutiny

        [Header("Armor Stats")]
        public int protection = 0;
        public bool isShield = false;

        [Header("Consumable")]
        public bool isConsumable = false;
        public string consumeEffectId;  // e.g. "heal_10", "restore_wyrd_5"
        public int consumeValue = 0;

        [Header("Quest / Key Item")]
        public bool isQuestItem = false;
        public string questFlag;        // Flag set when this item is used

        [Header("Occult")]
        public bool isPsiArtifact = false;
        public bool isTheurgyRelicItem = false;
        public string artifactAbilityId;
    }

    public enum ItemCategory
    {
        Weapon,
        Armor,
        Shield,
        Consumable,
        QuestItem,
        Artifact,
        TradeGood,
        Document,
        Clothing
    }

    public enum WeaponType
    {
        None,
        Sword,
        Axe,
        Spear,
        Bow,
        Crossbow,
        Pistol,        // Tech heresy level: moderate
        Laser,         // Tech heresy level: high
        Blaster,       // Tech heresy level: very high
        Flail,
        Dagger,
        Staff
    }
}
