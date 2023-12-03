using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AI;

public class SergeantKuller : MonoBehaviour , IDamage
{
    [Header("----- Body Components -----")]
    [SerializeField] Collider weakSpot;
    [SerializeField] Collider[] bodyColliders;
    [SerializeField] Animator animator;
    [SerializeField] Transform headPos;

    [Header("----- Boss Settings -----")]
    [Range(1, 250)][SerializeField] float HP;
    [SerializeField] float turnRate;
    [Range(2.5f, 5)][SerializeField] float attackPauseTime;
    [Range(1, 10)][SerializeField] float attackCooldownTime;
    [Range(1, 10)][SerializeField] float attackAfterPauseTime;
    [SerializeField] float shootCone;

    [Header("----- Attack Components -----")]
    [SerializeField] GameObject shockwave;
    [SerializeField] GameObject seekerMissles;
    [SerializeField] GameObject lasers;

    [Header("----- Attack Instantiation Points -----")]
    [SerializeField] Transform[] lRMRackLaunchPoints_L;
    [SerializeField] Transform[] lRMRackLaunchPoints_R;
    [SerializeField] Transform laserShootPoint_L;
    [SerializeField] Transform laserShootPoint_R;
    [SerializeField] Transform footImpactPoint;

    private bool bootUpDone;
    private bool isAttacking;
    private bool attackCooldown;
    private bool cooldownStarted;
    private bool dontTurn;
    private bool doubleDMG;
    private Vector3 playerDirection;
    private float angleToPlayer;
    private bool canShoot;
    private bool landed;
    private bool collidersON;
    private bool isShooting;

    void Start()
    {
        isShooting = false;
        weakSpot.enabled = false;
        bodyColliders = GetComponentsInChildren<Collider>(true);
        TurnOffColiders();
        landed = false;
        bootUpDone = false;
        isAttacking = false;
        doubleDMG = false;
        attackCooldown = true;
        dontTurn = false;
        playerDirection = gameManager.instance.player.transform.position - headPos.transform.position;
    }

    void Update()
    {
        if(!gameManager.instance.GetIsPaused())
        {
            if(bootUpDone)
            {
                if (!collidersON)
                {
                    weakSpot.enabled = true;
                    TurnOnColiders();
                }

                CanSeePlayer();
                if((!isAttacking && attackCooldown && !dontTurn) || isShooting)
                    FaceTarget();

                if (!isAttacking && attackCooldown && !cooldownStarted)
                    StartCoroutine(Cooldown());

                if(!isAttacking && !attackCooldown)
                {
                    Attack();
                }
            }
            else if(!bootUpDone && !landed)
            {
                Incoming();
                if(transform.position.y < 0.1f)
                {
                    landed = true;
                }
            }
            else if(!bootUpDone && landed)
            {
                animator.SetTrigger("BootUp");
            }
        }
    }

    #region Movement

