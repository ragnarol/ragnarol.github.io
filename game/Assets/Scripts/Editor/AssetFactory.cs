using UnityEngine;
using UnityEditor;
using System.IO;
using FadingSuns.Data;
using FadingSuns.Characters;

namespace FadingSuns.Editor
{
    /// <summary>
    /// Run via Tools > Fading Suns > Generate All Assets.
    /// Creates every ScriptableObject needed to start the game:
    /// archetypes, NPCs, factions, milestones, starting items.
    /// All assets land in Assets/ScriptableObjects/.
    /// </summary>
    public static class AssetFactory
    {
        private const string Root = "Assets/ScriptableObjects";

        [MenuItem("Tools/Fading Suns/Generate All Assets")]
        public static void GenerateAll()
        {
            CreateFolders();
            CreateFactions();
            CreateItems();
            CreateArchetypes();
            CreateNPCs();
            CreateMilestones();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AssetFactory] All Fading Suns assets generated successfully.");
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static void CreateFolders()
        {
            foreach (var sub in new[] { "Factions", "Items", "Characters", "NPCs",
                                        "Milestones", "Abilities", "Tiles" })
                Directory.CreateDirectory($"{Root}/{sub}");
        }

        private static T Create<T>(string path) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        // ── Factions ───────────────────────────────────────────────────────────

        private static void CreateFactions()
        {
            var factions = new (string id, string name, string desc, Color col,
                                string[] rivals, string[] allies)[]
            {
                ("NoblesHouse", "Noble Houses",
                    "The feudal aristocracy of the Known Worlds — proud, fractious, and fading.",
                    new Color(0.8f, 0.7f, 0.2f),
                    new[]{ "Underworld" }, new[]{ "UniversalChurch" }),

                ("UniversalChurch", "Universal Church",
                    "The theocratic faith of the Pancreator — powerful, watchful, and hungry.",
                    new Color(0.9f, 0.9f, 0.9f),
                    new[]{ "Underworld", "MerchantLeague" }, new[]{ "NoblesHouse" }),

                ("MerchantLeague", "Merchant League",
                    "The guilds that keep the stars profitable — pragmatic above all else.",
                    new Color(0.3f, 0.7f, 0.9f),
                    new[]{ "UniversalChurch" }, new[]{ "Aliens" }),

                ("Aliens", "Alien Peoples",
                    "Vorox, Ur-Obun, Ukar — ancient, strange, and barely tolerated.",
                    new Color(0.5f, 0.9f, 0.5f),
                    new[]{ "NoblesHouse" }, new[]{ "MerchantLeague" }),

                ("Underworld", "Underworld",
                    "Criminals, rebels, and those who live outside feudal law.",
                    new Color(0.6f, 0.2f, 0.6f),
                    new[]{ "NoblesHouse", "UniversalChurch" }, System.Array.Empty<string>())
            };

            foreach (var f in factions)
            {
                var asset = Create<FactionData>($"{Root}/Factions/{f.id}.asset");
                asset.id = f.id;
                asset.displayName = f.name;
                asset.description = f.desc;
                asset.factionColor = f.col;
                asset.rivalFactions = new System.Collections.Generic.List<string>(f.rivals);
                asset.alliedFactions = new System.Collections.Generic.List<string>(f.allies);
                EditorUtility.SetDirty(asset);
            }
        }

        // ── Items ──────────────────────────────────────────────────────────────

