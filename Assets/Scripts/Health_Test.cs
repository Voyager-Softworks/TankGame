using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DO NOT USE THIS CLASS. This is a test class for the Health class.
/// </summary>
public class Health_Test : Health
{
    [Header("Test")]
    public Rigidbody m_rb;
    [SerializeField] private float m_force = 1.0f;

    protected override void Awake() {
        base.Awake();

        // instantiate material to avoid changing the original material
    }

    public override void Damage(DamageInfo damageInfo)
    {
        if (!CanDamage())
        {
            return;
        }

        base.Damage(damageInfo);

        // change material color from Red to Black based on health
        float healthPercent = m_currentHealth / m_maxHealth;
        Color color = Color.Lerp(Color.black, Color.red, healthPercent);
        GetComponent<Renderer>().material.color = color;
    }

    public override void Die()
    {
        if (!CanDie())
        {
            return;
        }

        base.Die();

        // unlock rigidbody
        m_rb.isKinematic = false;
        m_rb.freezeRotation = false;
    }
}
