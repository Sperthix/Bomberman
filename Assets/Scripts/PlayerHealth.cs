using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    private static int maxHealth = 3;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(3);
    public Action<GameObject> PlayerDied;


    public void TakeDamage(int damage)
    {
        currentHealth.Value -= damage;
        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        
        PlayerDied.Invoke(gameObject);
        // GameManager.Instance.PlayerDied();
    }
}