    void FaceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(playerDirection.x, 0, playerDirection.z));

        if(!canShoot)
            animator.SetFloat("Turnrate", turnRate);

        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * turnRate);
    }

    void CanSeePlayer()
    {
        playerDirection = gameManager.instance.player.transform.position - headPos.transform.position;
        angleToPlayer = Vector3.Angle(playerDirection, headPos.transform.forward);

        UnityEngine.Debug.DrawRay(headPos.transform.position, playerDirection);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDirection, out hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (angleToPlayer <= shootCone)
                {
                    canShoot = true;
                    if(!isAttacking)
                        animator.SetFloat("Turnrate", .5f);
                }
            }
            else { canShoot = false; }
        }
    }

    public void BootUpDone()
    {
        bootUpDone = true;
    }

    public void Incoming()
    {
        transform.position = Vector3.Lerp(transform.position,Vector3.zero, Time.deltaTime);
    }
    #endregion
    #region Damage

    public void takeDamage(int damageAmount)
    {
        //will double damage if it was the weakspot hit
        if(doubleDMG) 
        {
            damageAmount *= 2;

            UnityEngine.Debug.Log("Double Damage");
        }
        else
        { //half damage if it is not
            damageAmount /= 2;

            UnityEngine.Debug.Log("Damage Halved");
        }

        HP -= damageAmount;

        Debug.Log("Boss HP: " + HP.ToString());

        //after use, will turn double damage off if on
        if(doubleDMG) { doubleDMG = false; }

        if(HP <= 0)
        {

        }
    }

    void TurnOffColiders()
    {
        for (int ndx = 0; ndx < bodyColliders.Length; ndx++)
        {
            if (bodyColliders[ndx].enabled)
            {
                bodyColliders[ndx].enabled = false;
            }
        }
        collidersON = false;
    }

    void TurnOnColiders()
    {
        for (int ndx = 0; ndx < bodyColliders.Length; ndx++)
        {
            if (!bodyColliders[ndx].enabled)
            {
                bodyColliders[ndx].enabled = true;
            }
        }
        collidersON = true;
    }

    public Collider GetWeakspot()
    {
        return weakSpot;
    }

    public void SetDoubleDamage(bool dDMG)
    {
        doubleDMG = dDMG;
    }
    #endregion
    #region Attacks

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger && other.CompareTag("Player"))
        {
            animator.SetTrigger("Shockwave Attack");
        }
    }

    public void Shockwave()
    {
        Instantiate(shockwave, footImpactPoint.transform.position, shockwave.transform.rotation);
    }

    IEnumerator Shoot()
    {
        float ogTR = turnRate;
        turnRate = .2f;
        Instantiate(lasers, laserShootPoint_R.transform.position, lasers.transform.rotation);
        Instantiate(lasers, laserShootPoint_L.transform.position, lasers.transform.rotation);
        yield return new WaitForSeconds(0.2f);
        if (isAttacking && canShoot)
        {
            StartCoroutine(Shoot());
        }
        else
        {
            turnRate = ogTR;
        }
    }

    IEnumerator IsAttacking()
    {
        isAttacking = true;
        dontTurn = true;
        yield return new WaitForSeconds(attackPauseTime);
        isAttacking = false;
        yield return new WaitForSeconds(attackAfterPauseTime);
        dontTurn = false;
        attackCooldown = true;
    }

    void Attack()
    {
        animator.SetFloat("Turnrate", 0f);
        StartCoroutine(IsAttacking());

        int attacks = 1; // will always be able to use the rockets so this is one
        if (canShoot)
        {
            attacks++; //if the player is within, at the time of this call, it will add one
        }
        if(gameManager.instance.player.transform.position.y <= 1.50f)
        {
            attacks++; //if the player is above this certain height, we won't use the shockwave bc it won't hit them
        }

        if(attacks == 1)
        {
            UnityEngine.Debug.Log("ROCKETS");
            StartCoroutine(Rockets());
            return; //if rockets is the only thing that can be used, it will
        }
        
        attacks = Random.Range(0, attacks); //random choice of attack, if we can use the shoot but not shockwave, it won't be reached at the bottom.

        if(!canShoot && attacks == 1) //makes sure we don't use the shoot if it can't see player
        {
            attacks = Random.Range(0, attacks); //rerolls
            if (attacks == 0)
            {
                UnityEngine.Debug.Log("ROCKETS");
                StartCoroutine(Rockets());
                return;
            }
            else if (attacks == 1)
            {
                Debug.Log("Shockwave Attack");
                animator.SetTrigger("Shockwave Attack");
                return;
            }

        }

        if (attacks == 0)
        {
            UnityEngine.Debug.Log("ROCKETS");
            StartCoroutine(Rockets());
        }
        else if (attacks == 1)
        {
            Debug.Log("Shooting Lasers");
            StartCoroutine (Shoot());
        }
        else if(attacks == 2) //if we can't use shockwave, we will never reach it
        {
            Debug.Log("Shockwave Attack");
            animator.SetTrigger("Shockwave Attack");
        }
    }

    IEnumerator Cooldown()
    {
        cooldownStarted = true;
        yield return new WaitForSeconds(attackCooldownTime);
        attackCooldown = false;
        cooldownStarted = false;
    }

    IEnumerator Rockets()
    {
        for (int i = 0;i < lRMRackLaunchPoints_L.Length;i++)
        {
            Instantiate(seekerMissles, lRMRackLaunchPoints_L[i].position, lRMRackLaunchPoints_L[i].rotation);
            yield return new WaitForSeconds(0.5f);
            Instantiate(seekerMissles, lRMRackLaunchPoints_R[i].position, lRMRackLaunchPoints_R[i].rotation);
            yield return new WaitForSeconds(0.5f);
        }
    }
    #endregion
}
