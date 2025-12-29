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

        GameStateManager.Instance.RegisterWall(x, y, this);
    }

    public void HitByExplosion()
    {
        if (Type != WallType.WallDestructible) return;
        GameStateManager.Instance.UnregisterWall(X, Y, this);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.UnregisterWall(X, Y, this);
    }
}