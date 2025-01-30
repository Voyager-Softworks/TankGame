using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health_Animal : Health
{
    private void Awake()
    {
        OnDeath += Radgoll;
    }

    private void Radgoll()
    {
        // Ragdoll the animal
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }
}
