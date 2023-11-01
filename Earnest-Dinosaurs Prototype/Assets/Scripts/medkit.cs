using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class medkit : MonoBehaviour
{
    [Header("----- Medkit's Stats ------")]
    [SerializeField] int healingAmount;
    [SerializeField] int medkitDuration;
    [SerializeField] int rotationSpeed;

    int maxHP;
    int currentHP;

    // Start is called before the first frame update
    void Start()
    {
        maxHP = gameManager.instance.playerScript.getPlayerMaxHP();
        currentHP = gameManager.instance.playerScript.getPlayerCurrentHP();
    }

    // Update is called once per frame
    void Update()
    {
        //Make the medkit rotate around 
        transform.Rotate(new Vector3(0.0f, 20.0f, 0.0f) * Time.deltaTime * rotationSpeed);

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
            //If player HP is more than maximum then don't pick
            if (currentHP < maxHP)
            {
                //Heal player 
                gameManager.instance.playerScript.healPlayer(healingAmount);

                //Destroy medkit
                Destroy(gameObject);
            }        
        }
    }
}
