using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UpgradeShop : MonoBehaviour
{
    [Header("----- Audio Components -----")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip purchaseSuccess;
    [SerializeField] AudioClip purchaseFailed;

    [Header("----- Max HP Upgrade Components-----")]
    [SerializeField] Button buttonMaxHpUpgrade;
    [SerializeField] TextMeshProUGUI textCurrentMaxHP;
    [SerializeField] TextMeshProUGUI textMaxHPCost;
    [SerializeField] TextMeshProUGUI textMaxHPPurchases;
    [SerializeField] TextMeshProUGUI textMaxHPUpgradeMaximum;
    [SerializeField] GameObject[] buttonMaxHPTexts; //this will exclude the maximum text BC it will still be used
    private int maxHPCost;
    private int maxHPPurchases;

    [Header("----- Speed Upgrade Components-----")]
    [SerializeField] Button buttonSpeedUpgrade;
    [SerializeField] TextMeshProUGUI textCurrentSpeed;
    [SerializeField] TextMeshProUGUI textSpeedCost;
    [SerializeField] TextMeshProUGUI textSpeedPurchases;
    [SerializeField] TextMeshProUGUI textSpeedUpgradeMaximum;
    [SerializeField] TextMeshProUGUI textSpeedUpgradeAmount;
    [SerializeField] GameObject[] buttonSpeedTexts; //this will exclude the maximum text BC it will still be used
    private int speedCost;
    private int speedPurchases;

    [Header("----- Last Upgrade Upgrade Components-----")]
    [SerializeField] Button buttonLastBulletUpgrade;
    [SerializeField] GameObject[] textLastBulletInfo;
    [SerializeField] TextMeshProUGUI textLastBulletPurchases;
    [SerializeField] TextMeshProUGUI textLastBulletUpgradeMaximum;
    [SerializeField] GameObject[] buttonLastBulletTexts; //this will exclude the maximum text BC it will still be used

    [Header("----- Covers -----")]
    [SerializeField] GameObject lastBulletCover;
    //Keys:
    // - "playerMaxHP" - int
    // - "MaxHPCost" - int
    // - "MaxHPPurchases" - int
    // ------------------------
    // - "playerSpeed" - float
    // - "SpeedCost" - int
    // - "SpeedPurchases" - int
    // ------------------------
    // - "SergeantKullerKills" - int
    // - "LastBulletPurchase" - int

    void Start()
    {
        //Sets text and current cost of Max Hp Increase Upgrade
        textCurrentMaxHP.text = PlayerPrefs.GetInt("playerMaxHP", 15).ToString();
        maxHPPurchases = PlayerPrefs.GetInt("MaxHPPurchases", 0);
        textMaxHPPurchases.text = maxHPPurchases.ToString() + " / 5";
        if(!CheckIfPurchasable(maxHPPurchases, 5, buttonMaxHpUpgrade, textMaxHPUpgradeMaximum, buttonMaxHPTexts))
        {
            maxHPCost = PlayerPrefs.GetInt("MaxHPCost", 1);
            textMaxHPCost.text = maxHPCost.ToString();
        }
        

        //Sets text and current cost of Speed Increase Upgrade
        textCurrentSpeed.text = PlayerPrefs.GetFloat("playerSpeed", 12).ToString();
        speedPurchases = PlayerPrefs.GetInt("SpeedPurchases", 0);
        textSpeedPurchases.text = speedPurchases.ToString() + " / 5";
        if(!CheckIfPurchasable(speedPurchases, 5, buttonSpeedUpgrade, textSpeedUpgradeMaximum, buttonSpeedTexts))
        {
            speedCost = PlayerPrefs.GetInt("SpeedCost", 1);
            textSpeedCost.text = speedCost.ToString();
        }

        //Sets Last Bullet UI
        if (PlayerPrefs.GetInt("SergeantKullerKills", 0) > 0)
        {
            lastBulletCover.SetActive(false);
            buttonLastBulletUpgrade.interactable = true;
            for(int i = 0; i < textLastBulletInfo.Length; i++)
            {
                textLastBulletInfo[i].SetActive(true);
            }
            if(CheckIfPurchasable(PlayerPrefs.GetInt("LastBulletPurchase", 0), 1, buttonLastBulletUpgrade, textLastBulletUpgradeMaximum, buttonLastBulletTexts))
            {
                textLastBulletPurchases.text = "1 / 1";
            }
        }
        else
        {
            lastBulletCover.SetActive(true);
            buttonLastBulletUpgrade.interactable = false;
        }
    }

    //checks if the corresponding button is still purchasable
    bool CheckIfPurchasable(int purchaseAmount, int maxPurchases, Button purchaseButton, TextMeshProUGUI maximumText, GameObject[] buttonTexts)
    {
        if(purchaseAmount >= maxPurchases)
        {
            purchaseButton.image.color = purchaseButton.colors.disabledColor;
            for (int i = 0; i < buttonTexts.Length; i++)
            {
                buttonTexts[i].gameObject.SetActive(false);
            }
            maximumText.text = "MAXED";
            return true;
        }
        return false;
    }

    public void UpgradeMaxHealth()
    {
        if(gameManager.instance.GetCurrentBossTokens() - maxHPCost >= 0 && maxHPPurchases < 5)
        {
            //makes sure no other shop sound effect is playing at the time
            if (!aud.isPlaying) { aud.PlayOneShot(purchaseSuccess, 0.5f); }
            //takes away cost amount from the boss tokens and shows float text
            gameManager.instance.ShowBossTokens(-maxHPCost);
            //Sets player max health up by 5 from previous amount then updated the current maxHp amount text
            PlayerPrefs.SetInt("playerMaxHP", PlayerPrefs.GetInt("playerMaxHP") + 5);
            textCurrentMaxHP.text = PlayerPrefs.GetInt("playerMaxHP").ToString();
            //updates the cost of the upgrade and then the text
            PlayerPrefs.SetInt("MaxHPCost", PlayerPrefs.GetInt("MaxHPCost") + 1);
            maxHPCost = PlayerPrefs.GetInt("MaxHPCost");
            textMaxHPCost.text = maxHPCost.ToString();
            //updates the purchases amount, if it has reached 5 then it disables the button and sets the text to maximum
            PlayerPrefs.SetInt("MaxHPPurchases", PlayerPrefs.GetInt("MaxHPPurchases") + 1);
            maxHPPurchases = PlayerPrefs.GetInt("MaxHPPurchases");
            textMaxHPPurchases.text = maxHPPurchases.ToString() + " / 5";
            CheckIfPurchasable(maxHPPurchases, 5, buttonMaxHpUpgrade, textMaxHPUpgradeMaximum, buttonMaxHPTexts);
        }
        else 
        {
            //makes sure no other shop sound effect is playing at the time
            if (!aud.isPlaying) { aud.PlayOneShot(purchaseFailed, 0.5f);}
        }
    }

    public void UpgradeSpeed()
    {
        if (gameManager.instance.GetCurrentBossTokens() - speedCost >= 0 && speedPurchases < 5)
        {
            float upAmmount = 0.5f;
            //makes sure no other shop sound effect is playing at the time
            if (!aud.isPlaying) { aud.PlayOneShot(purchaseSuccess, 0.5f); }

            //takes away cost amount from the boss tokens and shows float text
            gameManager.instance.ShowBossTokens(-speedCost);

            //Sets player speed up by 0.5 from previous amount then updated the current speed amount text
            //if it's the last time purchasing, ups it by 1 instead of 0.5
            if(speedPurchases == 4) { upAmmount = 1f; }
            PlayerPrefs.SetFloat("playerSpeed", PlayerPrefs.GetFloat("playerSpeed") + upAmmount);
            textCurrentSpeed.text = PlayerPrefs.GetFloat("playerSpeed").ToString();

            //updates the cost of the upgrade and then the text
            PlayerPrefs.SetInt("SpeedCost", PlayerPrefs.GetInt("SpeedCost") + 1);
            speedCost = PlayerPrefs.GetInt("SpeedCost");
            textSpeedCost.text = speedCost.ToString();

            //updates the purchases amount, if it has reached 5 then it disables the button and sets the text to maximum
            PlayerPrefs.SetInt("SpeedPurchases", PlayerPrefs.GetInt("SpeedPurchases") + 1);
            speedPurchases = PlayerPrefs.GetInt("SpeedPurchases");
            textSpeedPurchases.text = speedPurchases.ToString() + " / 5";
            if(speedPurchases == 4) { textSpeedUpgradeAmount.text = "Speed +1"; }
            CheckIfPurchasable(speedPurchases, 5, buttonSpeedUpgrade, textSpeedUpgradeMaximum, buttonSpeedTexts);
        }
        else
        {
            //makes sure no other shop sound effect is playing at the time
            if (!aud.isPlaying) { aud.PlayOneShot(purchaseFailed, 0.5f); }
        }
    }

    public void UpgradeLastBullet()
    {
        if (gameManager.instance.GetCurrentBossTokens() - 5 >= 0 && PlayerPrefs.GetInt("LastBulletPurchase", 0) < 1)
        {
            //makes sure no other shop sound effect is playing at the time
            if (!aud.isPlaying) { aud.PlayOneShot(purchaseSuccess, 0.5f); }

            //takes away cost amount from the boss tokens and shows float text
            gameManager.instance.ShowBossTokens(-5);

            //Sets that it was bought
            PlayerPrefs.SetInt("LastBulletPurchase", 1);

            //Make sure to disable after purchase
            CheckIfPurchasable(PlayerPrefs.GetInt("LastBulletPurchase", 0), 1, buttonLastBulletUpgrade, textLastBulletUpgradeMaximum, buttonLastBulletTexts);

            //Change purchases text
            textLastBulletPurchases.text = "1 / 1";
        }
        else
        {
            //makes sure no other shop sound effect is playing at the time
            if (!aud.isPlaying) { aud.PlayOneShot(purchaseFailed, 0.5f); }
        }
    }
}
