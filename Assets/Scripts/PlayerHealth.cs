using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
        NotifyHealthChanged();
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        NotifyHealthChanged();
    }

    private void Die()
    {
        Debug.Log("Player died");
        //TODO: Respawn / scoreboard / endgame
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
