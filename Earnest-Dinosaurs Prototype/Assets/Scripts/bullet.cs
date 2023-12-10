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
    [SerializeField] ParticleSystem flashEffect;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] shootEffects;
    [SerializeField] AudioClip[] ricochetEffect;
    [Range(0, 1)][SerializeField] float bulletVol;

    [Header("----- Bullet's Stats ------")]
    [SerializeField] int bulletSpeed;
    [SerializeField] float bulletDuration;
    [SerializeField] int bulletDamage;
    [SerializeField] bool isShotgun;
    [SerializeField] float shotgunRandomValue;

    // Start is called before the first frame update
    void Start()
    {
        if (!isShotgun)
        {
            //Bullet travel to the player's position. Added verticality to the enemy shooting. 
            rb.velocity = (gameManager.instance.player.transform.position - transform.position).normalized * bulletSpeed;
        }

        else
        {
            //Spread the bullet in random direction for shotgun spreading 
            float targetPositionX = gameManager.instance.player.transform.position.x + Random.Range(-shotgunRandomValue, shotgunRandomValue);
            float targetPositionY = gameManager.instance.player.transform.position.y + Random.Range(-shotgunRandomValue, shotgunRandomValue);
            float targetPositionZ = gameManager.instance.player.transform.position.z + Random.Range(-shotgunRandomValue, shotgunRandomValue);

            //The position after spread 
            Vector3 targetPositonSpread = new Vector3(targetPositionX, targetPositionY, targetPositionZ);

            rb.velocity = (targetPositonSpread - transform.position).normalized * bulletSpeed;

            if(flashEffect != null)
            {
                Instantiate(flashEffect, transform.position, transform.rotation);
            }
        }

        //Sound from player 
        int randomShootSound = Random.Range(0, shootEffects.Length); 
        aud.PlayOneShot(shootEffects[randomShootSound], bulletVol);

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

        if(damageable != null && !other.CompareTag("Enemy") && !other.CompareTag("Boss") && !other.CompareTag("EliteDrone"))
        {
            damageable.takeDamage(bulletDamage);
        }

        if(hitEffect != null)
        {
            //Bullet hit effect 
            Instantiate(hitEffect, transform.position, hitEffect.transform.rotation);

            //Sound from player 
            int randomRicoSound = Random.Range(0, ricochetEffect.Length);
            aud.PlayOneShot(ricochetEffect[randomRicoSound], bulletVol);
        }
        
        //Destroy the bullet when hitting the gameObject 
        Destroy(gameObject, 0.5f);
    }
}