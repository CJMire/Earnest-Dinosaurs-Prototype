using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shockwave : MonoBehaviour
{
    Vector3 scaleUp;

    [Header("----- Shockwave Stats -----")]
    [SerializeField] float expandRate;
    [Range(1, 10)][SerializeField] int damage;
    [Range(1, 25)][SerializeField] float liveTime;

    [Header("----- Audio Stats -----")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip explosion;
    [Range(0f, 1f)][SerializeField] float explosionVol;
    [SerializeField] AudioClip fadeOut;
    [Range(0f, 1f)][SerializeField] float fadeOutVol;

    // Start is called before the first frame update
    void Start()
    {
        scaleUp = new Vector3(expandRate, 0, expandRate);

        aud.PlayOneShot(explosion, explosionVol);
        aud.PlayOneShot(fadeOut, fadeOutVol);

        Destroy(gameObject, liveTime);
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

        if (damageable != null && other.CompareTag("Player") && gameManager.instance.player.transform.position.y < 1.75f)
        {
            damageable.takeDamage(damage);
        }
    }

    void expandExplosion()
    {
        transform.localScale += scaleUp * Time.deltaTime;
    }
}
