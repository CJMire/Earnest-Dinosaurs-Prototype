using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class eliteDrone : MonoBehaviour, IDamage
{
    [Header("----- Drone's Components ------")]
    [SerializeField] Renderer[] enemyModelArray;
    [SerializeField] Transform headPos;
    [SerializeField] Transform shootPos;
    [SerializeField] Animator anim;
    [SerializeField] Collider damageCol;
    [SerializeField] AudioSource aud;
    [SerializeField] GameObject summoner;
    [SerializeField] GameObject summonerGround;
    [SerializeField] bool MasterPrimeMode;
    [SerializeField] LineRenderer shieldBeam;
    [SerializeField] Transform startPosition;

    [Header("----- Drone's Stats ------")]
    [SerializeField] int HP;
    [SerializeField] float damageDuration;

    [Header("----- Drone's Exlusive Component ------")]
    [SerializeField] ParticleSystem droneDeathParticle;

    [Header("----- Enemy Sound------")]
    [SerializeField] AudioClip hurtSound;
    [SerializeField] AudioClip deadSound;
    [Range(0, 1)][SerializeField] float enemyVol;

    Color[] modelOrigColor;
    Color barrierOrigColor;

    Vector3 targetDirection;
    Vector3 origStartPosition;
    bool isShooting;
    int maxHP;
    float orbitingSpeed;

    // Start is called before the first frame update
    void Start()
    {
        //Store different model parts information 
        modelOrigColor = new Color[enemyModelArray.Length];

        for (int i = 0; i < enemyModelArray.Length; i++)
        {
            modelOrigColor[i] = enemyModelArray[i].material.color;
        }

        maxHP = HP;
        orbitingSpeed = 50;

        origStartPosition = startPosition.position;
    }

    // Update is called once per frame
    void Update()
    {
        shieldBeam.SetPosition(0, shootPos.transform.position);
        shieldBeam.SetPosition(1, summoner.transform.position);

        Debug.Log("transform position y: " + transform.position.y);

        droneMovement();
    }

    void droneMovement()
    {
        if(MasterPrimeMode)
        {
            Debug.Log("MasterPrime Mode");

            //Orbiting the boss 
            transform.RotateAround(summoner.transform.position, Vector3.up, 75.0f * Time.deltaTime);
        }

        else
        {
            //Orbiting the boss 
            transform.RotateAround(summoner.transform.position, Vector3.up, orbitingSpeed * Time.deltaTime);
        }
        
    }

    public void takeDamage(int damageAmount)
    {
        HP -= damageAmount;

        Debug.Log("HP:" + HP + " Damage amount: " + damageAmount);
        Debug.Log(HP -= damageAmount);

        //Model damage red flash 
        StartCoroutine(damageFeedback());

        //HP is zero then destroy the enemy 
        if (HP <= 0)
        {
            //Spawn medkit within drop rate, set isDead and destroy gameObject 
            aud.PlayOneShot(deadSound, enemyVol);
            
            //anim.SetBool("Dead", true);
            StartCoroutine(OnDroneDeath());

            //turns off enemy damage colliders when dead
            damageCol.enabled = false;
        }

        else
        {
            //Play damage animation
            anim.SetTrigger("Damage");

            aud.PlayOneShot(hurtSound, enemyVol);
        }
    }

    IEnumerator damageFeedback()
    {
        Debug.Log("Damage Feedback");

        for (int i = 0; i < enemyModelArray.Length; i++)
        {
            enemyModelArray[i].material.color = Color.red;
        }

        yield return new WaitForSeconds(damageDuration);

        for (int i = 0; i < enemyModelArray.Length; i++)
        {
            enemyModelArray[i].material.color = modelOrigColor[i];
        }
    }

    IEnumerator OnDroneDeath()
    {
        //The delay splash in the game is because the animation is playing, not the code here. 

        yield return new WaitForSeconds(0.5f);

        if (droneDeathParticle != null)
        {
            Instantiate(droneDeathParticle, transform.position, transform.rotation);
        }

        gameObject.SetActive(false);
    }

    public void respawn()
    {
        Vector3 position = new Vector3(summonerGround.transform.position.x + origStartPosition.x, summonerGround.transform.position.y + 6.0f, summonerGround.transform.position.z + origStartPosition.z);
        transform.position = position;
        HP = maxHP;
        gameObject.SetActive(true);
        damageCol.enabled = true;
    }
}
