using UnityEngine;
using State;

public class WallBehaviour : MonoBehaviour
{
    private int X { get; set; }
    private int Y { get; set; }
    private WallType Type { get; set; }
    
    public void Init(int x, int y, WallType type)
    {
        X = x;
        Y = y;
        Type = type;
    }
    
    public void HitByExplosion()
    {
        if (Type != WallType.WallDestructible) return;

        GameState.Instance.WallMap[X, Y] = new GridTile(WallType.Empty);
        Destroy(gameObject);
    }
}