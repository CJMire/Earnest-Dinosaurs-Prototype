using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerSpawner : MonoBehaviour
{
    int isEnemyNear;
    List<GameObject> enemies;

    void Start()
    {
        isEnemyNear = 0;
        enemies = new List<GameObject>();
    }

    private void Update()
    {
        if(enemies.Count > 0)
        {
            for(int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].GetComponent<enemyAI>().GetIsDead())
                {
                    isEnemyNear--;
                    enemies.RemoveAt(i);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //makes sure the collider is not another trigger and is the enemy damage collider
        if (!other.isTrigger && (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss")))
        {
            isEnemyNear++;
            if (other.gameObject.CompareTag("Enemy")) { enemies.Add(other.gameObject); }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.isTrigger && (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss")))
        {
            isEnemyNear--;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == other.gameObject)
                {
                    enemies.RemoveAt(i);
                }
            }
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
