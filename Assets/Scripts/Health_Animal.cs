using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Health_Animal : Health
{
    public GameObject m_hitBloodDecal;

    private List<GameObject> m_hitDecals = new List<GameObject>();

    private void Awake()
    {
        OnDeath += Radgoll;
        OnDamage += SpawnHitDecal;
    }

    private void Radgoll(DamageInfo _lastDamage)
    {
        // Ragdoll the animal
        if (TryGetComponent(out Rigidbody rb))
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    private void SpawnHitDecal(DamageInfo _lastDamage)
    {
        // Spawn a blood decal
        if (m_hitBloodDecal != null)
        {
            GameObject hitDecal = Instantiate(m_hitBloodDecal, transform.position + _lastDamage.m_localHitPoint, Quaternion.identity, transform);
            hitDecal.transform.up = _lastDamage.m_hitNormal;
            m_hitDecals.Add(hitDecal);
        }
    }
}
