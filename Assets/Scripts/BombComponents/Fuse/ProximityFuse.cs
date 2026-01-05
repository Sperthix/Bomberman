using DefaultNamespace;
using State;
using Unity.Netcode;

namespace BombComponents.Fuse
{
    public class ProximityFuse : NetworkBehaviour
    {
        private GameStateManager _gs;
        ulong _ownerPlayerUid;

        private void Start()
        {
            _gs = GameStateManager.Instance;
            _ownerPlayerUid = gameObject.GetComponent<BombExplode>().bombOwnerPlayerUid.Value;
        }


        private void Update()
        {
            if (!IsServer) return;
            var fuzeTile = GridUtils.WorldToGrid(transform.position);


            foreach (var dynamicObject in _gs.GetDynamicGameObjectsWithTilePlacement())
            {
                var objTile = dynamicObject.Value;
                if (fuzeTile == objTile && dynamicObject.Key.CompareTag("Player") &&
                    dynamicObject.Key.GetComponent<NetworkObject>().OwnerClientId != _ownerPlayerUid)
                {
                    gameObject.GetComponent<IExplosive>().OnExplosion();
                }
            }
        }
    }
}