using UnityEngine;

namespace UI
{
    public class CursorController : MonoBehaviour
    {
        private void OnEnable()
        { 
            GameManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }

        private void Start()
        {
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