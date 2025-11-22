using System.Collections;
using System.Linq;
using UnityEngine;

public class BombExplode : MonoBehaviour
{
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float radius = 2f;
    public BombType bombType;

    private GameState _gs;

    private void Start()
    {
        _gs = GameState.Instance;
    }

    private void OnEnable()
    {
        StartCoroutine(FuseCoroutine());
    }

    private IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(fuseTime);

        Explode();
    }

    private void Explode()
    {
        switch (bombType)
        {
            case BombType.SphereBomb:
                SphereExplode();
                break;
            case BombType.LinesBomb:
                LinesExplode();
                break;
        }
    }

    private void LinesExplode()
    {
        ExplodeInLine(Vector3.left);
        ExplodeInLine(Vector3.right);
        ExplodeInLine(Vector3.forward);
        ExplodeInLine(Vector3.back);
        Destroy(gameObject);
    }

    private void ExplodeInLine(Vector3 direction)
    {
        var hits = Physics.RaycastAll(transform.position, direction, radius)
            .OrderBy(hit => hit.distance).ToArray();

        foreach (var hit in hits)
        {
            var HitGameObject = hit.collider.gameObject;
            if (HitGameObject.TryGetComponent<WallBehaviour>(out var wallBehaviour))
            {
                wallBehaviour.HitByExplosion();
                return;
            }
        }
    }

    private void SphereExplode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            //pridat kontrolu na hit hraca - dostane dmg
            var wall = hit.GetComponentInParent<WallBehaviour>();
            wall?.HitByExplosion();
        }

        Destroy(gameObject);
    }
}

public enum BombType
{
    SphereBomb,
    LinesBomb
}