using UnityEngine;
using State;

public class WallBehaviour : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }

    public WallType Type { get; private set; }
    
    public void Init(int x, int y, WallType type)
    {
        X = x;
        Y = y;
        Type = type;

        GameState.Instance.RegisterWall(x, y, this);
    }
    
    public void HitByExplosion()
    {
        if (Type != WallType.WallDestructible) return;

        GameState.Instance.WallMap[X, Y] = new GridTile(WallType.Empty);
        GameState.Instance.UnregisterWall(X, Y, this);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (GameState.Instance)
        {
            GameState.Instance.UnregisterWall(X, Y, this);
        }
    }
}