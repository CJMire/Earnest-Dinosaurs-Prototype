using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class damagePickup : MonoBehaviour
{
    [Header("---- Pickup Stats ----")]
    [SerializeField] int rotationSpeed;
    [SerializeField] int pickupGroundDuration;
    [SerializeField] int pickupDuration;
    [SerializeField] int damageIncrease;

    public bool hasDamageIncrease;

    // Start is called before the first frame update
    void Start()
    {
        hasDamageIncrease = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0.0f, 20.0f, 0.0f) * Time.deltaTime * rotationSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }

        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(damageUp());
        }
    }
    
    IEnumerator damageUp()
    {
        hasDamageIncrease = true;
        gameManager.instance.playerScript.damageIncrease(damageIncrease);
        transform.position = new Vector3(0, -10, 0);
        yield return new WaitForSeconds(pickupDuration);
        gameManager.instance.playerScript.damageDecrease(damageIncrease);
        hasDamageIncrease = false;
        Destroy(gameObject);
    }
}
