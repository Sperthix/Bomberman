using UnityEngine;
using System.Collections;

public class BombExplode : MonoBehaviour
{
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float radius = 2f;

    private void OnEnable()
    {
        StartCoroutine(FuseCoroutine());
    }

    private IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    public void Explode()
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