using PlayerComponents;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.InGamePlayerHud
{
    public class HudController : MonoBehaviour
    {
        private Label livesLabel;
        
        private PlayerHealth playerHealth;
        private PlayerInventory _playerInventory;
        
        private VisualElement[] slots;

        private void Start()
        {
            var uiDoc = GetComponent<UIDocument>();
            var root = uiDoc.rootVisualElement;

            livesLabel = root.Q<Label>("lives-label");
            
            var actionBar = root.Q<VisualElement>("action-bar");
            slots = new VisualElement[3];
            slots[0] = actionBar.Q<VisualElement>("slot-0");
            slots[1] = actionBar.Q<VisualElement>("slot-1");
            slots[2] = actionBar.Q<VisualElement>("slot-2");
        }
        
        public void BindPlayer(GameObject player)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerHealth.currentHealth.OnValueChanged += HandleHealthChanged;
            HandleHealthChanged(0,playerHealth.currentHealth.Value);
            
            _playerInventory = player.GetComponent<PlayerInventory>();
            _playerInventory.OnAbilitySelectionChanged += HandleAbilitySelectionChanged;
        }

        private void OnDestroy()
        {
            playerHealth.currentHealth.OnValueChanged -= HandleHealthChanged;
            _playerInventory.OnAbilitySelectionChanged -= HandleAbilitySelectionChanged;
        }
        
        private void HandleHealthChanged(int _ ,int current)
        {
            livesLabel.text = $"Lives: {current}";
        }

        private void HandleAbilitySelectionChanged(int currentIndex, int count)
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
