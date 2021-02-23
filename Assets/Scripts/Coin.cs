using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : PoolObject
{
    private void OnTriggerEnter(Collider other)
    {
        gameObject.SetActive(false);
    }

    public override void ResetState()
    {
        gameObject.SetActive(true);
    }
}
