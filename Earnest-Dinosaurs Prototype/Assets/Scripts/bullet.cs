using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class bullet : MonoBehaviour
{
    [Header("----- Bullet's Components ------")]
    [SerializeField] Rigidbody rb;

    [Header("----- Bullet's Stats ------")]
    [SerializeField] int bulletSpeed;
    [SerializeField] int bulletDuration;

    // Start is called before the first frame update
    void Start()
    {
        //Bullet travel at the gun's direction with given speed 
        rb.velocity = transform.forward * bulletSpeed;

        //Destroy the bullet within this remaining time after shoot 
        Destroy(gameObject, bulletDuration);
    }
    private void OnTriggerEnter(Collider other)
    {
        //Ignore all trigger 
        if(other.isTrigger)
        {
            return;
        }

        //Destroy the bullet when hitting the gameObject 
        Destroy(gameObject);
    }
}