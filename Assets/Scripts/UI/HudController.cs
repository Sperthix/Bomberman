using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class HudController : MonoBehaviour
    {
        private Label livesLabel;
        private PlayerHealth playerHealth;
        private bool isWired;

        private void Start()
        {
            var uiDoc = GetComponent<UIDocument>();
            var root = uiDoc.rootVisualElement;
            var gs = GameState.Instance;
            
            livesLabel = root.Q<Label>("lives-label");
            gs.OnPlayerSpawned += WirePlayer;
            
            if (!gs.PlayerRef) return;
            var existingHealth = gs.PlayerRef.GetComponent<PlayerHealth>();
            if (existingHealth)
            {
                WirePlayer(existingHealth);
            }
        }

        private void OnDestroy()
        {
            GameState.Instance.OnPlayerSpawned -= WirePlayer;

            if (playerHealth && isWired)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void WirePlayer(PlayerHealth health)
        {
            playerHealth = health;
            playerHealth.OnHealthChanged += HandleHealthChanged;
            isWired = true;

            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void HandleHealthChanged(int current, int max)
        {
            livesLabel.text = $"Lives: {current}";
        }
    }
}