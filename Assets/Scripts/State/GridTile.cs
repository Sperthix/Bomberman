using UnityEngine;

namespace State
{
    public enum WallType
    {
        Empty,                  // O
        WallIndestructible,     // X
        WallDestructible        // W
    }

    [System.Serializable]
    public class GridTile
    {
        public WallType Type { get; }

        public bool IsDestructible => Type == WallType.WallDestructible;
        public bool IsIndestructible => Type == WallType.WallIndestructible;

        public GridTile(WallType type)
        {
            Type = type;
        }
    }
}