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
    [SerializeField] GameObject[] blowupObjects;
    [SerializeField] ParticleSystem smallExplosion;
    [SerializeField] ParticleSystem bigExplosion;
    [SerializeField] Renderer sergeantKullerTank;

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

    [Header("----- Audio Components -----")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip laser;
    [Range(0f, 1f)][SerializeField] float laserVolume;
    [SerializeField] AudioClip explosion;
    [Range(0f, 1f)][SerializeField] float explosionVol;
    [SerializeField] AudioClip lightDamageSound;
    [Range(0f, 1f)][SerializeField] float lightDamageVol;
    [SerializeField] AudioClip[] tankDamageSound;
    [Range(0f, 1f)][SerializeField] float tankDamageVol;

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
    private bool alive;
    private Color colorOrig;

    //when spawning the boss, spawn at whatever y-pos you'd like, but preferably at 200 y-pos with x and z being 0
    //that way the spawn can uitilze the bootup code implemented

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
        gameManager.instance.ShowBossTokens(1);
        colorOrig = sergeantKullerTank.material.color;
    }

    void Update()
    {
        gameManager.instance.SetBossHealth((int)HP, 250);

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
            StartCoroutine(tankDamageFeedback());

            int randomSound = Random.Range(0, tankDamageSound.Length);
            aud.PlayOneShot(tankDamageSound[randomSound], tankDamageVol);
        }
        else
        { //half damage if it is not
            damageAmount /= 2;
            aud.PlayOneShot(lightDamageSound, lightDamageVol);
        }

        HP -= damageAmount;

        //after use, will turn double damage off if on
        if(doubleDMG) { doubleDMG = false; }

        if(HP <= 0)
        {
            OnDeath();
        }
    }

    IEnumerator tankDamageFeedback()
    {
        sergeantKullerTank.material.color = Color.red;

        Instantiate(smallExplosion, weakSpot.transform.position, smallExplosion. transform.rotation);

        yield return new WaitForSeconds(0.5f);

        sergeantKullerTank.material.color = colorOrig;
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
        if(alive)
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

    void OnDeath()
    {
        alive = false;
        gameManager.instance.ShowBossTokens(2);
        StopAllCoroutines();
        TurnOffColiders();
        PlayerPrefs.SetInt("SergeantKullerKills", PlayerPrefs.GetInt("SergeantKullerKills", 0) + 1);
        animator.SetFloat("Turnrate", 0f);
        animator.SetTrigger("Death");
        aud.PlayOneShot(explosion, explosionVol);
        for(int ndx = 0;ndx < blowupObjects.Length; ndx++)
        {
            Instantiate(smallExplosion, blowupObjects[ndx].transform.position, blowupObjects[ndx].transform.rotation);
            Destroy(blowupObjects[ndx]);
        }
        StartCoroutine(BlowUp());
    }

    IEnumerator BlowUp()
    {
        while (true)
        {
            int ndx = Random.Range(0, bodyColliders.Length);
            aud.PlayOneShot(explosion, explosionVol);
            while (bodyColliders[ndx] == null)
                ndx = Random.Range(0, bodyColliders.Length);
            Instantiate(smallExplosion, bodyColliders[ndx].transform.position, bodyColliders[ndx].transform.rotation);
            yield return new WaitForSeconds(Random.Range(0.1f,0.5f));
        }
    }

    public void BlowUpWhole()
    {
        StopAllCoroutines();
        aud.PlayOneShot(explosion, explosionVol);
        //Has to be like this, otherwise will spawn at feet
        Instantiate(bigExplosion, new Vector3(transform.position.x, transform.position.y, transform.position.z + 15f), bigExplosion.transform.rotation);
        Destroy(gameObject);
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
        turnRate = .3f;
        isShooting = true;

        aud.PlayOneShot(laser, laserVolume);

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
            isShooting = false;
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
            StartCoroutine(Rockets());
            return; //if rockets is the only thing that can be used, it will
        }
        
        attacks = Random.Range(0, attacks); //random choice of attack, if we can use the shoot but not shockwave, it won't be reached at the bottom.

        if(!canShoot && attacks == 1) //makes sure we don't use the shoot if it can't see player
        {
            attacks = Random.Range(0, attacks); //rerolls
            if (attacks == 0)
            {
                StartCoroutine(Rockets());
                return;
            }
            else if (attacks == 1)
            {
                animator.SetTrigger("Shockwave Attack");
                return;
            }

        }

        if (attacks == 0)
        {
            StartCoroutine(Rockets());
        }
        else if (attacks == 1)
        {
            StartCoroutine (Shoot());
        }
        else if(attacks == 2) //if we can't use shockwave, we will never reach it
        {
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
