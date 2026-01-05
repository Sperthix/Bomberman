using DefaultNamespace;
using State;
using Unity.Netcode;

namespace BombComponents.Fuse
{
    public class ProximityFuse : NetworkBehaviour
    {
        private GameStateManager _gs;
    
        private void Start()
        {
            _gs = GameStateManager.Instance;
        }

        private void Update()
        {
            if (!IsServer) return;

            var fuzeTile = GridUtils.WorldToGrid(transform.position);
        
        
            foreach (var dynamicObject in _gs.GetDynamicGameObjectsWithTilePlacement())
            {
                var objTile = dynamicObject.Value;
                if (fuzeTile == objTile && dynamicObject.Key.CompareTag("Player"))
                {
                    gameObject.GetComponent<IExplosive>().OnExplosion();
                }
            
            }
        }
    
    }
}
