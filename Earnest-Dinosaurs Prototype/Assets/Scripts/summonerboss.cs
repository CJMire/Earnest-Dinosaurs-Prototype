using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.FilePathAttribute;

public class summonerboss : MonoBehaviour, IDamage
{
    [Header("----- Boss's Components ------")]
    [SerializeField] Renderer[] bossModelArray;
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Transform headPos;
    [SerializeField] Transform torsoPos;
    [SerializeField] Animator anim;
    [SerializeField] Collider damageCol;
    [SerializeField] AudioSource aud;

    [Header("----- Boss's Stats ------")]
    [SerializeField] int HP;
    [SerializeField] int facingSpeed;
    [SerializeField] float damageDuration;
    [SerializeField] float stuntTime;
    [SerializeField] float knockbackForce;
    [SerializeField] int viewCone;
    [SerializeField] int shootCone;

    [Header("----- Boss attack's Stats ------")]
    [SerializeField] int maxSpawn;

    [Header("----- Boss's Stats ------")]
    [SerializeField] ParticleSystem spawnParticle;
    [SerializeField] ParticleSystem deathParticle;
    [SerializeField] ParticleSystem damageParticle;
    [SerializeField] ParticleSystem lightingParticle;

    [Header("----- Boss barrier's Stats ------")]
    [SerializeField] GameObject barrierObject;
    [SerializeField] ParticleSystem barrierParticle;
    [SerializeField] Renderer barrierRenderer;
    [SerializeField] AudioClip barrierDestroy;

    [Header("----- Summoning enemy------")]
    [SerializeField] GameObject[] summonedEnemy;
    [SerializeField] GameObject[] eliteDrones;

    [Header("----- EMP Burst-----")]
    [SerializeField] GameObject empBurstObject;
    [SerializeField] ParticleSystem empBurstParticle;

    [Header("----- Boss's Sounds------")]
    [SerializeField] AudioClip fearDeathSound;
    [SerializeField] AudioClip hurtSound;
    [SerializeField] AudioClip spawnSound;
    [SerializeField] AudioClip deadSound;
    [SerializeField] AudioClip explosionSound;
    [SerializeField] AudioClip EMPSound;
    [SerializeField] AudioClip aboutToEMPSound;
    [SerializeField] AudioClip barrierRecoverSound;
    [Range(0, 1)][SerializeField] float bossVol;

    //Private variable 
    Color[] modelOrigColor;
    Color barrierOrigColor;

    Vector3 targetDirection;
    bool isSummoning;
    bool isDead;
    bool isStunt;
    bool playerInSummonRange;
    int maxHP;
    float angleToPlayer;
    float stoppingDisOrig;
    int eliteDroneStatus;
    Vector3 startingPos;

