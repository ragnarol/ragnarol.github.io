using UnityEngine;
using System.Collections.Generic;
using FadingSuns.Characters;

namespace FadingSuns.World
{
    /// <summary>
    /// Non-leader party members follow the leader in single-file formation.
    /// In combat they are placed individually by the player.
    /// </summary>
    public class PartyFollowAI : MonoBehaviour
    {
        [Header("Follow Settings")]
        public float followSpeed = 4.5f;
        public float followDistance = 1.2f; // Tiles of separation
        public float arrivalThreshold = 0.1f;

        private Character thisCharacter;
        private IsometricTileMap tileMap;

        // Trail of positions the leader has visited; followers move along it
        private static Queue<Vector3> leaderTrail = new();
        private static Vector3 lastLeaderPos;

        void Start()
        {
            thisCharacter = GetComponent<Character>();
            tileMap = FindObjectOfType<IsometricTileMap>();
        }

        public static void RecordLeaderPosition(Vector3 pos)
        {
            if (Vector3.Distance(pos, lastLeaderPos) > 0.5f)
            {
                leaderTrail.Enqueue(pos);
                lastLeaderPos = pos;
                // Keep trail bounded
                while (leaderTrail.Count > 20) leaderTrail.Dequeue();
            }
        }

        void Update()
        {
            if (Core.GameManager.Instance.currentPhase != Core.GamePhase.Exploration) return;

            var members = PartyManager.Instance.Members;
            int myIndex = -1;
            for (int i = 0; i < members.Count; i++)
                if (members[i] == thisCharacter) { myIndex = i; break; }

            if (myIndex <= 0) return; // Leader doesn't follow

            // Target position: offset along trail
            Vector3 target = GetTargetFromTrail(myIndex);
            if (Vector3.Distance(transform.position, target) > arrivalThreshold)
                transform.position = Vector3.MoveTowards(transform.position, target,
                    followSpeed * Time.deltaTime);
        }

        private Vector3 GetTargetFromTrail(int memberIndex)
        {
            var trail = new List<Vector3>(leaderTrail);
            int targetIdx = Mathf.Max(0, trail.Count - memberIndex * 3);
            return targetIdx < trail.Count ? trail[targetIdx] : transform.position;
        }

        public static void ClearTrail() => leaderTrail.Clear();
    }
}
