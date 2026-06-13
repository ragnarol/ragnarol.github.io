using UnityEngine;
using System.Collections.Generic;

namespace FadingSuns.Data
{
    [CreateAssetMenu(fileName = "FactionData", menuName = "FadingSuns/Faction Data")]
    public class FactionData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea(1, 3)]
        public string description;

        public Color factionColor = Color.white;
        public Sprite emblem;

        public List<string> rivalFactions = new();
        public List<string> alliedFactions = new();
    }

    [CreateAssetMenu(fileName = "TileData", menuName = "FadingSuns/Tile Data")]
    public class TileData : ScriptableObject
    {
        public string tileId;
        public string displayName;
        public Sprite sprite;
        public bool isWalkable = true;
        public bool blocksLineOfSight = false;
        public int movementCost = 1;   // Higher = slower to traverse
        public TileType tileType;
        public string ambientSoundId;
    }

    public enum TileType
    {
        Floor_Stone,
        Floor_Metal,
        Floor_Dirt,
        Floor_Grass,
        Wall_Stone,
        Wall_Metal,
        Water,
        Void,
        Stairs_Up,
        Stairs_Down,
        Door_Closed,
        Door_Open,
        Vegetation,
        Rubble
    }

    [CreateAssetMenu(fileName = "AbilityData", menuName = "FadingSuns/Ability Data")]
    public class AbilityData : ScriptableObject
    {
        public string abilityId;
        public string displayName;
        [TextArea(1, 3)]
        public string description;
        public AbilityCategory category;
        public int wyrCost = 0;       // Occult energy cost
        public int range = 1;
        public TargetType targetType;
        public string effectId;       // Resolves to an effect handler
        public Sprite icon;
    }

    public enum AbilityCategory { Physical, Social, Psi, Theurgy, Antinomy }
    public enum TargetType { Self, SingleAlly, SingleEnemy, AllAllies, AllEnemies, AreaEffect, Tile }
}
