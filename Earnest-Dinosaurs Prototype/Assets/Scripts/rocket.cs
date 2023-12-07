using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class rocket : MonoBehaviour
{
    [Header("----- Rocket's Components ------")]
    [SerializeField] Rigidbody rb;
    [SerializeField] GameObject explosion;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip shootEffect;
    [SerializeField] AudioClip rocketFlying; 
    [Range(0, 1)][SerializeField] float rocketVol;

    [Header("----- Rocket's Stats ------")]
    [SerializeField] int rocketSpeed;
    [SerializeField] float rocketDuration;

    // Start is called before the first frame update
    void Start()
    {
        //Bullet travel to the player's position. Added verticality to the enemy shooting. 
        rb.velocity = (gameManager.instance.player.transform.position - transform.position).normalized * rocketSpeed;

        //Sound from player 
        aud.PlayOneShot(shootEffect, rocketVol);

        aud.PlayOneShot(rocketFlying, rocketVol);

        //Destroy the bullet within this remaining time after shoot 
        Destroy(gameObject, rocketDuration);
    }

    // Update is called once per frame
    private void OnTriggerExit(Collider other)
    {
        //Ignore all trigger
        if (other.isTrigger || other.CompareTag("Enemy"))
        {
            return;
        }

        //Create explosion
        Instantiate(explosion, transform.position, explosion.transform.rotation);

        //Destroy the bullet when hitting the gameObject 
        Destroy(gameObject);
    }
}
