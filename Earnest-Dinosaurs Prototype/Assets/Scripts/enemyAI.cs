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

    [Header("----- Enemy gun's Stats ------")]
    [SerializeField] GameObject bulletObject;
    [SerializeField] float shootingRate;

    Color modelOriginalColor; 
    Vector3 targetDirection;
    bool isShooting;

    // Start is called before the first frame update
    void Start()
    {
        modelOriginalColor = enemyModel.material.color;
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

        //When target is within the stopping distance, continue facing the target 
        if(navAgent.remainingDistance < navAgent.stoppingDistance)
        {
            faceTarget();
        }

        //** Only for testing damage feedback **
        if (Input.GetKeyDown("o"))
        {
            Debug.Log("Press O");
            takeDamage(1);
        }


        //Set the target position as destination 
        navAgent.SetDestination(targetPosition.position);
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
        StartCoroutine(damageFeedback());

        Debug.Log(gameObject.name + " take damage");

        //HP is zeo then destroy the enemy 
        if(HP <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator damageFeedback()
    {
        enemyModel.material.color = Color.red;

        yield return new WaitForSeconds(damageDuration);

        enemyModel.material.color = modelOriginalColor;
    }
}
