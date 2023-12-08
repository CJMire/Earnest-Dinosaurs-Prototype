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
    [Header("----- Audio Clips -----")]
    [SerializeField] AudioClip purchaseSuccess;
    [SerializeField] AudioClip purchaseFailed;

    [Header("----- Max HP Upgrade Components-----")]
    [SerializeField] TextMeshProUGUI textCurrentMaxHP;
    [SerializeField] TextMeshProUGUI textMaxHPCost;
    private int maxHPCost;

    //Keys:
    // - "MaxHPCost" - int

    void Start()
    {
        //Sets text and current cost of Max Hp Increase Upgrade
        textCurrentMaxHP.text = PlayerPrefs.GetInt("playerMaxHP").ToString();
        maxHPCost = PlayerPrefs.GetInt("MaxHPCost", 1);
        textMaxHPCost.text = maxHPCost.ToString();
    }

    public void UpgradeMaxHealth()
    {

    }
}
