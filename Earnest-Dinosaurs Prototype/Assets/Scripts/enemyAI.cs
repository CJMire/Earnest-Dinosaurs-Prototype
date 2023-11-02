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

    [Header("----- Enemy's Stats ------")]
    [SerializeField] int HP;
    [SerializeField] int facingSpeed;
    [SerializeField] float damageDuration;
    [SerializeField] float knockbackForce;

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

    // Start is called before the first frame update
    void Start()
    {
        modelOriginalColor = enemyModel.material.color;
        isDead = false;
        gameManager.instance.updateEnemyCount(1);
        wanderingDirection = randomLocation();
    }

    // Update is called once per frame
    void Update()
    {
        //If player in range then attack the player 
        if(playerInRange)
        {
            //Get direction of the target 
            targetDirection = gameManager.instance.player.transform.position - transform.position;

            //Keep shooting at the target 
            if (!isShooting)
            {
                StartCoroutine(shootTarget());
            }

            //Have enemy facing the target all the time because level is open space 
            faceTarget();

            //Need to stop setting destination when enemy is dead, might find better way to implement this. 
            if (!isDead)
            {
                //Set the target position as destination 
                navAgent.SetDestination(gameManager.instance.player.transform.position);
            }
        }

        //Wandering around spawnpoint 
        else
        {
            //Need to stop setting destination when enemy is dead, might find better way to implement this. 
            if (!isDead)
            {
                //Get new location when getting near the wandering position 
                if(navAgent.remainingDistance < navAgent.stoppingDistance)
                {
                    wanderingDirection = randomLocation();
                }

                //Continue wandering 
                wandering();
            }
        }
    }

    void wandering()
    {
        //Set the target position as destination 
        navAgent.SetDestination(wanderingDirection);
    }

    Vector3 randomLocation()
    {
        //Make it within -40 and 40 for now 
        float random_x = Random.Range(-40, 40);
        float random_z = Random.Range(-40, 40);

        Vector3 random = new Vector3(random_x, transform.position.y, random_z);

        return random;
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

        //If take damage, then chase the player 
        navAgent.SetDestination(gameManager.instance.player.transform.position);

        Debug.Log(gameObject.name + " take damage");

        knockback();

        //HP is zeo then destroy the enemy 
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
