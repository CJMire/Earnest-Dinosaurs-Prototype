using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class gunStats : ScriptableObject
{
    public int shootDamage;
    public int shootDist;
    public float shootRate;
    public int ammoCur;
    public int ammoMax;
    public float reloadTime;

    public GameObject model;
    public ParticleSystem hitEffect;
    public AudioClip shootSound;
    [Range(0, 1)] public float shootSoundVol;
}
