using System;
using Unity.Netcode;
using UnityEngine;

namespace PlayerComponents
{
    public class PlayerInventory : NetworkBehaviour
    {
        public GameObject[] bombPreviewPrefabs;
        private int _selectedBombPreviewIndex;

        public event Action<int, int> OnAbilitySelectionChanged; // (currentIndex, totalCount)


        void Start()
        {
            NotifyBombSelectionChanged();
        }


        private void NotifyBombSelectionChanged()
        {
            OnAbilitySelectionChanged?.Invoke(_selectedBombPreviewIndex, bombPreviewPrefabs?.Length ?? 0);
        }

        public void HandleAbilityActionEvent(int actionIndex)
        {
            if (bombPreviewPrefabs == null || bombPreviewPrefabs.Length == 0) return;
            if (actionIndex < 0 || actionIndex >= bombPreviewPrefabs.Length) return;
            if (actionIndex == _selectedBombPreviewIndex) return;

            _selectedBombPreviewIndex = actionIndex;
            NotifyBombSelectionChanged();
        }

        public void HandleUseAbility()
        {
            var bombPreview = Instantiate(bombPreviewPrefabs[_selectedBombPreviewIndex],
                transform.position + (transform.forward * 1f), Quaternion.identity);
            bombPreview.GetComponent<BombSpawnPlaceableValidator>().Init(this.gameObject);
        }
    }
}