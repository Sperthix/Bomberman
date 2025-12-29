using UnityEngine;

namespace UI.InGamePlayerHud
{
    public class InGamePlayerHudManager : MonoBehaviour
    {
        public static InGamePlayerHudManager Instance { get; private set; }

        [SerializeField] private HealthChangeController healthChangeController;
        [SerializeField] private HudController hudController;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void BindPlayer(GameObject playerRef)
        {
            healthChangeController.BindPlayer(playerRef);
            hudController.BindPlayer(playerRef);
        }
    }
}