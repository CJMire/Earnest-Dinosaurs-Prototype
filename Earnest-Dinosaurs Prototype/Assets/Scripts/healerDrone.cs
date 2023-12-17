using System;
using System.Collections;
using System.Runtime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class healerDrone : MonoBehaviour, IDamage
{
    [Header("----- Healer's Components ------")]
    [SerializeField] Renderer[] enemyModelArray;
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Transform shootPosition;
    [SerializeField] Transform headPos;
    [SerializeField] Transform torsoPos;
    [SerializeField] Animator anim;
    [SerializeField] Collider damageCol;
    [SerializeField] AudioSource aud;
    [SerializeField] ParticleSystem damageParticle;
    [SerializeField] ParticleSystem lightingParticle;
    [SerializeField] ParticleSystem droneDeathParticle;

    [Header("----- Healer's Stats ------")]
    [SerializeField] int HP;
    [SerializeField] int facingSpeed;
    [SerializeField] float damageDuration;
    [SerializeField] float knockbackForce;

    [Header("----- Healer's Beam Stats ------")]
    [SerializeField] LineRenderer beamObject;
    [SerializeField] int healAmount;
    [SerializeField] float healRate;

    [Header("----- Barrier's Stats ------")]
    [SerializeField] GameObject barrierObject;
    [SerializeField] ParticleSystem barrierParticle;
    [SerializeField] Renderer barrierRenderer;
    [SerializeField] AudioClip barrierDamage;
    [SerializeField] AudioClip barrierDestroy;
    [SerializeField] int barrierHP;

    [Header("----- Loot ------")]
    [SerializeField] GameObject medkitObject;
    [Range(1, 100)][SerializeField] float medkitDropRate;

    [Header("----- Sound ------")]
    [SerializeField] AudioClip hurtSound;
    [SerializeField] AudioClip deadSound;
    [Range(0, 1)][SerializeField] float enemyVol;

    Color[] modelOrigColor;
    Color barrierOrigColor;

    GameObject target;
    enemyAI targetScript;

    Vector3 targetDirection;
    bool isHealing;
    bool isDead;
    bool targetInHealRange;
    Vector3 startingPos;


    void Start()
    {
        //Store different model parts information 
        modelOrigColor = new Color[enemyModelArray.Length];

        for (int i = 0; i < enemyModelArray.Length; i++)
        {
            modelOrigColor[i] = enemyModelArray[i].material.color;
        }

        barrierOrigColor = barrierRenderer.material.color;

        startingPos = transform.position;
        isHealing = false;
        isDead = false;
        gameManager.instance.updateEnemyCount(1);
    }

    // Update is called once per frame
    void Update()
    {
        //If target is dead, reset
        if (target != null && targetScript.GetIsDead())
        {
            target = null;
            targetScript = null;
            targetInHealRange = false;
        }

        //If no target had, find one
        if (target == null)
        {
            findTarget();
            //If couldn't find target after all enemies spawn, destroy self.
            if (gameManager.instance.GetStopSpawning() && target == null && !isDead)
            {
                Debug.Log("Destructed");
                SelfDestruct();
            }
        }

        //If healing, update beam
        if (isHealing)
        {
            updateBeam();
        }

        //If agent is not on then don't do anything. Otherwise, go to target
        if (navAgent.isActiveAndEnabled && !isDead && target != null)
        {
            //Set the model animation speed along with its navAgent normalized velocity 
            anim.SetFloat("Speed", navAgent.velocity.normalized.magnitude);

            //Target inside the sphere but not see the target 
            if (!canSeeTarget())
            {
                seek();
            }
        }
    }

    //Finds the closest gameObject of tag "Enemy" and sets it to target
    void findTarget()
    {
        float closest = 50;
        GameObject closestObj = null;

        //Finds all colliders in a radius and puts them in an array
        Collider[] hits = Physics.OverlapSphere(transform.position, 100);

        //Iteration to find closest collider of tag "Enemy"
            foreach (var enemy in hits)
            {
                if (enemy.gameObject.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, enemy.gameObject.transform.position);
                    if (distance < closest)
                    {
                        closest = distance;
                        closestObj = enemy.gameObject;
                    }
                }
            }

            target = closestObj;
            if (target != null)
                targetScript = target.GetComponent<enemyAI>();
    }

    bool canSeeTarget()
    {
        //Get target direction
        targetDirection = target.transform.position - headPos.position;

        //Raycast checking
        RaycastHit hit;

        if (Physics.Raycast(headPos.position, targetDirection, out hit))
        {
            //If the target can be seen
            if (hit.collider.gameObject.GetInstanceID() == target.GetInstanceID())
            {
                //Heal target when in range
                if (!isHealing && targetInHealRange)
                {
                    StartCoroutine(healTarget());
                }

                //Do this when player is in stopping distance 
                if (navAgent.remainingDistance < navAgent.stoppingDistance)
                {
                    faceTarget();
                }

                return true;
            }
        }

        navAgent.stoppingDistance = 0.0f;

        return false;
    }

    void seek()
    {
        //Go towards the target
        faceTarget();
        navAgent.SetDestination(target.gameObject.transform.position);
    }

    void faceTarget()
    {
        //Get rotation to the target 
        Quaternion faceRotation = Quaternion.LookRotation(targetDirection);

        //Rotate to the target using lerp with set up speed
        transform.rotation = Quaternion.Lerp(transform.rotation, faceRotation, Time.deltaTime * facingSpeed);
    }

    IEnumerator healTarget()
    {
        isHealing = true;
        beamObject.enabled = true;

        enemyAI targetScript = target.GetComponent<enemyAI>();
        if (targetScript.GetIsDead()) updateBeam();

        while (targetInHealRange && !isDead && !targetScript.GetIsDead())
        {
            targetScript.heal(healAmount);
            yield return new WaitForSeconds(healRate);
        }

        isHealing = false;
        beamObject.enabled = false;

        /*
        isHealing = true;
        beamObject.enabled = true;
        updateBeam();

        var targetScript = target.GetComponent<enemyAI>();
        while (targetInHealRange && !isDead && !targetScript.GetIsDead())
        {
            targetScript.heal(healAmount);
            yield return new WaitForSeconds(healRate);
        }

        isHealing = false;
        beamObject.enabled = false;
        */
    }

    public void updateBeam()
    {
        if(target != null)
        {
            beamObject.SetPosition(0, shootPosition.transform.position);
            beamObject.SetPosition(1, target.gameObject.transform.position);
        }
      
    }

    public void takeDamage(int damageAmount)
    {
        if (barrierHP <= 0)
        {
            HP -= damageAmount;

            //Model damage red flash 
            StartCoroutine(damageFeedback());

            //HP is zero then destroy the enemy 
            if (HP <= 0)
            {
                //Spawn medkit within drop rate, set isDead and destroy gameObject 
                aud.PlayOneShot(deadSound, enemyVol);

                Instantiate(lightingParticle, torsoPos.position, lightingParticle.transform.rotation);

                DropSomething(); //Drops one puick-up for player use

                isDead = true;
                navAgent.enabled = false;
                anim.SetBool("Dead", true);

                //turns off enemy damage colliders when dead
                damageCol.enabled = false;

                StartCoroutine(OnDeath());

                //Destroy(gameObject);
                gameManager.instance.updateEnemyCount(-1);
            }

            else
            {

                //Play damage animation
                anim.SetTrigger("Damage");

                aud.PlayOneShot(hurtSound, enemyVol);

                knockback();
            }
        }

        else
        {
            barrierHP--;
            aud.PlayOneShot(barrierDamage, enemyVol);
            StartCoroutine(barrierFeedback());

            if (barrierHP <= 0)
            {
                aud.PlayOneShot(barrierDestroy, enemyVol);
                barrierObject.SetActive(false);
            }
        }
    }

    IEnumerator damageFeedback()
    {
        for (int i = 0; i < enemyModelArray.Length; i++)
        {
            enemyModelArray[i].material.color = Color.red;
        }

        Instantiate(damageParticle, torsoPos.position, transform.rotation);

        yield return new WaitForSeconds(damageDuration);

        for (int i = 0; i < enemyModelArray.Length; i++)
        {
            enemyModelArray[i].material.color = modelOrigColor[i];
        }
    }

    IEnumerator barrierFeedback()
    {
        float alpha = barrierOrigColor.a;

        barrierRenderer.material.color = new Color(1.0f, 0.0f, 0.0f, alpha);

        if (barrierParticle != null)
        {
            Instantiate(barrierParticle, transform.position, barrierParticle.transform.rotation);
        }

        yield return new WaitForSeconds(damageDuration);

        barrierRenderer.material.color = barrierOrigColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (target != null && other.gameObject.GetInstanceID() == target.GetInstanceID())
        {
            targetInHealRange = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (target != null && other.gameObject.GetInstanceID() == target.GetInstanceID())
        {
            targetInHealRange = false;
        }
    }

    void knockback()
    {
        //Set velocity to opposite of facing target direction and multiply by force 
        navAgent.velocity = (targetDirection * -1.0f).normalized * knockbackForce;

        //Set angular speed to zero (Enemy face not turning when knocked back)
        navAgent.angularSpeed = 0;
    }

    //Made from Chayathorn's Medkitdrop() method & Cameron's SpeedPickupDrop() and InvincibilityPickupDrop() methods
    //invincibility should be the hardest to get & damage should be 2nd hardest
    //speed should be the 3rd hardest & the medkit should be the easiest
    //However, enemies should only drop 1 thing at time
    //I have set the drop-rates with changable ranges for testing
    void DropSomething()
    {
        float drop = UnityEngine.Random.Range(1, 100);

        if (drop <= medkitDropRate)
        {
            Instantiate(medkitObject, transform.position, transform.rotation);
        }
    }

    //Destroys gameObject after set amount of time
    IEnumerator OnDeath()
    {
        yield return new WaitForSeconds(0.5f);
        Instantiate(droneDeathParticle, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void SelfDestruct()
    {
        //Spawn medkit within drop rate, set isDead and destroy gameObject 
        aud.PlayOneShot(deadSound, enemyVol);

        Instantiate(lightingParticle, torsoPos.position, lightingParticle.transform.rotation);

        DropSomething(); //Drops one puick-up for player use

        isDead = true;
        navAgent.enabled = false;
        anim.SetBool("Dead", true);

        //turns off enemy damage colliders when dead
        damageCol.enabled = false;

        StartCoroutine(OnDeath());

        //Destroy(gameObject);
        gameManager.instance.updateEnemyCount(-1);
    }
}
