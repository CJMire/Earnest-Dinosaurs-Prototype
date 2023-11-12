using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerSpawner : MonoBehaviour
{
    int isEnemyNear;

    void Start()
    {
        isEnemyNear = 0;
    }

    void OnTriggerEnter(Collider other)
    {
        //makes sure the collider is not another trigger and is the enemy damage collider
        if (!other.isTrigger && other.gameObject.CompareTag("Enemy"))
        {
            isEnemyNear++;
            Debug.Log(gameObject.name + " detects " + isEnemyNear + " enemies");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && other.gameObject.CompareTag("Enemy"))
        {
            isEnemyNear--;
            Debug.Log(gameObject.name + " detects " + isEnemyNear + " enemies");
        }
    }

    public bool IsEnemyNear()
    {
        //returns false when there is not an enemy within the spawner's colliders range
        //otherwise, returns true
        if(isEnemyNear > 0)
            return true;

        return false;
    }
}
