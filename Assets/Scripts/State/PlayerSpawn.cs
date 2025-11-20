using UnityEngine;

namespace State
{
    public class PlayerSpawn
    {
        public PlayerSpawn(int gridX, int gridY)
        {
            GridX = gridX;
            GridY = gridY;
        }

        public int GridX;
        public int GridY;
    }
}