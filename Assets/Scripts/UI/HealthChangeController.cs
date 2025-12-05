using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class HealthChangeController : MonoBehaviour
    {
        [Header("Flash settings")]
        [SerializeField] private float flashDuration = 1f;
        [SerializeField] private float maxOverlayOpacity = 0.5f;
        [SerializeField] private float numberScale = 1.5f;
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private Color healColor = Color.green;

        [Header("Low HP settings")]
        [SerializeField] private Color lowHealthColor = new Color(1f, 0.6f, 0.2f);
        [SerializeField] private float lowHealthPulseScale = 1.1f;
        [SerializeField] private float lowHealthPulseSpeed = 4f;

        private VisualElement overlay;
        private Label livesLabel;
        private PlayerHealth playerHealth;

        private float baseFontSize;
        private Color baseColor;

        private int lastHealth;

        private bool flashActive;
        private float flashTimer;
        private float flashStartScale;
        private Color flashColor;

        private bool lowHealthActive;
        private float lowHealthPulseTime;

        private void Start()
        {
            var uiDoc = GetComponent<UIDocument>();
            var root = uiDoc.rootVisualElement;

            overlay = root.Q<VisualElement>("health-change-overlay");
            livesLabel = root.Q<Label>("lives-label");
            
            overlay.pickingMode = PickingMode.Ignore;
            overlay.style.opacity = 0f;

            baseFontSize = livesLabel.resolvedStyle.fontSize;
            baseColor = livesLabel.resolvedStyle.color;

            var gs = GameState.Instance;
            gs.OnLocalPlayerSpawned += WireLocalPlayer;

            if (!gs.PlayerRef) return;
            var existingHealth = gs.PlayerRef.GetComponent<PlayerHealth>();
            if (existingHealth != null)
            {
                WireLocalPlayer(existingHealth);
            }
        }

        private void OnDestroy()
        {
            GameState.Instance.OnLocalPlayerSpawned -= WireLocalPlayer;
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }

        private void WireLocalPlayer(PlayerHealth health)
        {
            if (playerHealth)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }

            playerHealth = health;
            playerHealth.OnHealthChanged += HandleHealthChanged;

            lastHealth = playerHealth.currentHealth.Value;
            UpdateLowHealthState(playerHealth.currentHealth.Value, playerHealth.MaxHealth);
        }

        private void HandleHealthChanged(int current, int max)
        {
            UpdateLowHealthState(current, max);

            if (current < lastHealth)
            {
                StartFlash(damageColor, numberScale);
            }
            else if (current > lastHealth)
            {
                StartFlash(healColor, numberScale);
            }

            lastHealth = current;
        }

        private void UpdateLowHealthState(int current, int max)
        {
            bool lowHp = current == 1;

            if (lowHp == lowHealthActive) return;

            lowHealthActive = lowHp;
            lowHealthPulseTime = 0f;
        }

        private void StartFlash(Color color, float scale)
        {
            flashActive = true;
            flashTimer = flashDuration;
            flashColor = color;
            flashStartScale = scale;
        }

        private void Update()
        {
            float finalScale = 1f;
            Color baseTargetColor = baseColor;
            float overlayBaseOpacity = 0f;
            
            if (lowHealthActive)
            {
                lowHealthPulseTime += Time.deltaTime * lowHealthPulseSpeed;
                float pulse = 1f + (lowHealthPulseScale - 1f) * (0.5f + 0.5f * Mathf.Sin(lowHealthPulseTime));
                finalScale *= pulse;

                baseTargetColor = lowHealthColor;
            }

            Color finalColor = baseTargetColor;
            
            if (flashActive)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0f)
                {
                    flashActive = false;
                }
                else
                {
                    float t = 1f - flashTimer / flashDuration;
                    float flashScale = Mathf.Lerp(flashStartScale, 1f, t);
                    finalScale *= flashScale;
                    finalColor = flashColor;
                    overlayBaseOpacity += Mathf.Lerp(maxOverlayOpacity, 0f, t);
                }
            }

            livesLabel.style.fontSize = baseFontSize * finalScale;
            livesLabel.style.color = finalColor;
            overlay.style.backgroundColor = finalColor;
            overlay.style.opacity = overlayBaseOpacity;
        }
    }
}
