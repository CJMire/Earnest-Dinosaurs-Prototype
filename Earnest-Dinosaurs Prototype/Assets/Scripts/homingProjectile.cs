using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class homingProjectile : MonoBehaviour
{
    [Header("----- Bullet's Components ------")]
    [SerializeField] Rigidbody rb;
    [SerializeField] ParticleSystem hitEffect;
    [SerializeField] ParticleSystem flashEffect;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip shootEffect;
    [SerializeField] AudioClip detonateEffect;
    [Range(0, 1)][SerializeField] float bulletVol;

    [Header("----- Projectile's Stats ------")]
    [SerializeField] int projectileSpeed;
    [SerializeField] int rotationSpeed;
    [SerializeField] float projectileDuration;
    [SerializeField] int projectileDamage;

    Vector3 playerDirection;

    // Start is called before the first frame update
    void Start()
    {
        //Projectile travels to the player's position.
        transform.LookAt(gameManager.instance.player.transform.position);
        rb.velocity = transform.forward * projectileSpeed;

        //Sound from player 
        aud.PlayOneShot(shootEffect, bulletVol);

        //Destroy the bullet within this remaining time after shoot 
        Destroy(gameObject, projectileDuration);
    }

    private void Update()
    {
        //Get the angle to the player
        playerDirection = gameManager.instance.player.transform.position - transform.position;

        //Get rotation to the target
        Quaternion faceRotation = Quaternion.LookRotation(playerDirection);

        //Rotate to the target using lerp with set up speed
        transform.rotation = Quaternion.Lerp(transform.rotation, faceRotation, Time.deltaTime * rotationSpeed);

        //Update vector to go forwards
        rb.velocity = transform.forward * projectileSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Ignore all trigger
        if (other.isTrigger || other.CompareTag("Boss"))
        {
            return;
        }

        Debug.Log("Tag: " + other.gameObject.name);

        //Make the bullet damage the IDamage
        IDamage damageable = other.GetComponent<IDamage>();

        if (damageable != null && !other.CompareTag("Enemy"))
        {
            damageable.takeDamage(projectileDamage);
        }

        if (hitEffect != null)
        {
            //Detonate effect 
            Instantiate(hitEffect, transform.position, hitEffect.transform.rotation);

            //Sound from player 
            aud.PlayOneShot(detonateEffect, bulletVol);
        }

        //Destroy the projectile
        Destroy(gameObject);
    }
}