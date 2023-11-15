using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speedPickup : MonoBehaviour
{
    [Header("---- Speed PowerUp ----")]
    [SerializeField] float speedIncrease;
    [SerializeField] bool hasSpeed;

    [Header("---- Pickup Stats ----")]
    [SerializeField] int rotationSpeed;
    [SerializeField] int pickupGroundDuration;
    [SerializeField] int pickupDuration;

    public float currentSpeed;
    public float newSpeed;

    // Start is called before the first frame update
    void Start()
    {
        currentSpeed = gameManager.instance.playerScript.getPlayerCurrentSpeed();
        hasSpeed = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0.0f, 20.0f, 0.0f) * Time.deltaTime * rotationSpeed);

        Destroy(gameObject, pickupGroundDuration);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            if(hasSpeed == false)
            {
                StartCoroutine(speedUp());
            }

        }
    }

    IEnumerator speedUp()
    {
        hasSpeed = true;
        gameManager.instance.playerScript.speedUpPlayer(speedIncrease);
        transform.position = new Vector3(0, -10, 0);
        yield return new WaitForSeconds(pickupDuration);
        gameManager.instance.playerScript.speedDownPlayer(speedIncrease);
        hasSpeed = false;
        Destroy(gameObject);
    }
}
