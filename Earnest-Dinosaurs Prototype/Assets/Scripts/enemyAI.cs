using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("----- Enemy's Components ------")]
    [SerializeField] Renderer enemyModel;
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Transform shootPosition;
    [SerializeField] Transform headPos;
    //[SerializeField] Animator anim;

    [Header("----- Enemy's Stats ------")]
    [SerializeField] int HP;
    [SerializeField] int facingSpeed;
    [SerializeField] float damageDuration;
    [SerializeField] float knockbackForce;
    [SerializeField] int viewCone;
    [SerializeField] int shootCone;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTime;

    [Header("----- Enemy gun's Stats ------")]
    [SerializeField] GameObject bulletObject;
    [SerializeField] float shootingRate;

    [Header("----- Enemy Loot------")]
    [SerializeField] GameObject medkitObject;
    [SerializeField] float dropRate;

    Color modelOriginalColor; 
    Vector3 targetDirection;
    Vector3 wanderingDirection;
    bool isShooting;
    bool isDead;
    bool playerInRange;
    float angleToPlayer;
    float stoppingDisOrig;
    bool destinationChosen;
    Vector3 startingPos;


    // Start is called before the first frame update
    void Start()
    {
        modelOriginalColor = enemyModel.material.color;
        startingPos = transform.position;
        stoppingDisOrig = navAgent.stoppingDistance;
        isDead = false;
        gameManager.instance.updateEnemyCount(1);
    }

    // Update is called once per frame
    void Update()
    {
        //If agent is not on then don't do anything
        if (navAgent.isActiveAndEnabled)
        {
            //Player inside the sphere but not see the player 
            if (playerInRange && !canSeePlayer())
            {
                StartCoroutine(roam());
            }

            //Player outside the sphere 
            else if (!playerInRange)
            {
                StartCoroutine(roam());
            }
        }
    }

    bool canSeePlayer()
    {
        //Get player direction
        targetDirection = gameManager.instance.player.transform.position - headPos.position;

        //Get angle to the player except y-axis
        angleToPlayer = Vector3.Angle(new Vector3(targetDirection.x, 0, targetDirection.z), transform.forward);

        //For debuging enemy
        Debug.DrawRay(headPos.position, targetDirection);
        Debug.Log(angleToPlayer);

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

    IEnumerator roam()
    {
        if(navAgent.remainingDistance < 0.05f && !destinationChosen)
        {
            //Set the destination 
            destinationChosen = true;
            navAgent.stoppingDistance = 0;
            yield return new WaitForSeconds(roamPauseTime);

            //Get random position 
            Vector3 randomPos = Random.insideUnitSphere * roamDist;
            randomPos += startingPos;

            //Get position that's only on navmesh 
            NavMeshHit hit;
            NavMesh.SamplePosition(randomPos, out hit, roamDist, 1);
            navAgent.SetDestination(hit.position);

            destinationChosen = false;
        }
    }

    void faceTarget()
    {
        //Get rotation to the target 
        Quaternion faceRotation = Quaternion.LookRotation(targetDirection);

        //Rotate to the target using lerp with set up speed
        transform.rotation = Quaternion.Lerp(transform.rotation, faceRotation, Time.deltaTime * facingSpeed);
    }

    IEnumerator shootTarget()
    {
        isShooting = true;

        //Create a bullet at shooting position and current rotation 
        Instantiate(bulletObject, shootPosition.position, transform.rotation);

        //Shooting rate 
        yield return new WaitForSeconds(shootingRate);

        isShooting = false;
    }

    public void takeDamage(int damageAmount)
    {
        HP -= damageAmount;

        //Model damage red flash 
        StartCoroutine(damageFeedback());

        //If take damage,then chase the player 
        navAgent.SetDestination(gameManager.instance.player.transform.position);

        Debug.Log(gameObject.name + " take damage");

        knockback();

        //HP is zero then destroy the enemy 
        if (HP <= 0)
        {
            //Spawn medkit within drop rate, set isDead and destroy gameObject 
            medkitDrop();
            isDead = true;
            Destroy(gameObject);
            gameManager.instance.updateEnemyCount(-1);
        }
    }

    IEnumerator damageFeedback()
    {
        enemyModel.material.color = Color.red;

        yield return new WaitForSeconds(damageDuration);

        enemyModel.material.color = modelOriginalColor;
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

    void medkitDrop()
    {
        float drop = Random.Range(1, 100);

        if(drop <= dropRate)
        {
            Instantiate(medkitObject, transform.position, transform.rotation);
        }
    }
}