        private static void CreateItems()
        {
            CreateItem("sword_basic", "Iron Sword", "A plain but reliable blade.",
                ItemCategory.Weapon, WeaponType.Sword, damage: 4, firebirds: 40);

            CreateItem("dagger", "Dagger", "Quick and easily hidden.",
                ItemCategory.Weapon, WeaponType.Dagger, damage: 2, firebirds: 15);

            CreateItem("pistol", "Pistol", "A tech-heresy firearm. Use with caution near Church eyes.",
                ItemCategory.Weapon, WeaponType.Pistol, damage: 6, firebirds: 120, techHeresy: true);

            CreateItem("leather_armor", "Leather Armour", "Worn but adequate.",
                ItemCategory.Armor, protection: 2, firebirds: 30);

            CreateItem("chain_mail", "Chain Mail", "Heavy but protective.",
                ItemCategory.Armor, protection: 4, firebirds: 80);

            CreateItem("relic_shard", "Relic Shard",
                "A fragment of a saint's bone. Requisite for Church rites.",
                ItemCategory.QuestItem, isQuestItem: true, questFlag: "has_relic");

            CreateItem("letter_of_introduction",
                "Letter of Introduction",
                "Signed by a minor noble. Opens doors in the Palace district.",
                ItemCategory.Document, isQuestItem: true, questFlag: "noble_introduced");

            CreateItem("firebirds_50", "50 Firebirds",
                "A purse of firebirds, the currency of the Known Worlds.",
                ItemCategory.TradeGood, firebirds: 50);

            CreateItem("healing_salve", "Healing Salve",
                "An alchemical paste. Heals wounds slowly but surely.",
                ItemCategory.Consumable, consumeEffect: "heal", consumeValue: 15, firebirds: 25);

            CreateItem("wyrd_draught", "Wyrd Draught",
                "A smoky elixir that replenishes occult energy.",
                ItemCategory.Consumable, consumeEffect: "restore_wyrd", consumeValue: 10, firebirds: 40);

            CreateItem("alien_artefact", "Alien Artefact",
                "Of Ur-Obun origin. Hums faintly. Could be valuable — or dangerous.",
                ItemCategory.Artifact, isPsiArtifact: true, firebirds: 200);
        }

        private static void CreateItem(string id, string name, string desc,
            ItemCategory cat,
            WeaponType weapon = WeaponType.None,
            int damage = 0, int protection = 0, int firebirds = 0,
            bool techHeresy = false, bool isQuestItem = false,
            string questFlag = null, string consumeEffect = null,
            int consumeValue = 0, bool isPsiArtifact = false)
        {
            var asset = Create<ItemData>($"{Root}/Items/{id}.asset");
            asset.itemId = id;
            asset.displayName = name;
            asset.description = desc;
            asset.category = cat;
            asset.weaponType = weapon;
            asset.damage = damage;
            asset.protection = protection;
            asset.firebirds = firebirds;
            asset.isTechHeresy = techHeresy;
            asset.isQuestItem = isQuestItem;
            asset.questFlag = questFlag ?? "";
            asset.isConsumable = consumeEffect != null;
            asset.consumeEffectId = consumeEffect ?? "";
            asset.consumeValue = consumeValue;
            asset.isPsiArtifact = isPsiArtifact;
            EditorUtility.SetDirty(asset);
        }

        // ── Player Archetypes ──────────────────────────────────────────────────

        private static void CreateArchetypes()
        {
            // Noble Heir
            {
                var a = Create<CharacterData>($"{Root}/Characters/NobleHeir.asset");
                a.characterId = "noble_heir";
                a.displayName = "Noble Heir";
                a.background = "You carry the name of a crumbling house and a sword that still remembers better days. Born to privilege now fraying at the edges, you must decide whether to restore your house — or abandon it for something greater.";
                a.origin = CharacterOrigin.Human_Noble;
                a.primaryFaction = "NoblesHouse";
                a.body = 5; a.mind = 4; a.spirit = 4;
                a.vitality = 24; a.wyrd = 8;
                a.occultType = OccultType.None;
                a.skills = new SkillSet { melee = 3, charm = 3, lore = 2, knavery = 1, athletics = 2 };
                EditorUtility.SetDirty(a);
            }

            // Church Acolyte
            {
                var a = Create<CharacterData>($"{Root}/Characters/ChurchAcolyte.asset");
                a.characterId = "church_acolyte";
                a.displayName = "Church Acolyte";
                a.background = "The Pancreator's light guides your hand. So does the Inquisition's eye. You have trained in the holy rites and carry the weight of faith — and the suspicion of those who fear what faith can do.";
                a.origin = CharacterOrigin.Human_Priest;
                a.primaryFaction = "UniversalChurch";
                a.body = 3; a.mind = 5; a.spirit = 6;
                a.vitality = 18; a.wyrd = 16;
                a.occultType = OccultType.Theurgy;
                a.occultRating = 1;
                a.skills = new SkillSet { occultism = 3, lore = 3, medicine = 2, charm = 2, athletics = 1 };
                EditorUtility.SetDirty(a);
            }

            // Guild Merchant
            {
                var a = Create<CharacterData>($"{Root}/Characters/GuildMerchant.asset");
                a.characterId = "guild_merchant";
                a.displayName = "Guild Merchant";
                a.background = "Coin speaks louder than a baron's seal — you've always known this. The League sent you here to open new trade routes. You have your own reasons for staying.";
                a.origin = CharacterOrigin.Human_GuildMember;
                a.primaryFaction = "MerchantLeague";
                a.body = 4; a.mind = 6; a.spirit = 3;
                a.vitality = 20; a.wyrd = 6;
                a.occultType = OccultType.None;
                a.skills = new SkillSet { charm = 3, knavery = 3, artifice = 2, techRedemption = 2, ranged = 1 };
                EditorUtility.SetDirty(a);
            }
        }

