using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class empExplosion : MonoBehaviour
{
    Vector3 scaleUp;
    [SerializeField] float expandRate;

    // Start is called before the first frame update
    void Start()
    {
        scaleUp = new Vector3(expandRate, expandRate, expandRate);

        Destroy(gameObject, 3.0f);
    }

    // Update is called once per frame
    void Update()
    {
        expandExplosion();
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

    void expandExplosion()
    {
        transform.localScale += scaleUp * Time.deltaTime;
    }
}
