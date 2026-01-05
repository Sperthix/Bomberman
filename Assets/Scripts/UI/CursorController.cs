using UnityEngine;

namespace UI
{
    public class CursorController : MonoBehaviour
    {
        private bool isSubscribed;

        private void OnEnable()
        {
            if (!GameManager.Instance) return;
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            isSubscribed = true;
            HandlePhaseChanged(GameManager.Instance.Phase);
        }
        
        private void OnDisable()
        {
            if (!isSubscribed || !GameManager.Instance) return;
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            isSubscribed = false;
        }

        private void Start()
        {
            if (isSubscribed || !GameManager.Instance) return;
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
            isSubscribed = true;
            HandlePhaseChanged(GameManager.Instance.Phase);
        }

        private void HandlePhaseChanged(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Playing:
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    break;

                case GamePhase.MainMenu:
                case GamePhase.GameOver:
                default:
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    break;
            }
        }
    }
}