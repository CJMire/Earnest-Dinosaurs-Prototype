using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class invincibility : MonoBehaviour
{
    [Header("---- Pickup Stats ----")]
    [SerializeField] int rotationSpeed;
    [SerializeField] int pickupGroundDuration;
    [SerializeField] int pickupDuration;

    public GameObject bullet1;
    public GameObject bullet2;
    public GameObject bullet3;

    public bool isInvincible;
    // Start is called before the first frame update
    void Start()
    {
        isInvincible = false;
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
            StartCoroutine(invincible());

        }
    }

    IEnumerator invincible()
    {
        isInvincible = true;
        bullet1.GetComponent<SphereCollider>().enabled = false;
        bullet2.GetComponent<SphereCollider>().enabled = false;
        bullet3.GetComponent<SphereCollider>().enabled = false;
        transform.position = new Vector3(0, -10, 0);
        yield return new WaitForSeconds(pickupDuration);
        bullet1.GetComponent<SphereCollider>().enabled = true;
        bullet2.GetComponent<SphereCollider>().enabled = true;
        bullet3.GetComponent<SphereCollider>().enabled = true;
        isInvincible = false;
        Destroy(gameObject);
    }
}
