using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class grenade : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] int grenadeSpeed;
    [SerializeField] int grenadeLaunchUp;
    [SerializeField] int explosionTime;
    [SerializeField] GameObject explosion;

    // Start is called before the first frame update
    void Start()
    {
        //Grenade projectile motion 
        rb.velocity = (Vector3.up * grenadeLaunchUp) + (transform.forward) * grenadeSpeed;

        //Countdown until explosion
        StartCoroutine(explode());
    }

    IEnumerator explode()
    {
        yield return new WaitForSeconds(explosionTime);

        //Check for null gameObject
        if(explosion != null)
        {
            //Create explosion
            Instantiate(explosion, transform.position, explosion.transform.rotation);
        }

        Destroy(gameObject);
    }
}
