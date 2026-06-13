using UnityEngine;

namespace FadingSuns.Data
{
    [CreateAssetMenu(fileName = "CharacterData", menuName = "FadingSuns/Character Data")]
    public class CharacterData : ScriptableObject
    {
        [Header("Identity")]
        public string characterId;
        public string displayName;
        public string background;   // e.g. "Noble Heir", "Church Acolyte", "Guild Merchant"
        public CharacterOrigin origin;
        public Sprite portrait;
        public RuntimeAnimatorController animatorController;

        [Header("Core Attributes (1-10 scale)")]
        public int body = 5;        // Physical prowess
        public int mind = 5;        // Intellect, perception
        public int spirit = 5;      // Will, faith, occult capacity

        [Header("Derived Stats")]
        public int vitality = 20;   // Hit points equivalent
        public int wyrd = 10;       // Occult energy pool (Psi/Theurgy)
        public int endurance = 10;  // Stamina

        [Header("Starting Skills")]
        public SkillSet skills;

        [Header("Occult")]
        public OccultType occultType = OccultType.None;
        public int occultRating = 0; // 0 = mundane, higher = stronger powers

        [Header("Faction Affiliation")]
        public string primaryFaction; // "NoblesHouse", "UniversalChurch", etc.
    }

    public enum CharacterOrigin
    {
        Human_Noble,
        Human_Priest,
        Human_GuildMember,
        Human_Freeman,
        Human_Serf,
        Alien_Vorox,
        Alien_Obun,
        Alien_UrObun,
        Alien_Ukar,
        Alien_Gargoyle,
        Symbiont
    }

    public enum OccultType
    {
        None,
        Psi,        // Psychic - born talent
        Theurgy,    // Church rituals and rites
        Antinomy    // Dangerous, uncontrolled
    }

    [System.Serializable]
    public class SkillSet
    {
        // Combat
        public int melee = 1;
        public int ranged = 1;
        public int shield = 0;

        // Social
        public int charm = 1;
        public int knavery = 0;
        public int intimidate = 0;

        // Knowledge
        public int lore = 0;
        public int techRedemption = 0; // Fading Suns: tech is heretical but necessary
        public int occultism = 0;

        // Survival / Physical
        public int athletics = 1;
        public int stealth = 0;
        public int survival = 0;

        // Vocational
        public int spacefaring = 0;
        public int medicine = 0;
        public int artifice = 0;
    }
}
