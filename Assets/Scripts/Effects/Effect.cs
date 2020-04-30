using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect : MonoBehaviour
{
    protected abstract float Lifetime { get; }    

    protected virtual void Start()
    {
        Destroy(gameObject, Lifetime);
    }
}
