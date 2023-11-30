using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class empExplosion : MonoBehaviour
{
    Vector3 scaleUp;

    // Start is called before the first frame update
    void Start()
    {
        scaleUp = new Vector3(0.25f, 0.25f, 0.25f);

        Destroy(gameObject, 3.0f);
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(expandExplosion());
    }

    private void OnTriggerEnter(Collider other)
    {
        //Ignore other trigger 
        if (other.isTrigger)
        {
            return;
        }

        IDamage damageable = other.GetComponent<IDamage>();

        if (damageable != null && other.CompareTag("Enemy"))
        {
            damageable.takeDamage(1000);
        }
    }

    IEnumerator expandExplosion()
    {
        transform.localScale += scaleUp;

        yield return new WaitForSeconds(0.25f);
    }
}
