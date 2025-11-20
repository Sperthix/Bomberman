using UnityEngine;

namespace State
{
    public class Wall
    {
        public Wall(GameObject gameObject, bool isBreakable)
        {
            GameObject = gameObject;
            IsBreakable = isBreakable;
        }

        public GameObject GameObject;
        public bool IsBreakable;
    }
}