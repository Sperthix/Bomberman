namespace State
{
    [System.Serializable]
    public class PlayerSpawn
    {
        public int X { get; }
        public int Y { get; }

        public PlayerSpawn(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}