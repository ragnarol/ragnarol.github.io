using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Data;
using FadingSuns.Characters;

namespace FadingSuns.Core
{
    /// <summary>
    /// Player picks one of three starting archetypes at game start.
    /// Each has distinct stats, skills, faction affiliation, and
    /// a unique starting keyword for the dialogue system.
    /// </summary>
    public class CharacterCreation : MonoBehaviour
    {
        [SerializeField] private List<CharacterData> archetypes = new();

        public event System.Action<CharacterData> OnArchetypeChosen;

        public IReadOnlyList<CharacterData> Archetypes => archetypes.AsReadOnly();

        public void ChooseArchetype(int index)
        {
            if (index < 0 || index >= archetypes.Count) return;
            var chosen = archetypes[index];
            OnArchetypeChosen?.Invoke(chosen);
            ApplyToPartyLeader(chosen);
            GameManager.Instance.TravelToRegion("City");
        }

        private void ApplyToPartyLeader(CharacterData data)
        {
            var leader = PartyManager.Instance.leader;
            if (leader == null) return;
            leader.data = data;
            leader.InitFromData();

            // Set starting faction reputation
            WorldState.Instance.ModifyReputation(data.primaryFaction, 20);

            // Noble starts with a letter of introduction
            if (data.origin == CharacterOrigin.Human_Noble)
                leader.inventory.AddItem("letter_of_introduction", 1);

            // Priest starts with a holy relic
            if (data.origin == CharacterOrigin.Human_Priest)
                leader.inventory.AddItem("relic_shard", 1);

            // Guild member starts with coin
            if (data.origin == CharacterOrigin.Human_GuildMember)
                leader.inventory.AddItem("firebirds_50", 1);
        }
    }

    // ── Archetype Data (configured in the Unity Inspector or via ScriptableObjects) ──
    //
    // NOBLE HEIR
    //   Origin: Human_Noble | Faction: NoblesHouse
    //   Body 5, Mind 4, Spirit 4
    //   Skills: Melee 3, Charm 3, Lore 2, Knavery 1
    //   Starting item: Letter of Introduction (opens Palace and high-born NPC dialogue)
    //   Starting keyword unlocked: "house" (NPC responses about noble houses)
    //
    // CHURCH ACOLYTE
    //   Origin: Human_Priest | Faction: UniversalChurch
    //   Body 3, Mind 5, Spirit 6 | Wyrd 15 | OccultType: Theurgy, Rating 1
    //   Skills: Occultism 3, Lore 3, Medicine 2, Charm 2
    //   Starting item: Relic Shard (required for one Theurgy rite)
    //   Starting keyword unlocked: "pancreator" (Church-related NPC responses)
    //
    // GUILD MERCHANT
    //   Origin: Human_GuildMember | Faction: MerchantLeague
    //   Body 4, Mind 6, Spirit 3
    //   Skills: Charm 3, Knavery 3, Artifice 2, TechRedemption 2
    //   Starting currency: 50 Firebirds
    //   Starting keyword unlocked: "trade" (merchant NPC dialogue options)
}
