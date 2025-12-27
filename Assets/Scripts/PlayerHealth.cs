using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    private static int maxHealth = 3;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(3);


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
        GameManager.Instance.PlayerDied();
    }
}