using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class HudController : MonoBehaviour
    {
        private Label livesLabel;
        private PlayerHealth playerHealth;
        private PlayerController playerController;
        private bool isWired;
        
        private VisualElement[] slots;

        private void Start()
        {
            var uiDoc = GetComponent<UIDocument>();
            var root = uiDoc.rootVisualElement;
            var gs = GameState.Instance;

            livesLabel = root.Q<Label>("lives-label");
            
            var actionBar = root.Q<VisualElement>("action-bar");
            slots = new VisualElement[3];
            slots[0] = actionBar.Q<VisualElement>("slot-0");
            slots[1] = actionBar.Q<VisualElement>("slot-1");
            slots[2] = actionBar.Q<VisualElement>("slot-2");

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

            if (playerController)
            {
                playerController.OnBombSelectionChanged -= HandleBombSelectionChanged;
            }
        }

        private void WirePlayer(PlayerHealth health)
        {
            if (playerHealth && isWired)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }

            playerHealth = health;
            playerHealth.OnHealthChanged += HandleHealthChanged;
            isWired = true;
            
            playerController = health.GetComponent<PlayerController>();
            if (playerController)
            {
                playerController.OnBombSelectionChanged += HandleBombSelectionChanged;
            }

            HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }

        private void HandleHealthChanged(int current, int max)
        {
            livesLabel.text = $"Lives: {current}";
        }

        private void HandleBombSelectionChanged(int currentIndex, int count)
        { 
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                if (i == currentIndex)
                    slots[i].AddToClassList("selected");
                else
                    slots[i].RemoveFromClassList("selected");
            }
        }
    }
}
