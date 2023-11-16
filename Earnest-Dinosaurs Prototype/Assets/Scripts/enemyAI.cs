using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("----- Enemy's Components ------")]
    [SerializeField] Renderer enemyModel;
    [SerializeField] Renderer enemyModel_1;
    [SerializeField] Renderer enemyModel_2;
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Transform shootPosition;
    [SerializeField] Transform headPos;
    [SerializeField] Animator anim;
    [SerializeField] Collider damageCol;
    [SerializeField] AudioSource aud;

    [Header("----- Enemy's Stats ------")]
    [SerializeField] int HP;
    [SerializeField] int facingSpeed;
    [SerializeField] float damageDuration;
    [SerializeField] float knockbackForce;
    [SerializeField] int viewCone;
    [SerializeField] int shootCone;

    [Header("----- Enemy gun's Stats ------")]
    [SerializeField] GameObject bulletObject;
    [SerializeField] float shootingRate;

    [Header("----- Enemy Loot------")]
    [SerializeField] GameObject medkitObject;
    [Range(1,100)][SerializeField] float medkitDropRate;
    [SerializeField] GameObject speedPickupObject;
    [Range(1, 100)][SerializeField] float speedDropRate;
    [SerializeField] GameObject invincibilityPickupObject;
    [Range(1, 100)][SerializeField] float invincibilityDropRate;
    [SerializeField] GameObject damagePickupObject;
    [Range(1, 100)][SerializeField] float damageDropRate;


    [Header("----- Enemy Sound------")]
    [SerializeField] AudioClip hurtSound;
    [SerializeField] AudioClip deadSound;
    [Range(0, 1)][SerializeField] float enemyVol;

    Color modelOriginalColor;
    Color modelOriginalColor_1;
    Color modelOriginalColor_2;
    Vector3 targetDirection;
    bool isShooting;
    bool isDead;
    bool playerInRange;
    float angleToPlayer;
    float stoppingDisOrig;
    Vector3 startingPos;


    void Start()
    {
        //Main character 
        modelOriginalColor = enemyModel.material.color;

        //Model with more than one part 
        if(enemyModel_1 != null)
        {
            modelOriginalColor_1 = enemyModel_1.material.color;
        }

        if (enemyModel_2 != null)
        {
            modelOriginalColor_2 = enemyModel_2.material.color;
        }

        startingPos = transform.position;
        stoppingDisOrig = navAgent.stoppingDistance;
        isDead = false;
        gameManager.instance.updateEnemyCount(1);
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
                seek();
            }

            seek();
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
                if (angleToPlayer <= shootCone && !isShooting)
                {
                    StartCoroutine(shootTarget());
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

    void seek()
    {
        if(!isDead)
        {
            //Always go To Player
            faceTarget();
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

    public void createBullet()
    {
        //Create a bullet at shooting position and current rotation 
        Instantiate(bulletObject, shootPosition.position, transform.rotation);
    }

    IEnumerator shootTarget()
    {
        isShooting = true;

        anim.SetTrigger("Shoot");

        //Shooting rate 
        yield return new WaitForSeconds(shootingRate);

        isShooting = false;
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
            aud.PlayOneShot(deadSound, enemyVol);

            DropSomething(); //Drops one puick-up for player use

            isDead = true;
            navAgent.enabled = false;
            anim.SetBool("Dead", true);

            //turns off enemy damage colliders when dead
            damageCol.enabled = false;

            StartCoroutine(OnDeath());

            //Destroy(gameObject);
            gameManager.instance.updateEnemyCount(-1);
        }

        else
        {

            //Play damage animation
            anim.SetTrigger("Damage");

            aud.PlayOneShot(hurtSound, enemyVol);

            //If take damage,then chase the player 
            if (!isDead)
            {
                navAgent.SetDestination(gameManager.instance.player.transform.position);
            }

            knockback();
        }
    }

    IEnumerator damageFeedback()
    {
        enemyModel.material.color = Color.red;

        if (enemyModel_1 != null)
        {
            enemyModel_1.material.color = Color.red;
        }

        if (enemyModel_2 != null)
        {
            enemyModel_2.material.color = Color.red;
        }

        yield return new WaitForSeconds(damageDuration);

        enemyModel.material.color = modelOriginalColor;

        if (enemyModel_1 != null)
        {
            enemyModel_1.material.color = modelOriginalColor_1;
        }

        if (enemyModel_2 != null)
        {
            enemyModel_2.material.color = modelOriginalColor_2;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            navAgent.stoppingDistance = 0.0f;
        }
    }

    void knockback()
    {
        //Set velocity to opposite of facing target direction and multiply by force 
        navAgent.velocity = (targetDirection * -1.0f) * knockbackForce;

        //Set angular speed to zero (Enemy face not turning when knocked back)
        navAgent.angularSpeed = 0;
    }

    //Made from Chayathorn's Medkitdrop() method & Cameron's SpeedPickupDrop() and InvincibilityPickupDrop() methods
    //invincibility should be the hardest to get & damage should be 2nd hardest
    //speed should be the 3rd hardest & the medkit should be the easiest
    //However, enemies should only drop 1 thing at time
    //I have set the drop-rates with changable ranges for testing
    void DropSomething()
    {
        float drop = UnityEngine.Random.Range(1, 100);
        if(drop <= invincibilityDropRate)
        {
            Instantiate(invincibilityPickupObject, transform.position + new Vector3(0, 1.5f), transform.rotation);
        }
        else if(drop <= speedDropRate)
        {
            Instantiate(speedPickupObject, transform.position + new Vector3(0, 1.5f), transform.rotation);
        }
        else if(drop <= medkitDropRate)
        {
            Instantiate(medkitObject, transform.position, transform.rotation);
        }
        else if(drop <= damageDropRate)
        {
            Instantiate(damagePickupObject, transform.position + new Vector3(0, 1.5f), transform.rotation);
        }
    }

    //Destroys gameObject after set amount of time
    IEnumerator OnDeath()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
