using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
    [Header("----- Components -----")]
    [SerializeField] CharacterController characterController;

    [Header("----- Player Stats -----")]
    [SerializeField] int HP;
    private int maxHP;
    [SerializeField] int ammoCount;
    [SerializeField] float playerSpeed;
    [SerializeField] float playerJumpHeight;
    [SerializeField] int playerJumpMax;
    [SerializeField] float gravityStrength;

    [Header("----- Player Gun Stats -----")]
    [SerializeField] int shootDamage;
    [SerializeField] float shootDistance;
    [SerializeField] float shootRate;
    private int maxAmmo;
    [SerializeField] float reloadTime;
    [SerializeField] GameObject bulletObject;

    private Vector3 move;
    private Vector3 playerVelocity;
    private bool playerIsGrounded;
    private int jumpTimes;
    private bool isShooting;
    private bool isReloading;

    // Start is called before the first frame update
    void Start()
    {
        //Sets maxAmmo
        maxAmmo = ammoCount;
        //sets maxHP
        maxHP = HP;
        //updates HUD to match current HP && Ammo
        gameManager.instance.updateHUD();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDistance, Color.red);

        //Checks if the player can shoot
        if (Input.GetButton("Shoot") && !isShooting && !isReloading && ammoCount > 0 && !(gameManager.instance.isPaused))
        {
            StartCoroutine(Shoot());
        }

        playerIsGrounded = characterController.isGrounded;
        if (playerIsGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
            jumpTimes = 0;
        }

        move = Input.GetAxis("Horizontal") * transform.right +
               Input.GetAxis("Vertical") * transform.forward;
        characterController.Move(move * Time.deltaTime * playerSpeed);

        if (Input.GetButtonDown("Jump") && jumpTimes < playerJumpMax)
        {
            playerVelocity.y = playerJumpHeight;
            jumpTimes++;
        }
        playerVelocity.y += gravityStrength * Time.deltaTime;
        characterController.Move(playerVelocity * Time.deltaTime);
    }

    IEnumerator Shoot()
    {
        isShooting = true;
        ammoCount--;
        gameManager.instance.updateHUD();

        RaycastHit hit;
        //for use when RaycastHit does hit an enemy
        float offset = 0;
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDistance))
        {
            IDamage damageable = hit.collider.GetComponent<IDamage>();

            if (hit.transform != transform && damageable != null)
            {
                damageable.takeDamage(shootDamage);

                //since the hitmarker will be shown for x amount of time, we must offset the next time the player can shoot
                offset = gameManager.instance.getHitMarkerRate();

                //On hit, shows the hitmarker for the hitMarkerRate duration in gameManager
                gameManager.instance.GetHitMarker().gameObject.SetActive(true);
                yield return new WaitForSeconds(offset);
                gameManager.instance.GetHitMarker().gameObject.SetActive(false);
            }

        }
        //If the raycastHit doesn't hit, there's no subtraction and shootrate remains constant
        //if it does, there's a slightly longer wait to shoot again. this ensures there's no loss in time
        yield return new WaitForSeconds(shootRate - offset);
        isShooting = false;
    }

    public void takeDamage(int damageAmount)
    {
        //Updates HP value and HUD
        HP -= damageAmount;
        StartCoroutine(gameManager.instance.playerHurtFlash());
        //makes sure no HP is negative & calls lose screen
        if (HP <= 0)
        {
            HP = 0;
            gameManager.instance.OnDeath();
        }

        gameManager.instance.updateHUD();
    }

    public void healPlayer(int amount)
    {
        //Heal the player 
        HP += amount;
        if (HP > maxHP)
            HP = maxHP;
        gameManager.instance.updateHUD();
    }

    public void ReloadSuccess()
    {
        ammoCount = maxAmmo;
        gameManager.instance.updateHUD();
    }

    #region Getters & Setters
    public int getPlayerMaxHP()
    {
        return maxHP;
    }

    public int getPlayerCurrentHP()
    {
        return HP;
    }

    public int getPlayerMaxAmmo()
    {
        return maxAmmo;
    }

    public int getPlayerCurrentAmmo()
    {
        return ammoCount;
    }

    public bool getIsShooting()
    {
        return isShooting;
    }

    public void SetIsShooting(bool isShooting)
    {
        this.isShooting = isShooting;
    }

    public bool GetIsReloading()
    {
        return isShooting;
    }

    public void SetIsReloading(bool isReloading)
    {
        this.isReloading = isReloading;
    }

    public float GetReloadTime()
    {
        return reloadTime;
    }
    #endregion
}