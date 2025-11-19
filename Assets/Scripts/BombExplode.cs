using UnityEngine;

public class BombExplode : MonoBehaviour
{
    private float fuseTime = 3f;
    private float radius = 2f;

    private void Start()
    {
        Invoke(nameof(Explode), fuseTime);
    }
    
    public void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            //pridat kontrolu na hit hraca - dostane dmg
            
            Destructible destructible = hit.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.Destroy();
            }
        }
        
        Destroy(gameObject);
    }
}
