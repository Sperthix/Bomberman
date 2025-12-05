using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    private static int maxHealth = 3;
    public int MaxHealth => maxHealth;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(3);

    
    public event Action<int, int> OnHealthChanged;

    private void Start()
    {
        if (IsOwner)
        {
            currentHealth.OnValueChanged += (oldValue, newValue) =>
            NotifyHealthChanged();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth.Value -= damage;
        NotifyHealthChanged();
        if (currentHealth.Value <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
        NotifyHealthChanged();
    }

    private void Die()
    {
        GameManager.Instance.PlayerDied();
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth.Value, maxHealth);
    }
}
