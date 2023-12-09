using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bossBarrier : MonoBehaviour, IBossBarrier
{
    [Header("----- Barrier's Components ------")]
    [SerializeField] AudioSource aud;
    [SerializeField] ParticleSystem ricochetParticle;
    [SerializeField] AudioClip ricochetSound;
    [SerializeField] float yPosAdjustment;
    [Range(0, 1)][SerializeField] float barrierVol;


    public void takeNoDamage(Transform hitPosition)
    {
        Instantiate(ricochetParticle, hitPosition.position + new Vector3(0.0f, yPosAdjustment, 0.0f), ricochetParticle.transform.rotation);
        aud.PlayOneShot(ricochetSound, barrierVol);
    }
}
