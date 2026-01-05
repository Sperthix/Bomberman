using System.Collections;
using DefaultNamespace;
using Unity.Netcode;
using UnityEngine;

namespace BombComponents.Fuse
{
    public class TimeFuse : NetworkBehaviour
    {
        [SerializeField] private float fuseTime = 3f;
        void Start()
        {
            if (IsServer)
            {
                StartCoroutine(FuseCoroutine());
            }
        }

        private IEnumerator FuseCoroutine()
        {
            yield return new WaitForSeconds(fuseTime);
            gameObject.GetComponent<IExplosive>().OnExplosion();
        }
    
    }
}
