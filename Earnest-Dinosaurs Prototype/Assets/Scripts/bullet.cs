using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class bullet : MonoBehaviour
{
    [Header("----- Bullet's Components ------")]
    [SerializeField] Rigidbody rb;
    [SerializeField] ParticleSystem hitEffect;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip shootEffect;
    [SerializeField] AudioClip ricochetEffect;
    [Range(0, 1)][SerializeField] float bulletVol;

    [Header("----- Bullet's Stats ------")]
    [SerializeField] int bulletSpeed;
    [SerializeField] int bulletDuration;
    [SerializeField] int bulletDamage;

    // Start is called before the first frame update
    void Start()
    {
        //Bullet travel at the gun's direction with given speed 
        rb.velocity = transform.forward * bulletSpeed;

        //Sound from player 
        aud.PlayOneShot(shootEffect, bulletVol);

        //Destroy the bullet within this remaining time after shoot 
        Destroy(gameObject, bulletDuration);
    }
    private void OnTriggerEnter(Collider other)
    {
        //Ignore all trigger
        if (other.isTrigger)
        {
            return;
        }

        //Make the bullet damage the IDamage
        IDamage damageable = other.GetComponent<IDamage>();

        if(damageable != null && !other.CompareTag("Enemy"))
        {
            damageable.takeDamage(bulletDamage);
        }

        if(hitEffect != null)
        {
            //Bullet hit effect 
            Instantiate(hitEffect, transform.position, hitEffect.transform.rotation);

            //Sound from player 
            aud.PlayOneShot(ricochetEffect, bulletVol);
        }
        
        //Destroy the bullet when hitting the gameObject 
        Destroy(gameObject, 0.5f);
    }
}