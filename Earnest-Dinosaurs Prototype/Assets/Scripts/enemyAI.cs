using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour
{
    [Header("----- Enemy's Components ------")]
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Transform targetPosition;  //Player is not yet implemented so adding transform position for testing 

    [Header("----- Enemy's Stats ------")]
    [SerializeField] int facingSpeed;

    Vector3 targetDirection;        

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Get direction of the target 
        targetDirection = targetPosition.position - transform.position;

        //When target is within the stopping distance, continue facing the target 
        if(navAgent.remainingDistance < navAgent.stoppingDistance)
        {
            faceTarget();
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
}
