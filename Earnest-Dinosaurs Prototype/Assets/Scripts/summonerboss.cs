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
    [SerializeField] Animator anim;
    [SerializeField] Collider damageCol;
    [SerializeField] AudioSource aud;

    [Header("----- Boss's Stats ------")]
    [SerializeField] int HP;
    [SerializeField] int facingSpeed;
    [SerializeField] float damageDuration;
    [SerializeField] float knockbackForce;
    [SerializeField] int viewCone;
    [SerializeField] int shootCone;

    [Header("----- Boss attack's Stats ------")]
    [SerializeField] float spawnRate;
    [SerializeField] int maxSpawn;

    [Header("----- Boss's Stats ------")]
    [SerializeField] ParticleSystem spawnParticle;
    [SerializeField] ParticleSystem deathParticle;

    [Header("----- Boss barrier's Stats ------")]
    [SerializeField] GameObject barrierObject;
    [SerializeField] ParticleSystem barrierParticle;
    [SerializeField] Renderer barrierRenderer;
    [SerializeField] AudioClip barrierDestroy;
    [SerializeField] int barrierHP;

    [Header("----- Summoning enemy------")]
    [SerializeField] GameObject[] summonedEnemy;
    [SerializeField] GameObject eliteDrone1;
    [SerializeField] GameObject eliteDrone2;
    [SerializeField] GameObject eliteDrone3;

    [Header("----- EMP Burst-----")]
    [SerializeField] GameObject empBurstObject;

    [Header("----- Boss's Sounds------")]
    [SerializeField] AudioClip hurtSound;
    [SerializeField] AudioClip deadSound;
    [Range(0, 1)][SerializeField] float bossVol;

    //Private variable 
    Color[] modelOrigColor;
    Color barrierOrigColor;

    Vector3 targetDirection;
    bool isSummoning;
    bool isDead;
    bool playerInSummonRange;
    float angleToPlayer;
    float stoppingDisOrig;
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

        startingPos = transform.position;
        stoppingDisOrig = navAgent.stoppingDistance;
        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        //If agent is not on then don't do anything
        if (navAgent.isActiveAndEnabled && !isDead)
        {
            //Set the model animation speed along with its navAgent normalized velocity 
            anim.SetFloat("Speed", navAgent.velocity.normalized.magnitude);

            //Player inside the sphere but not see the player 
            if (!canSeePlayer())
            {
                seekAndAvoid();
            }

            seekAndAvoid();
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
                if (angleToPlayer <= shootCone && !isSummoning && playerInSummonRange && gameManager.instance.GetEnemyCount() < maxSpawn)
                {
                    Debug.Log("Summon");
                    StartCoroutine(summonMinion());
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
        if (!isDead)
        {
            faceTarget();

            float distance = Vector3.Distance(transform.position, gameManager.instance.player.transform.position);

            if(distance < stoppingDisOrig)
            {
                //WIP on making the enemy avoid the player when player get too close 
                Vector3 behindPosition = ((targetDirection * -1.0f) * 10.0f) + transform.position;
                navAgent.SetDestination(behindPosition);
            }

            else
            {
                //Always go To Player
                navAgent.SetDestination(gameManager.instance.player.transform.position);
            }
        }
    }

    void faceTarget()
    {
        //Get rotation to the target 
        Quaternion faceRotation = Quaternion.LookRotation(targetDirection);

        //Rotate to the target using lerp with set up speed
        transform.rotation = Quaternion.Lerp(transform.rotation, faceRotation, Time.deltaTime * facingSpeed);
    }

    IEnumerator summonMinion()
    {
        isSummoning = true;

        anim.SetTrigger("Summon");

        yield return new WaitForSeconds(spawnRate);

        isSummoning = false;
    }

    void summonEliteDrone()
    {
        eliteDrone1.GetComponent<eliteDrone>().fullRegen();
        eliteDrone1.SetActive(true);

        eliteDrone2.GetComponent<eliteDrone>().fullRegen();
        eliteDrone2.SetActive(true);

        eliteDrone3.GetComponent<eliteDrone>().fullRegen();
        eliteDrone3.SetActive(true);
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

        //Setting enemy barrier 
        int barrierChance = Random.Range(0, 100);
 
        if (barrierChance < 50)
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

        empBurst();

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

             //turns off enemy damage colliders when dead
             damageCol.enabled = false;

             //Destroy(gameObject);
             gameManager.instance.updateEnemyCount(-1);
         }

         else
         {
             //Play damage animation
             anim.SetTrigger("Damage");

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

        yield return new WaitForSeconds(damageDuration);

        for (int i = 0; i < bossModelArray.Length; i++)
        {
            bossModelArray[i].material.color = modelOrigColor[i];
        }
    }

    IEnumerator OnDeath()
    {
        yield return new WaitForSeconds(1.0f);

        Instantiate(deathParticle, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