        // ── NPCs ───────────────────────────────────────────────────────────────

        private static void CreateNPCs()
        {
            // Brother Caspar — City, Church quarter, recruitable
            {
                var n = Create<NPCData>($"{Root}/NPCs/BrotherCaspar.asset");
                n.npcId = "brother_caspar";
                n.displayName = "Caspar";
                n.title = "Brother";
                n.occupation = "Itinerant Physician";
                n.origin = CharacterOrigin.Human_Priest;
                n.faction = "UniversalChurch";
                n.role = NPCRole.Companion;
                n.isRecruitable = true;
                n.isEssential = false;
                n.homeRegion = "City";
                n.body = 3; n.mind = 5; n.spirit = 6; n.vitality = 16;
                n.dailySchedule = new System.Collections.Generic.List<NPCScheduleEntry>
                {
                    new NPCScheduleEntry { hourStart = 6, hourEnd = 12, locationNodeId = "church_steps", activity = "pray" },
                    new NPCScheduleEntry { hourStart = 12, hourEnd = 20, locationNodeId = "market_square", activity = "idle" },
                    new NPCScheduleEntry { hourStart = 20, hourEnd = 6, locationNodeId = "church_interior", activity = "pray" }
                };
                EditorUtility.SetDirty(n);
            }

            // Guildmaster Orlan — Spaceport, trade quest giver
            {
                var n = Create<NPCData>($"{Root}/NPCs/GuildmasterOrlan.asset");
                n.npcId = "guildmaster_orlan";
                n.displayName = "Orlan";
                n.title = "Guildmaster";
                n.occupation = "League Factor";
                n.origin = CharacterOrigin.Human_GuildMember;
                n.faction = "MerchantLeague";
                n.role = NPCRole.QuestGiver;
                n.isRecruitable = false;
                n.isEssential = true;
                n.homeRegion = "Spaceport";
                n.body = 3; n.mind = 7; n.spirit = 4; n.vitality = 14;
                EditorUtility.SetDirty(n);
            }

            // Baron Alexei — Palace, noble questline
            {
                var n = Create<NPCData>($"{Root}/NPCs/BaronAlexei.asset");
                n.npcId = "baron_alexei";
                n.displayName = "Alexei";
                n.title = "Baron";
                n.occupation = "Planetary Governor";
                n.origin = CharacterOrigin.Human_Noble;
                n.faction = "NoblesHouse";
                n.role = NPCRole.Noble;
                n.isRecruitable = false;
                n.isEssential = true;
                n.homeRegion = "Palace";
                n.body = 5; n.mind = 6; n.spirit = 5; n.vitality = 22;
                EditorUtility.SetDirty(n);
            }

            // Grr'kash — Spaceport, Vorox warrior, recruitable
            {
                var n = Create<NPCData>($"{Root}/NPCs/Grrkash.asset");
                n.npcId = "grrkash";
                n.displayName = "Grr'kash";
                n.title = "";
                n.occupation = "Dockworker";
                n.origin = CharacterOrigin.Alien_Vorox;
                n.faction = "Aliens";
                n.role = NPCRole.Companion;
                n.isRecruitable = true;
                n.isEssential = false;
                n.homeRegion = "Spaceport";
                n.body = 8; n.mind = 3; n.spirit = 4; n.vitality = 30;
                EditorUtility.SetDirty(n);
            }

            // Sila — Wilderness, Ur-Obun sage, Psi user, recruitable
            {
                var n = Create<NPCData>($"{Root}/NPCs/Sila.asset");
                n.npcId = "sila";
                n.displayName = "Sila";
                n.title = "Elder";
                n.occupation = "Sage";
                n.origin = CharacterOrigin.Alien_UrObun;
                n.faction = "Aliens";
                n.role = NPCRole.Companion;
                n.isRecruitable = true;
                n.isEssential = false;
                n.homeRegion = "Wilderness";
                n.body = 3; n.mind = 7; n.spirit = 8; n.vitality = 14;
                EditorUtility.SetDirty(n);
            }

            // Inquisitor Harkan — Spaceport, antagonist / quest giver
            {
                var n = Create<NPCData>($"{Root}/NPCs/InquisitorHarkan.asset");
                n.npcId = "inquisitor_harkan";
                n.displayName = "Harkan";
                n.title = "Inquisitor";
                n.occupation = "Church Inquisitor";
                n.origin = CharacterOrigin.Human_Priest;
                n.faction = "UniversalChurch";
                n.role = NPCRole.QuestGiver;
                n.isRecruitable = false;
                n.isEssential = true;
                n.homeRegion = "Spaceport";
                n.body = 6; n.mind = 6; n.spirit = 7; n.vitality = 24;
                EditorUtility.SetDirty(n);
            }

            // Fence Yara — City, underworld contact
            {
                var n = Create<NPCData>($"{Root}/NPCs/FenceYara.asset");
                n.npcId = "fence_yara";
                n.displayName = "Yara";
                n.title = "";
                n.occupation = "Dealer in Curiosities";
                n.origin = CharacterOrigin.Human_Freeman;
                n.faction = "Underworld";
                n.role = NPCRole.Merchant;
                n.isRecruitable = false;
                n.isEssential = false;
                n.homeRegion = "City";
                n.body = 4; n.mind = 5; n.spirit = 3; n.vitality = 16;
                EditorUtility.SetDirty(n);
            }

            // Rebel Leader Tomas — Wilderness
            {
                var n = Create<NPCData>($"{Root}/NPCs/RebelTomas.asset");
                n.npcId = "rebel_tomas";
                n.displayName = "Tomas";
                n.title = "";
                n.occupation = "Rebel Commander";
                n.origin = CharacterOrigin.Human_Serf;
                n.faction = "Underworld";
                n.role = NPCRole.QuestGiver;
                n.isRecruitable = false;
                n.isEssential = true;
                n.homeRegion = "Wilderness";
                n.body = 6; n.mind = 5; n.spirit = 5; n.vitality = 22;
                EditorUtility.SetDirty(n);
            }
        }

