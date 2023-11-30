using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class summonerboss : MonoBehaviour
{
    [Header("----- Boss's Components ------")]
    [SerializeField] Renderer[] bossModelArray;
    [SerializeField] NavMeshAgent navAgent;
    [SerializeField] Transform headPos;
    [SerializeField] Animator anim;
    [SerializeField] Collider damageCol;
    [SerializeField] AudioSource aud;

    [Header("----- Boss's Stats ------")]
    [SerializeField] int HP;
    [SerializeField] int facingSpeed;
    [SerializeField] float damageDuration;
    [SerializeField] float knockbackForce;
    [SerializeField] int viewCone;
    [SerializeField] int shootCone;

    [Header("----- Boss barrier's Stats ------")]
    [SerializeField] GameObject barrierObject;
    [SerializeField] ParticleSystem barrierParticle;
    [SerializeField] Renderer barrierRenderer;
    [SerializeField] AudioClip barrierDestroy;
    [SerializeField] int barrierHP;

    [Header("----- Summoning enemy------")]
    [SerializeField] GameObject[] summonedEnemy;
    [SerializeField] GameObject eliteDrone;

    [Header("----- Boss's Sounds------")]
    [SerializeField] AudioClip hurtSound;
    [SerializeField] AudioClip deadSound;
    [Range(0, 1)][SerializeField] float bossVol;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void summonMinion()
    {

    }

    void summongEliteDrone()
    {

    }
}
