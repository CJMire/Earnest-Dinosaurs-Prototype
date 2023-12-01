using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SergeantKuller : MonoBehaviour
{
    [SerializeField] Collider weakSpot;

    [Header("----- Attack Components -----")]
    [SerializeField] GameObject bullets;
    [SerializeField] GameObject seekerMissles;
    [SerializeField] GameObject lasers;

    [Header("----- Attack Instantiation Points")]
    [SerializeField] Transform bigGunBarrelAL;
    [SerializeField] Transform bigGunBarrelBL;
    [SerializeField] Transform bigGunBarrelAR;
    [SerializeField] Transform bigGunBarrelBR;
    [SerializeField] Transform smallGunBarrellAL;
    [SerializeField] Transform smallGunBarrellBL;
    [SerializeField] Transform smallGunBarrellAR;
    [SerializeField] Transform smallGunBarrellBR;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
