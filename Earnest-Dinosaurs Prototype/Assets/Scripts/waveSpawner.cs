using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class waveSpawner : MonoBehaviour
{
    public bool stopSpawning = false;
    public static waveSpawner instance;

    [Header("----- Spawner Enemies -----")]
    [SerializeField] private GameObject EnemyBase_1;
    [SerializeField] private GameObject EnemyBase_2;
    [SerializeField] private GameObject EnemyBase_3;

    [Header("----- Wave Settings -----")]
    [SerializeField] int waveCount;
    [SerializeField] float spawnSpeed;
    [SerializeField] float gracePeriod;
    public float timetillSpawn;
    public int currentWave = 0;

    [Header("----- Spawner Points -----")]
    [SerializeField] public Transform spawnLocation1;
    [SerializeField] public Transform spawnLocation2;
    [SerializeField] public Transform spawnLocation3;
    [SerializeField] public Transform spawnLocation4;

    [Header("----- Enemy Settings -----")]
    [SerializeField] public int totalEnemies;
    public int enemyCount;

    public int enemiesAlive;

    public void SpawnWave()
    {
        int random = Random.Range(0, 4);
        if(random == 1)
        {
            Instantiate(EnemyBase_1, spawnLocation1);
        }
        else if(random == 2)
        {
            Instantiate(EnemyBase_2, spawnLocation2);
        }
        else if(random == 3)
        {
            Instantiate(EnemyBase_1, spawnLocation3);
        }
        else
        {
            Instantiate(EnemyBase_2, spawnLocation4);
        }
        enemyCount++;
    }
}
