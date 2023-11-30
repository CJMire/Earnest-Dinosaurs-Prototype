using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class eliteDrone : MonoBehaviour
{
    [Header("----- Drone's Components ------")]
    [SerializeField] Renderer[] enemyModelArray;
    [SerializeField] Transform shootPosition;
    [SerializeField] Transform headPos;
    [SerializeField] Animator anim;
    [SerializeField] Collider damageCol;
    [SerializeField] AudioSource aud;

    [Header("----- Drone's Stats ------")]
    [SerializeField] int HP;

    [Header("----- Drone's Exlusive Component ------")]
    [SerializeField] ParticleSystem droneDeathParticle;

    [Header("----- Enemy Sound------")]
    [SerializeField] AudioClip hurtSound;
    [SerializeField] AudioClip deadSound;
    [Range(0, 1)][SerializeField] float enemyVol;

    Color[] modelOrigColor;
    Color barrierOrigColor;

    Vector3 targetDirection;
    bool isShooting;
    bool isDead;
    int maxHP;

    // Start is called before the first frame update
    void Start()
    {
        maxHP = HP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void fullRegen()
    {
        HP = maxHP;
    }
}
