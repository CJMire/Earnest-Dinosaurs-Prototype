using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickups : MonoBehaviour
{
    [Header("---- Speed PowerUp ----")]
    [SerializeField] float speedIncrease;
    bool hasSpeed;

    [Header("---- Instant Kill ----")]
    [SerializeField] int damageIncrease;
    bool hasInstantKill;

    [Header("---- Invincibility ----")]
    bool isInvincible;

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
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0.0f, 20.0f, 0.0f) * Time.deltaTime * rotationSpeed);

        Destroy(gameObject, pickupDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            speedUp();
            Destroy(gameObject);
        }
    }

    IEnumerator speedUp()
    {
        hasSpeed = true;
        newSpeed = currentSpeed * speedIncrease;
        yield return new WaitForSeconds(pickupDuration);
        newSpeed = currentSpeed / speedIncrease;
        hasSpeed = false;
    }
}
