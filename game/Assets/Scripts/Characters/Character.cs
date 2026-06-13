using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Data;

namespace FadingSuns.Characters
{
    public abstract class Character : MonoBehaviour
    {
        [Header("Data")]
        public CharacterData data;

        [Header("Runtime Stats")]
        public int currentVitality;
        public int maxVitality;
        public int currentWyrd;
        public int maxWyrd;

        [Header("Status")]
        public List<StatusEffect> activeEffects = new();
        public bool isIncapacitated = false;

        public AbilitySet abilities;
        public Inventory inventory;

        protected virtual void Awake()
        {
            if (data != null) InitFromData();
            abilities = GetComponent<AbilitySet>() ?? gameObject.AddComponent<AbilitySet>();
            inventory = GetComponent<Inventory>() ?? gameObject.AddComponent<Inventory>();
        }

        public void InitFromData()
        {
            maxVitality = data.vitality + data.body * 2;
            currentVitality = maxVitality;
            maxWyrd = data.wyrd + data.spirit;
            currentWyrd = maxWyrd;
        }

        public int GetAttribute(AttributeType attr) => attr switch
        {
            AttributeType.Body => data.body,
            AttributeType.Mind => data.mind,
            AttributeType.Spirit => data.spirit,
            _ => 0
        };

        public int GetSkillValue(string skillName)
        {
            var s = data.skills;
            return skillName switch
            {
                "melee" => s.melee,
                "ranged" => s.ranged,
                "charm" => s.charm,
                "knavery" => s.knavery,
                "intimidate" => s.intimidate,
                "lore" => s.lore,
                "techRedemption" => s.techRedemption,
                "occultism" => s.occultism,
                "athletics" => s.athletics,
                "stealth" => s.stealth,
                "survival" => s.survival,
                "medicine" => s.medicine,
                _ => 0
            };
        }

        /// <summary>Roll 2d10 + skill vs difficulty. Returns margin of success/failure.</summary>
        public RollResult RollSkill(string skillName, int difficulty = 10)
        {
            int d1 = Random.Range(1, 11);
            int d2 = Random.Range(1, 11);
            int roll = d1 + d2 + GetSkillValue(skillName);
            return new RollResult(roll, difficulty, d1 == 1 && d2 == 1, d1 == 10 && d2 == 10);
        }

        public void TakeDamage(int amount)
        {
            currentVitality = Mathf.Max(0, currentVitality - amount);
            if (currentVitality <= 0) OnIncapacitated();
        }

        public void Heal(int amount)
        {
            currentVitality = Mathf.Min(maxVitality, currentVitality + amount);
            isIncapacitated = false;
        }

        protected virtual void OnIncapacitated()
        {
            isIncapacitated = true;
        }

        public void ApplyEffect(StatusEffect effect)
        {
            activeEffects.Add(effect);
        }

        public void TickEffects()
        {
            activeEffects.RemoveAll(e => e.Tick(this));
        }
    }

    public enum AttributeType { Body, Mind, Spirit }

    [System.Serializable]
    public class RollResult
    {
        public int total;
        public int difficulty;
        public bool criticalSuccess;
        public bool criticalFailure;
        public bool IsSuccess => total >= difficulty && !criticalFailure;
        public int Margin => total - difficulty;

        public RollResult(int total, int difficulty, bool critFail, bool critSuccess)
        {
            this.total = total;
            this.difficulty = difficulty;
            this.criticalFailure = critFail;
            this.criticalSuccess = critSuccess;
        }
    }

    [System.Serializable]
    public class StatusEffect
    {
        public string effectId;
        public int duration; // turns remaining
        public int magnitude;

        // Returns true when expired
        public bool Tick(Character target)
        {
            duration--;
            // Apply per-turn effect
            if (effectId == "burning") target.TakeDamage(magnitude);
            if (effectId == "healing") target.Heal(magnitude);
            return duration <= 0;
        }
    }
}
