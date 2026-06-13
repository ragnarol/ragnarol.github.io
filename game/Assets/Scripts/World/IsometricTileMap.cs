using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using FadingSuns.Data;
// Alias to disambiguate from UnityEngine.Tilemaps.TileData added in Unity 6
using GameTileData = FadingSuns.Data.TileData;

namespace FadingSuns.World
{
    /// <summary>
    /// Manages the isometric tile grid. Unity's built-in Tilemap is used for
    /// rendering; this class provides game logic on top (pathfinding, LOS, interaction).
    /// Camera and tiles should be set to Isometric Z as Y in project settings.
    /// </summary>
    public class IsometricTileMap : MonoBehaviour
    {
        [Header("Tilemaps")]
        public Tilemap groundLayer;
        public Tilemap objectLayer;   // Props, furniture, doors
        public Tilemap overlayLayer;  // Highlights, cursors

        [Header("Tile Definitions")]
        [SerializeField] private List<GameTileData> tileDefinitions = new();
        private Dictionary<string, GameTileData> tileById = new();

        // Runtime walkability overrides (e.g. an NPC blocking a tile)
        private HashSet<Vector3Int> dynamicBlocked = new();

        void Awake()
        {
            foreach (var td in tileDefinitions)
                tileById[td.tileId] = td;
        }

        public bool IsWalkable(Vector3Int cellPos)
        {
            if (dynamicBlocked.Contains(cellPos)) return false;

            // Check object layer first (walls, doors)
            var objTile = objectLayer.GetTile(cellPos);
            if (objTile != null)
            {
                string id = objTile.name;
                if (tileById.TryGetValue(id, out var td) && !td.isWalkable) return false;
            }

            // Then ground layer (water, void)
            var groundTile = groundLayer.GetTile(cellPos);
            if (groundTile == null) return false;
            if (tileById.TryGetValue(groundTile.name, out var gtd))
                return gtd.isWalkable;

            return true;
        }

        public int GetMoveCost(Vector3Int cellPos)
        {
            var tile = groundLayer.GetTile(cellPos);
            if (tile != null && tileById.TryGetValue(tile.name, out var td))
                return td.movementCost;
            return 1;
        }

        public bool BlocksLOS(Vector3Int cellPos)
        {
            var tile = objectLayer.GetTile(cellPos);
            if (tile != null && tileById.TryGetValue(tile.name, out var td))
                return td.blocksLineOfSight;
            return false;
        }

        public void SetDynamicBlock(Vector3Int cellPos, bool blocked)
        {
            if (blocked) dynamicBlocked.Add(cellPos);
            else dynamicBlocked.Remove(cellPos);
        }

        /// <summary>Convert world position to isometric cell coordinates.</summary>
        public Vector3Int WorldToCell(Vector3 worldPos) => groundLayer.WorldToCell(worldPos);

        /// <summary>Convert cell to world position (center of tile).</summary>
        public Vector3 CellToWorld(Vector3Int cell) =>
            groundLayer.GetCellCenterWorld(cell);

        /// <summary>A* pathfinding on the isometric grid.</summary>
        public List<Vector3Int> FindPath(Vector3Int from, Vector3Int to, int maxSearchTiles = 200)
        {
            var open = new SortedList<float, Vector3Int>(Comparer<float>.Create((a, b) =>
                a == b ? 1 : a.CompareTo(b)));
            var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            var gCost = new Dictionary<Vector3Int, float>();

            gCost[from] = 0;
            open.Add(Heuristic(from, to), from);

            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            while (open.Count > 0 && gCost.Count < maxSearchTiles)
            {
                var current = open.Values[0];
                open.RemoveAt(0);

                if (current == to) return ReconstructPath(cameFrom, from, to);

                for (int i = 0; i < 4; i++)
                {
                    var neighbour = new Vector3Int(current.x + dx[i], current.y + dy[i], current.z);
                    if (!IsWalkable(neighbour)) continue;

                    float newG = gCost[current] + GetMoveCost(neighbour);
                    if (!gCost.ContainsKey(neighbour) || newG < gCost[neighbour])
                    {
                        gCost[neighbour] = newG;
                        cameFrom[neighbour] = current;
                        open.Add(newG + Heuristic(neighbour, to), neighbour);
                    }
                }
            }

            return null; // No path found
        }

        private float Heuristic(Vector3Int a, Vector3Int b) =>
            Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom,
            Vector3Int from, Vector3Int to)
        {
            var path = new List<Vector3Int>();
            var current = to;
            while (current != from)
            {
                path.Insert(0, current);
                current = cameFrom[current];
            }
            return path;
        }

        /// <summary>Highlight reachable tiles for movement range display in combat.</summary>
        public void HighlightReachable(Vector3Int origin, int movePoints, TileBase highlightTile)
        {
            overlayLayer.ClearAllTiles();
            var visited = new Dictionary<Vector3Int, int>();
            var queue = new Queue<(Vector3Int, int)>();
            queue.Enqueue((origin, movePoints));

            while (queue.Count > 0)
            {
                var (cell, remaining) = queue.Dequeue();
                if (visited.ContainsKey(cell) && visited[cell] >= remaining) continue;
                visited[cell] = remaining;

                if (cell != origin) overlayLayer.SetTile(cell, highlightTile);

                if (remaining <= 0) continue;
                int[] dx = { 0, 0, 1, -1 };
                int[] dy = { 1, -1, 0, 0 };
                for (int i = 0; i < 4; i++)
                {
                    var nb = new Vector3Int(cell.x + dx[i], cell.y + dy[i], cell.z);
                    int cost = GetMoveCost(nb);
                    if (IsWalkable(nb) && remaining - cost >= 0)
                        queue.Enqueue((nb, remaining - cost));
                }
            }
        }

        public void ClearHighlights() => overlayLayer.ClearAllTiles();
    }
}
