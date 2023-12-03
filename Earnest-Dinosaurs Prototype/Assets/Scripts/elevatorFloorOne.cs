using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class elevatorFloorOne : MonoBehaviour
{
    public gameManager instance;
    [SerializeField] public List<GameObject> elevatorDoors = new List<GameObject>();
    [SerializeField] public GameObject thePlayer;
    [SerializeField] public playerController playerController;
    [SerializeField] public Transform floor2Spawn;
    [SerializeField] public Transform floor3Spawn;
    [SerializeField] public GameObject playerSpawnPoint;
    [SerializeField] public GameObject spawnPoint1;
    [SerializeField] public GameObject spawnPoint2;
    [SerializeField] public GameObject spawnPoint3;
    [SerializeField] public GameObject spawnPoint4;
    [SerializeField] public Transform newSpawnPoint1;
    [SerializeField] public Transform newSpawnPoint2;
    [SerializeField] public Transform newSpawnPoint3;
    [SerializeField] public Transform newSpawnPoint4;
    [SerializeField] public Transform newPlayerSpawnPoint;
    [SerializeField] public Transform roofSpawnPoint1;
    [SerializeField] public Transform roofSpawnPoint2;
    [SerializeField] public Transform roofSpawnPoint3;
    [SerializeField] public Transform roofSpawnPoint4;
    [SerializeField] public Transform roofPlayerSpawnPoint;
    [SerializeField] AudioSource elevators;
    [SerializeField] public AudioClip elevatorDing;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //onFloorTwo();
        if(instance.waveCurrent == 2 && instance.enemyCount == 0)
        {
            elevatorDoors[0].SetActive(false);
            elevatorDoors[1].SetActive(false);
            playerSpawnPoint.SetActive(false);
            spawnPoint1.SetActive(false);
            spawnPoint2.SetActive(false);
            spawnPoint3.SetActive(false);
            spawnPoint4.SetActive(false);
            playerSpawnPoint.transform.position = newPlayerSpawnPoint.transform.position;
            spawnPoint1.transform.position = newSpawnPoint1.transform.position;
            spawnPoint2.transform.position = newSpawnPoint2.transform.position;
            spawnPoint3.transform.position = newSpawnPoint3.transform.position;
            spawnPoint4.transform.position = newSpawnPoint4.transform.position;
            playerSpawnPoint.SetActive(true);
            spawnPoint1.SetActive(true);
            spawnPoint2.SetActive(true);
            spawnPoint3.SetActive(true);
            spawnPoint4.SetActive(true);
        }
        if(instance.waveCurrent == 3 && instance.enemyCount == 0)
        {
            elevatorDoors[0].SetActive(false);
            elevatorDoors[1].SetActive(false);
            playerSpawnPoint.SetActive(false);
            spawnPoint1.SetActive(false);
            spawnPoint2.SetActive(false);
            spawnPoint3.SetActive(false);
            spawnPoint4.SetActive(false);
            playerSpawnPoint.transform.position = roofPlayerSpawnPoint.transform.position;
            spawnPoint1.transform.position = roofSpawnPoint1.transform.position;
            spawnPoint2.transform.position = roofSpawnPoint2.transform.position;
            spawnPoint3.transform.position = roofSpawnPoint3.transform.position;
            spawnPoint4.transform.position = roofSpawnPoint4.transform.position;
            playerSpawnPoint.SetActive(true);
            spawnPoint1.SetActive(true);
            spawnPoint2.SetActive(true);
            spawnPoint3.SetActive(true);
            spawnPoint4.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && playerController.transform.position.y <= 40)
        {
            thePlayer.SetActive(false);
            thePlayer.transform.position = floor2Spawn.transform.position;
            thePlayer.SetActive(true);
            elevatorDoors[0].SetActive(true);
            elevatorDoors[1].SetActive(true);

        }
        else if(other.CompareTag("Player") && playerController.transform.position.y <= 66)
        {
            thePlayer.SetActive(false);
            thePlayer.transform.position = floor3Spawn.transform.position;
            thePlayer.SetActive(true);
            elevatorDoors[0].SetActive(true);
            elevatorDoors[1].SetActive(true);
        }
        
    }

    //public void onFloorTwo()
    //{
        //if(onFloor2 == true)
        //{
            //GetComponent<Collider>().enabled = false;
        //}
    //}
}