    // Start is called before the first frame update
    void Start()
    {
        //Store different model parts information 
        modelOrigColor = new Color[bossModelArray.Length];

        for (int i = 0; i < bossModelArray.Length; i++)
        {
            modelOrigColor[i] = bossModelArray[i].material.color;
        }

        //barrierOrigColor = barrierRenderer.material.color;
        maxHP = HP;
        startingPos = transform.position;
        stoppingDisOrig = navAgent.stoppingDistance;
        isDead = false;
        isStunt = false;


        //Spawning in effects 
        for (int i = 0; i < 10; i++)
        {
            Instantiate(deathParticle, transform.position + new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)), transform.rotation);
        }

        aud.PlayOneShot(explosionSound, bossVol);
    }

    // Update is called once per frame
    void Update()
    {
        gameManager.instance.SetBossHealth(HP, maxHP);

        //If agent is not on then don't do anything
        if (navAgent.isActiveAndEnabled && !isDead && !isStunt)
        {
            //All elite drone death 
            if (trackEliteDrone() == 0)
            {
                Debug.Log("Stunt and Summon");
                StartCoroutine(stuntAndSummon());
            }

            //Set the model animation speed along with its navAgent normalized velocity 
            anim.SetFloat("Speed", navAgent.velocity.normalized.magnitude);

            //Player inside the sphere but not see the player 
            if (!canSeePlayer())
            {
                seekAndAvoid();
            }

            seekAndAvoid();
        }

        //Floating up then teleport away 
        if(isDead)
        {
            transform.Translate(Vector3.up * Time.deltaTime);
        }
    }

    bool canSeePlayer()
    {
        //Get player direction
        targetDirection = gameManager.instance.player.transform.position - headPos.position;

        //Get angle to the player except y-axis
        angleToPlayer = Vector3.Angle(new Vector3(targetDirection.x, 0, targetDirection.z), transform.forward);

        //Raycast checking 
        RaycastHit hit;

        if (Physics.Raycast(headPos.position, targetDirection, out hit))
        {
            //If the player is within the view cone 
            if (hit.collider.CompareTag("Player") && angleToPlayer <= viewCone)
            {
                navAgent.stoppingDistance = stoppingDisOrig;

                //Shoot the player within the shoot cone 
                if (angleToPlayer <= shootCone && playerInSummonRange)
                {
                    if(gameManager.instance.GetEnemyCount() < maxSpawn && !isStunt)
                    {
                        anim.SetTrigger("Summon");
                    }
                }

                //Do this when player is in stopping distance 
                if (navAgent.remainingDistance < navAgent.stoppingDistance)
                {
                    faceTarget();
                }

                //Need to stop setting destination when enemy is dead, might find better way to implement this. 
                if (!isDead)
                {
                    //Set the target position as destination 
                    navAgent.SetDestination(gameManager.instance.player.transform.position);
                }

                return true;
            }
        }

        navAgent.stoppingDistance = 0.0f;

        return false;
    }

    void seekAndAvoid()
    {
        if (!isDead && !isStunt)
        {
            faceTarget();

            float distance = Vector3.Distance(transform.position, gameManager.instance.player.transform.position);

            //Always go To Player
            navAgent.SetDestination(gameManager.instance.player.transform.position);
        }
    }

    void faceTarget()
    {
        //Get rotation to the target 
        Quaternion faceRotation = Quaternion.LookRotation(targetDirection);

        //Rotate to the target using lerp with set up speed
        transform.rotation = Quaternion.Lerp(transform.rotation, faceRotation, Time.deltaTime * facingSpeed);
    }

    int trackEliteDrone()
    {
        int currentDrone = 0;

        for(int i = 0; i < eliteDrones.Length; i++)
        {
            if (eliteDrones[i].activeSelf)
            {
                currentDrone++;
            }
        }

        return currentDrone;
    }

    IEnumerator stuntAndSummon()
    {
        aud.PlayOneShot(aboutToEMPSound, bossVol);
        isStunt = true;
        damageCol.enabled = false;
        anim.SetBool("Stunt", true);
        stunt();

        yield return new WaitForSeconds(stuntTime);

        anim.SetBool("Stunt", false);
    }

    void stunt()
    {
        barrierObject.SetActive(false);
        navAgent.SetDestination(transform.position);
    }

    void eliteSummon()
    {
        if(!isDead)
        {
            for(int i = 0; i < eliteDrones.Length; i++)
            {
                eliteDrones[i].GetComponent<eliteDrone>().respawn();
            }

            barrierObject.SetActive(true);

            isStunt = false;

            aud.PlayOneShot(barrierRecoverSound, bossVol);
        }
    }

    public void createDroneEnemy()
    {
        //Pick random drone to spawn 
        int randomDrone = Random.Range(0, summonedEnemy.Length - 1);

        //Create vector 3 of spawn position 
        float positionX = transform.position.x + Random.Range(-4.0f, 4.0f);
        float positionY = transform.position.y + 3.0f;
        float positionZ = transform.position.z + Random.Range(-4.0f, 4.0f);

        Vector3 spawnPosition = new Vector3(positionX, positionY, positionZ);

        //Add variable name to access the SetBarrierHP function 
        GameObject droneClone;
        droneClone = Instantiate(summonedEnemy[randomDrone], spawnPosition, transform.rotation);

        Instantiate(spawnParticle, spawnPosition, transform.rotation);

        aud.PlayOneShot(spawnSound, bossVol);

        //Setting enemy barrier 
        int barrierChance = Random.Range(0, 100);
 
        if (barrierChance < 20)
        {
            droneClone.GetComponent<enemyAI>().SetBarrierHP(3);
        }

        else
        {
            droneClone.GetComponent<enemyAI>().SetBarrierHP(0);
        }
    }

    public void empBurst()
    {
        Instantiate(empBurstObject, transform.position, transform.rotation);
        Instantiate(empBurstParticle, transform.position, transform.rotation);
        aud.PlayOneShot(EMPSound, bossVol);
        damageCol.enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInSummonRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInSummonRange = false;
            navAgent.stoppingDistance = 0.0f;
        }
    }

    public void takeDamage(int damageAmount)
    {
         HP -= damageAmount;

         //Model damage red flash 
         StartCoroutine(damageFeedback());

         //HP is zero then destroy the enemy 
         if (HP <= 0)
         {
             //Spawn medkit within drop rate, set isDead and destroy gameObject 
             if(deadSound != null)
             {
                 aud.PlayOneShot(deadSound, bossVol);
             }

             isDead = true;
             navAgent.enabled = false;
             anim.SetBool("Dead", true);
             aud.PlayOneShot(deadSound, bossVol);

             //turns off enemy damage colliders when dead
             damageCol.enabled = false;
             StartCoroutine(OnDeath());
         }

         else
         {
             //Play damage animation
             int damageAnimation = Random.Range(1, 4);
             playDamageAnimaton(damageAnimation);

             //anim.SetTrigger("Damage");

             aud.PlayOneShot(hurtSound, bossVol);

             if (deadSound != null)
             {
                 aud.PlayOneShot(hurtSound, bossVol);
             }

             //If take damage,then chase the player 
             if (!isDead)
             {
                 navAgent.SetDestination(gameManager.instance.player.transform.position);
             }
         }
    }

    IEnumerator damageFeedback()
    {
        for (int i = 0; i < bossModelArray.Length; i++)
        {
            bossModelArray[i].material.color = Color.red;
        }

        Instantiate(damageParticle, torsoPos.position, transform.rotation);

        damageCol.enabled = false;

        yield return new WaitForSeconds(damageDuration);

        for (int i = 0; i < bossModelArray.Length; i++)
        {
            bossModelArray[i].material.color = modelOrigColor[i];
        }

        damageCol.enabled = true;
    }

    IEnumerator OnDeath()
    {
        aud.PlayOneShot(fearDeathSound, bossVol);
        Instantiate(lightingParticle, torsoPos.position, lightingParticle.transform.rotation);

        yield return new WaitForSeconds(4.0f);

        for(int i = 0; i < 10; i++)
        {
            Instantiate(deathParticle, transform.position + new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)), transform.rotation);
        }

        aud.PlayOneShot(explosionSound, bossVol);

        for (int i = 0; i < bossModelArray.Length; i++)
        {
            bossModelArray[i].enabled = false;
        }

        yield return new WaitForSeconds(2.0f);

        Destroy(gameObject);
    }

    void playDamageAnimaton(int damageAnim)
    {
        if(damageAnim == 1)
        {
            anim.SetTrigger("Damage1");
        }
        
        else if(damageAnim == 2) 
        {
            anim.SetTrigger("Damage2");
        }

        else if (damageAnim == 3)
        {
            anim.SetTrigger("Damage3");
        }

        else
        {
            //Shouldn't get here
            Debug.Log("SummonerBoss Error Damage Animation");
        }
    }
}