        // ── Milestones ─────────────────────────────────────────────────────────

        private static void CreateMilestones()
        {
            // M1 — First Contact
            {
                var m = Create<MilestoneData>($"{Root}/Milestones/M01_FirstContact.asset");
                m.id = "first_contact";
                m.displayName = "First Contact";
                m.description = "You have spoken meaningfully with someone in this city. Word spreads that a stranger asks questions.";
                m.prerequisites = new System.Collections.Generic.List<string>();
                m.rewards = new System.Collections.Generic.List<MilestoneReward>
                {
                    new MilestoneReward { type = RewardType.SetFlag, stringValue = "rumour_keyword_unlocked" }
                };
                EditorUtility.SetDirty(m);
            }

            // M2 — A Name in the City
            {
                var m = Create<MilestoneData>($"{Root}/Milestones/M02_NameInTheCity.asset");
                m.id = "name_in_the_city";
                m.displayName = "A Name in the City";
                m.description = "Your actions have made you known. The city watches you now — some with hope, others with suspicion.";
                m.prerequisites = new System.Collections.Generic.List<string> { "first_contact" };
                m.rewards = new System.Collections.Generic.List<MilestoneReward>
                {
                    new MilestoneReward { type = RewardType.UnlockRegion, stringValue = "Spaceport" },
                    new MilestoneReward { type = RewardType.UnlockPartySlot }
                };
                EditorUtility.SetDirty(m);
            }

            // M3a — Noble path: The Baron's Ear
            {
                var m = Create<MilestoneData>($"{Root}/Milestones/M03a_BaronsEar.asset");
                m.id = "barons_ear";
                m.displayName = "The Baron's Ear";
                m.description = "You have delivered your letter to Baron Alexei and secured his attention. The Palace district is now open to you.";
                m.prerequisites = new System.Collections.Generic.List<string> { "name_in_the_city" };
                m.rewards = new System.Collections.Generic.List<MilestoneReward>
                {
                    new MilestoneReward { type = RewardType.UnlockRegion, stringValue = "Palace" },
                    new MilestoneReward { type = RewardType.SetFlag, stringValue = "noble_path_active" }
                };
                EditorUtility.SetDirty(m);
            }

            // M3b — Church path: The Inquisitor's Test
            {
                var m = Create<MilestoneData>($"{Root}/Milestones/M03b_InquisitorsTest.asset");
                m.id = "inquisitors_test";
                m.displayName = "The Inquisitor's Test";
                m.description = "Inquisitor Harkan has judged you useful. The Palace gates open under Church sponsorship.";
                m.prerequisites = new System.Collections.Generic.List<string> { "name_in_the_city" };
                m.rewards = new System.Collections.Generic.List<MilestoneReward>
                {
                    new MilestoneReward { type = RewardType.UnlockRegion, stringValue = "Palace" },
                    new MilestoneReward { type = RewardType.SetFlag, stringValue = "church_path_active" }
                };
                EditorUtility.SetDirty(m);
            }

            // M3c — Guild path: A Deal Well Made
            {
                var m = Create<MilestoneData>($"{Root}/Milestones/M03c_DealWellMade.asset");
                m.id = "deal_well_made";
                m.displayName = "A Deal Well Made";
                m.description = "The smuggling run succeeded. Orlan is pleased, the League coffers are heavier, and you know the black market's back channels.";
                m.prerequisites = new System.Collections.Generic.List<string> { "name_in_the_city" };
                m.rewards = new System.Collections.Generic.List<MilestoneReward>
                {
                    new MilestoneReward { type = RewardType.SetFlag, stringValue = "guild_path_active" },
                    new MilestoneReward { type = RewardType.SetFlag, stringValue = "black_market_open" },
                    new MilestoneReward { type = RewardType.GrantItem, stringValue = "firebirds_50" }
                };
                EditorUtility.SetDirty(m);
            }

            // M4 — Wilderness Access: The Alien Ruins
            {
                var m = Create<MilestoneData>($"{Root}/Milestones/M04_AlienRuins.asset");
                m.id = "alien_ruins";
                m.displayName = "Into the Wilderness";
                m.description = "You have ventured beyond the city walls into the ancient wilderness where alien ruins and older secrets wait.";
                m.prerequisites = new System.Collections.Generic.List<string> { "name_in_the_city" };
                m.rewards = new System.Collections.Generic.List<MilestoneReward>
                {
                    new MilestoneReward { type = RewardType.UnlockRegion, stringValue = "Wilderness" },
                    new MilestoneReward { type = RewardType.UnlockPartySlot }
                };
                EditorUtility.SetDirty(m);
            }

            // M5 — Full party unlocked
            {
                var m = Create<MilestoneData>($"{Root}/Milestones/M05_BannerRaised.asset");
                m.id = "banner_raised";
                m.displayName = "A Banner Raised";
                m.description = "Three companions stand at your side. The suns are fading, but you are not alone.";
                m.prerequisites = new System.Collections.Generic.List<string> { "alien_ruins" };
                m.rewards = new System.Collections.Generic.List<MilestoneReward>
                {
                    new MilestoneReward { type = RewardType.UnlockPartySlot }
                };
                EditorUtility.SetDirty(m);
            }
        }
    }
}
