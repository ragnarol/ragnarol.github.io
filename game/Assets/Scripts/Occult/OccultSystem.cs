using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Characters;
using FadingSuns.Data;

namespace FadingSuns.Occult
{
    /// <summary>
    /// Handles Psi (psychic) and Theurgy (Church ritual) powers.
    /// Both draw from the character's Wyrd pool. Overuse risks
    /// Urge (for Psi) or losing Church standing (for Theurgy gone wrong).
    /// </summary>
    public class OccultSystem : MonoBehaviour
    {
        public static OccultSystem Instance { get; private set; }

        [Header("Psi Powers")]
        [SerializeField] private List<PsiPower> psiPowers = new();

        [Header("Theurgy Rites")]
        [SerializeField] private List<TheurgyRite> theurgyRites = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool CanUsePsi(Character caster, string powerId)
        {
            if (caster.data.occultType != OccultType.Psi) return false;
            var power = psiPowers.Find(p => p.powerId == powerId);
            return power != null && caster.currentWyrd >= power.wyrdCost
                   && caster.data.occultRating >= power.requiredRating;
        }

        public OccultResult ActivatePsi(Character caster, Character target, string powerId)
        {
            var power = psiPowers.Find(p => p.powerId == powerId);
            if (power == null || !CanUsePsi(caster, powerId))
                return OccultResult.Failure("Power unavailable.");

            caster.currentWyrd -= power.wyrdCost;

            // Roll: Spirit + Occultism skill vs difficulty
            var roll = caster.RollSkill("occultism");
            int difficulty = power.difficulty + (target != null ? target.GetAttribute(AttributeType.Spirit) : 0);

            if (roll.criticalFailure)
            {
                TriggerUrge(caster);
                return OccultResult.CriticalFailure("The Wyrd turns against you — Urge manifests!");
            }

            if (!roll.IsSuccess)
                return OccultResult.Failure($"The power fades. (Rolled {roll.total} vs {difficulty})");

            ApplyPsiEffect(caster, target, power, roll);
            return OccultResult.Success($"{power.displayName} activated. Margin: {roll.Margin}");
        }

        private void ApplyPsiEffect(Character caster, Character target, PsiPower power, RollResult roll)
        {
            switch (power.effectType)
            {
                case PsiEffectType.Telepathy:
                    // Reveal a random fact about target
                    Debug.Log($"{caster.data.displayName} reads {target?.data.displayName}'s surface thoughts.");
                    break;
                case PsiEffectType.Telekinesis:
                    Debug.Log($"Objects hurl through the air. Damage: {roll.Margin + caster.data.spirit}");
                    target?.TakeDamage(roll.Margin + caster.data.spirit);
                    break;
                case PsiEffectType.PsychicShield:
                    caster.ApplyEffect(new StatusEffect { effectId = "psi_shield", duration = 3, magnitude = 5 });
                    break;
                case PsiEffectType.Empathy:
                    // Gain insight into NPC disposition (affects dialogue)
                    Core.WorldState.Instance.GetNPCRelationship(target?.data.characterId ?? "")
                        .trust += roll.Margin;
                    break;
                case PsiEffectType.Farsight:
                    Debug.Log("A vision of distant events...");
                    break;
            }
        }

        public bool CanUseTheurgy(Character priest, string riteId)
        {
            if (priest.data.occultType != OccultType.Theurgy) return false;
            var rite = theurgyRites.Find(r => r.riteId == riteId);
            return rite != null && priest.currentWyrd >= rite.wyrdCost
                   && priest.data.occultRating >= rite.requiredRating;
        }

        public OccultResult PerformTheurgy(Character priest, Character target, string riteId)
        {
            var rite = theurgyRites.Find(r => r.riteId == riteId);
            if (rite == null || !CanUseTheurgy(priest, riteId))
                return OccultResult.Failure("Rite unavailable.");

            priest.currentWyrd -= rite.wyrdCost;

            var roll = priest.RollSkill("occultism");

            if (roll.criticalFailure)
            {
                // Church standing penalty — the rite went wrong publicly
                Core.FactionManager.Instance.ModifyReputationWithConsequences("UniversalChurch", -10);
                return OccultResult.CriticalFailure("The Saints do not answer. The rite backfires.");
            }

            ApplyTheurgyEffect(priest, target, rite, roll);
            return OccultResult.Success($"The {rite.displayName} is answered by the Pancreator.");
        }

        private void ApplyTheurgyEffect(Character priest, Character target, TheurgyRite rite, RollResult roll)
        {
            switch (rite.effectType)
            {
                case TheurgyEffectType.Healing:
                    int healAmt = roll.Margin + priest.data.spirit * 2;
                    target?.Heal(healAmt);
                    priest.Heal(healAmt / 2); // Grace also heals the faithful
                    break;
                case TheurgyEffectType.Warding:
                    priest.ApplyEffect(new StatusEffect { effectId = "holy_ward", duration = 5, magnitude = 3 });
                    break;
                case TheurgyEffectType.Blessing:
                    // Temporary bonus to all skills — represented as a status flag
                    priest.ApplyEffect(new StatusEffect { effectId = "blessed", duration = 4, magnitude = 2 });
                    break;
                case TheurgyEffectType.Sanctify:
                    // Removes dark/occult effects from a location or character
                    target?.activeEffects.RemoveAll(e => e.effectId.StartsWith("dark_"));
                    break;
                case TheurgyEffectType.Revelation:
                    // Reveals hidden truths — can unlock new dialogue keywords
                    Core.WorldState.Instance.SetFlag($"revelation_{rite.riteId}", true);
                    break;
            }
        }

        /// <summary>
        /// Urge: the dark side of uncontrolled Psi. Character may act erratically.
        /// </summary>
        private void TriggerUrge(Character caster)
        {
            Debug.Log($"URGE triggered for {caster.data.displayName}!");
            caster.ApplyEffect(new StatusEffect { effectId = "urge", duration = 2, magnitude = 0 });
            // In a full implementation, Urge causes AI control for 1-2 turns
        }
    }

    [System.Serializable]
    public class PsiPower
    {
        public string powerId;
        public string displayName;
        public PsiEffectType effectType;
        public int wyrdCost;
        public int requiredRating;
        public int difficulty;
        public int range;
    }

    [System.Serializable]
    public class TheurgyRite
    {
        public string riteId;
        public string displayName;
        public TheurgyEffectType effectType;
        public int wyrdCost;
        public int requiredRating;
        public int difficulty;
    }

    public enum PsiEffectType { Telepathy, Telekinesis, PsychicShield, Empathy, Farsight }
    public enum TheurgyEffectType { Healing, Warding, Blessing, Sanctify, Revelation }

    public class OccultResult
    {
        public bool succeeded;
        public bool critical;
        public string message;

        public static OccultResult Success(string msg) => new() { succeeded = true, message = msg };
        public static OccultResult Failure(string msg) => new() { succeeded = false, message = msg };
        public static OccultResult CriticalFailure(string msg) => new() { succeeded = false, critical = true, message = msg };
    }
}
