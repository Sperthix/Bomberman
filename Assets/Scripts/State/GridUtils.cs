using UnityEngine;

namespace State
{
    public static class GridUtils
    {
        public const float CellSize = 2f;

        public static Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPos.x / CellSize),
                Mathf.RoundToInt(worldPos.z / CellSize));
        }

        public static Vector3 GridToWorld(int gx, int gy)
        {
            return new Vector3(gx * CellSize, 0f, gy * CellSize);
        }
    }
}