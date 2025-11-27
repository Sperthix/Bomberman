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
        public WallType Type;
        public WallBehaviour Wall;

        public GridTile(WallType type)
        {
            Type = type;
            Wall = null;
        }
    }
}