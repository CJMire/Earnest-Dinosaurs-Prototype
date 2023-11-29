using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class grenade : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] float explosionTime;
    [SerializeField] GameObject explosion;

    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip countdownSound;
    [Range(0, 1)][SerializeField] float countdownVol;

    float countdownTick;
    bool countinDown;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 playerTorosPos = gameManager.instance.player.transform.position + Vector3.up;

        float distance = Vector3.Distance(transform.position, playerTorosPos);

        Debug.Log("Distance:" + distance);

        //Grenade projectile motion 
        rb.velocity = (Vector3.up * 2.0f) + (transform.forward) * (Mathf.Clamp(distance, 0.0f, 17.5f) * 2.0f);

        countdownTick = explosionTime;

        StartCoroutine(aboutToExplode(countdownTick));

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

    IEnumerator aboutToExplode(float duration)
    {
        while(true)
        {
            duration *= 0.5f;

            aud.PlayOneShot(countdownSound, countdownVol);

            yield return new WaitForSeconds(duration);
        }
    }
}
