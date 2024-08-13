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
    public GameObject m_dropOnDeath = null;
    [Range(0f,1f)] public float m_dropChance = 1f/4f;

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
        GetComponentInChildren<Renderer>().material.color = color;
    }

    public override void Die()
    {
        if (!CanDie())
        {
            return;
        }

        base.Die();

        // drop item
        if (m_dropOnDeath != null)
        {
            float random = Random.Range(0f, 1f);
            if (random < m_dropChance)
            {
                Instantiate(m_dropOnDeath, transform.position + transform.up * 2f, Quaternion.identity);

                // play sound
                AudioManager.SpawnSound<AutoSound_CannonFire>(transform.position);
            }
        }

        // unlock rigidbody
        if(m_rb != null)
        {
            m_rb.isKinematic = false;
            m_rb.freezeRotation = false;
        }

        // destroy self 5 seconds later
        Destroy(gameObject, 5f);
    }
}
