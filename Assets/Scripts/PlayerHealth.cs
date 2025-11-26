using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private int _health = 3;

    public void TakeDamage(int damage)
    {
        _health -= damage;
        Debug.Log("HP left: " + _health);
        if (_health <= 0)
        {
            
            Destroy(gameObject);
        }
    }
}
