using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("----- Enemy's Components ------")]
    [SerializeField] Renderer enemyModel;
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Transform targetPosition;  //Player is not yet implemented so adding transform position for testing 
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
    bool isShooting;
    bool isDead;

    // Start is called before the first frame update
    void Start()
    {
        modelOriginalColor = enemyModel.material.color;
        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Get direction of the target 
        targetDirection = targetPosition.position - transform.position;

        //Keep shooting at the target 
        if(!isShooting)
        {
            StartCoroutine(shootTarget());
        }

        //Have enemy facing the target all the time because level is open space 
        faceTarget();

        //** Only for testing damage feedback **
        if (Input.GetKeyDown("o"))
        {
            Debug.Log("Toggle Damage");
            takeDamage(1);
            knockback();
        }

        //Need to stop setting destination when enemy is dead, might find better way to implement this. 
        if(!isDead)
        {
            //Set the target position as destination 
            navAgent.SetDestination(targetPosition.position);
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

        Debug.Log(gameObject.name + " take damage");

        //HP is zeo then destroy the enemy 
        if(HP <= 0)
        {
            //Spawn medkit within drop rate, set isDead and destroy gameObject 
            medkitDrop();
            isDead = true;
            Destroy(gameObject);
        }
    }

    IEnumerator damageFeedback()
    {
        enemyModel.material.color = Color.red;

        yield return new WaitForSeconds(damageDuration);

        enemyModel.material.color = modelOriginalColor;
    }

    public void knockback()
    {
        //Set velocity to opposite of facing target direction and multiply by force 
        navAgent.velocity = (targetDirection * -1.0f) * knockbackForce;

        //Set angular speed to zero (Enemy face not turning when knocked back)
        navAgent.angularSpeed = 0;
    }

    public void medkitDrop()
    {
        float drop = Random.Range(1, 100);

        if(drop <= dropRate)
        {
            Instantiate(medkitObject, transform.position, transform.rotation);
        }
    }
}
