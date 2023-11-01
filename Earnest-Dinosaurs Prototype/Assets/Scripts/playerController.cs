using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class playerController : MonoBehaviour, IDamage
{
    [Header("----- Components -----")]
    [SerializeField] CharacterController characterController;

    [Header("----- Player Stats -----")]
    [SerializeField] int HP;
    [SerializeField] int maxHP;
    [SerializeField] float playerSpeed;
    [SerializeField] float playerJumpHeight;
    [SerializeField] int playerJumpMax;
    [SerializeField] float gravityStrength;

    [Header("----- Player Gun Stats -----")]
    [SerializeField] int shootDamage;
    [SerializeField] float shootDistance;
    [SerializeField] float shootRate;
    [SerializeField] GameObject bulletObject;

    [Header("----- HUD Updates -----")]
    [SerializeField] Image imageHPBar;
    [SerializeField] TextMeshProUGUI textHealth;
    [SerializeField] TextMeshProUGUI textCurrentAmmo;
    [SerializeField] GameObject hitMarker;
    [SerializeField] float hitMarkerRate;

    private Vector3 move;
    private Vector3 playerVelocity;
    private bool playerIsGrounded;
    private int jumpTimes;
    private bool isShooting;

    // Start is called before the first frame update
    void Start()
    {
        //updates HUD to match current HP
        updateHUD();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDistance, Color.red);

        if(Input.GetButton("Shoot") && isShooting == false)
        {
            StartCoroutine(Shoot());
        }

        playerIsGrounded = characterController.isGrounded;
        if(playerIsGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
            jumpTimes = 0;
        }

        move = Input.GetAxis("Horizontal") * transform.right +
               Input.GetAxis("Vertical") * transform.forward;
        characterController.Move(move * Time.deltaTime * playerSpeed);

        if(Input.GetButtonDown("Jump") && jumpTimes < playerJumpMax)
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

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f)), out hit, shootDistance))
        {
            IDamage damageable = hit.collider.GetComponent<IDamage>();

            if (hit.transform != transform && damageable != null)
            {
                damageable.takeDamage(shootDamage);
                hitMarker.gameObject.SetActive(true);
                yield return new WaitForSeconds(hitMarkerRate);
                hitMarker.gameObject.SetActive(false);
            }

        }
        yield return new WaitForSeconds(shootRate);
        isShooting = false;
    }

    public void takeDamage(int damageAmount)
    {
        //Updates HP value and HUD
        HP -= damageAmount;
        updateHUD();
        StartCoroutine(gameManager.instance.playerHurtFlash());
    }

    public void healPlayer(int amount)
    {
        //Heal the player 
        HP += amount;
        updateHUD();
    }

    public int getPlayerMaxHP()
    {
        return maxHP;
    }

    public int getPlayerCurrentHP()
    {
        return HP;
    }

    public void updateHUD()
    {
        imageHPBar.fillAmount = (float)HP / maxHP;
        textHealth.text = HP + " / " + maxHP;
    }
}
