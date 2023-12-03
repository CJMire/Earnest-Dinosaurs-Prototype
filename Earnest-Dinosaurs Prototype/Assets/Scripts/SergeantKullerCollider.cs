using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SergeantKullerCollider : MonoBehaviour, IDamage
{
    private void Start()
    {
        tag = "Boss";
    }

    public void takeDamage(int dmg)
    {
        if (this.GetComponent<Collider>() == GetComponentInParent<SergeantKuller>().GetWeakspot())
            GetComponentInParent<SergeantKuller>().SetDoubleDamage(true);
        GetComponentInParent<SergeantKuller>().takeDamage(dmg);
    }
}
