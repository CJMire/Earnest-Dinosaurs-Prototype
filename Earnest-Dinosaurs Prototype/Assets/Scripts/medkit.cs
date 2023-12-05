using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class medkit : MonoBehaviour
{
    [Header("----- Medkit's Stats ------")]
    [SerializeField] int healingAmount;
    [SerializeField] int medkitDuration;
    [SerializeField] int rotationSpeed;
    [Range(0f, 1f)][SerializeField] float flashOffTime;
    [Range(0f, 1f)][SerializeField] float flashOnTime;

    [Header("----- Medkit's Feedback")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip healSound;
    [SerializeField] ParticleSystem healingSplash;
    [Range(0, 1)][SerializeField] float healVol;

    int maxHP;
    float currentHP;
    float timeStamp;
    private MeshRenderer mRend;
    bool startedFlashing;

    // Start is called before the first frame update
    void Start()
    {
        //maxHP = gameManager.instance.playerScript.getPlayerMaxHP();

        //Temporary implementation, the getPlayerMaxHP keeps returning 0. 
        maxHP = 15;

        //needed for Flash() coroutine
        mRend = GetComponent<MeshRenderer>();
        startedFlashing = false;
        timeStamp = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        //Make the medkit rotate around 
        transform.Rotate(new Vector3(0.0f, 20.0f, 0.0f) * Time.deltaTime * rotationSpeed);

        //Update the current HP 
        currentHP = gameManager.instance.playerScript.getPlayerCurrentHP();

        if((Time.time - timeStamp) / medkitDuration >= .7f && !startedFlashing)
        {
            StartCoroutine(Flash());
        }

        //Destroy the medkit within this remaining time 
        Destroy(gameObject, medkitDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Ignore all trigger 
        if (other.isTrigger)
        {
            return;
        }

        //If player collide then heal the player and destroy game object 
        if(other.CompareTag("Player"))
        {
            //If player HP is more than maximum then don't pick
            if (currentHP < maxHP)
            {
                StartCoroutine(heal());
            }        
        }
    }

    IEnumerator heal()
    {
        //Heal sound 
        aud.PlayOneShot(healSound, healVol);

        //Heal player 
        gameManager.instance.playerScript.healPlayer(healingAmount);

        //Heal particle 
        if (healingSplash != null)
        {
            Instantiate(healingSplash, new Vector3(transform.position.x, transform.position.y + 1.15f, transform.position.z + 0.25f), transform.rotation);
        }

        //Wait 
        yield return new WaitForSeconds(0.2f);

        //Destroy gameObject
        Destroy(gameObject);
    }

    IEnumerator Flash()
    {
        if (!startedFlashing) { startedFlashing = true; }

        mRend.enabled = false;
        yield return new WaitForSeconds(flashOffTime);
        mRend.enabled = true;
        yield return new WaitForSeconds(flashOnTime);
        StartCoroutine(Flash());
    }
}
