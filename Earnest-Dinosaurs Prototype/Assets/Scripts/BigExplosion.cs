using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigExplosion : MonoBehaviour
{
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip explosion;
    [Range(0f,1f)][SerializeField] float explosionVol;

    void Start()
    {
        aud.PlayOneShot(explosion, explosionVol);
    }
}
