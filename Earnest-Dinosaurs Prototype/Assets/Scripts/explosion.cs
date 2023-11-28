using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class explosion : MonoBehaviour
{
    [SerializeField] int damage;
    [SerializeField] ParticleSystem explosionEffect;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip explosionSound;
    [Range(0, 1)][SerializeField] float explosionVol;

    // Start is called before the first frame update
    void Start()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, explosionEffect.transform.rotation);
        }

        aud.PlayOneShot(explosionSound, explosionVol);

        Destroy(gameObject, 0.5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Ignore other trigger 
        if (other.isTrigger)
        {
            return;
        }

        IDamage damageable = other.GetComponent<IDamage>();

        if (damageable != null && !other.CompareTag("Enemy")) 
        {
            damageable.takeDamage(damage);
        }
    }
}
