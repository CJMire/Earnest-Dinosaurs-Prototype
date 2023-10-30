using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class medkit : MonoBehaviour
{
    [Header("----- Medkit's Stats ------")]
    [SerializeField] int healingAmount;
    [SerializeField] int medkitDuration;
    [SerializeField] int rotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //Make the medkit rotate around 
        transform.Rotate(new Vector3(20.0f, 20.0f, 20.0f) * Time.deltaTime * rotationSpeed);

        //Destroy the medkit within this remaining time 
        Destroy(gameObject, medkitDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        //Ignore all trigger 
        if (other.isTrigger)
        {
            return;
        }

        //If player collide then heal the player and destroy game object 
        if(other.gameObject.CompareTag("Player"))
        {
            //Heal the player 

            //Destroy the medkit 
            Destroy(gameObject);
        }
    }
}
