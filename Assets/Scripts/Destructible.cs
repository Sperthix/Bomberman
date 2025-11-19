using UnityEngine;

public class Destructible : MonoBehaviour
{
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
