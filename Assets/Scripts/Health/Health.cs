using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A base class for objects that have health.
/// </summary>
public abstract class Health : MonoBehaviour
{
    /// <summary>
    /// Information about a damage event.
    /// </summary>
    public class DamageInfo
    {
        public float m_damage;
        public GameObject m_sourceObject;
        public Vector3 m_originPoint;
        public Vector3 m_hitPoint;
        public Vector3 m_hitNormal;
        public float m_hitForce;

        public float m_time;

        public Vector3 Direction
        {
            get
            {
                return (m_hitPoint - m_originPoint).normalized;
            }
        }

        public DamageInfo(float damage, GameObject sourceObject, Vector3 originPoint, Vector3 hitPoint, Vector3 hitNormal, float hitForce)
        {
            m_damage = damage;
            m_sourceObject = sourceObject;
            m_originPoint = originPoint;
            m_hitPoint = hitPoint;
            m_hitNormal = hitNormal;
            m_hitForce = hitForce;
            m_time = Time.time;
        }
    }

    protected static List<Health> s_allHealth = new List<Health>();

    [Header("Settings")]
    public float m_maxHealth = 100.0f;
    [SerializeField] protected float m_currentHealth = 100.0f;
    [SerializeField] protected bool m_destroyOnDeath = true;
    [SerializeField] protected bool m_allowDamageAfterDeath = true;
    [SerializeField] protected bool m_allowHealAfterDeath = false;

    [Header("State")]
    [SerializeField, Utils.ReadOnly] protected bool m_isDead = false;
    public bool IsDead { get { return m_isDead; } }

    [Header("References")]
    public GameObject m_damageFX = null;
    public GameObject m_deathFX = null;

    [Header("Events")]
    public System.Action OnDamage;
    public System.Action OnHeal;
    public System.Action OnDeath;

    protected List<DamageInfo> m_damageHistory = new List<DamageInfo>();

    protected virtual void Awake()
    {
        if (!s_allHealth.Contains(this))
        {
            s_allHealth.Add(this);
        }
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        m_currentHealth = m_maxHealth;
    }

    protected virtual void Update()
    {
        if (!m_isDead && m_currentHealth <= 0.0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Damages the object.
    /// <br/>Ensure to call <see cref="CanDamage"/> in derived classes before applying damage!
    /// </summary>
    /// <param name="damage">The amount of damage to deal.</param>
    public virtual void Damage(DamageInfo _damageInfo)
    {
        if (!CanDamage())
        {
            return;
        }

        m_damageHistory.Add(_damageInfo);

        m_currentHealth -= _damageInfo.m_damage;

        if (m_damageFX != null)
        {
            Instantiate(m_damageFX, _damageInfo.m_hitPoint, Quaternion.LookRotation(_damageInfo.m_hitNormal));
        }

        OnDamage?.Invoke();

        if (m_currentHealth <= 0.0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals the object.
    /// <br/>Ensure to call <see cref="CanHeal"/> in derived classes before applying healing!
    /// </summary>
    /// <param name="heal">The amount of health to restore.</param>
    public virtual void Heal(float _health)
    {
        if (!CanHeal())
        {
            return;
        }

        m_currentHealth += _health;
        if (m_currentHealth > m_maxHealth)
        {
            m_currentHealth = m_maxHealth;
        }

        OnHeal?.Invoke();
    }

    /// <summary>
    /// Kills the object.
    /// <br/>Ensure to call <see cref="CanDie"/> in derived classes before killing!
    /// </summary>
    public virtual void Die()
    {
        if (!CanDie())
        {
            return;
        }

        m_isDead = true;

        if (m_deathFX != null)
        {
            Instantiate(m_deathFX, transform.position, Quaternion.identity);
        }

        if (m_destroyOnDeath)
        {
            Destroy(gameObject);
        }

        OnDeath?.Invoke();
    }

    /// <summary>
    /// Returns true if the object can be damaged.
    /// </summary>
    /// <returns></returns>
    protected bool CanDamage()
    {
        if (m_isDead && !m_allowDamageAfterDeath)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if the object can be healed.
    /// </summary>
    /// <returns></returns>
    protected bool CanHeal()
    {
        if (m_isDead && !m_allowHealAfterDeath)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if the object can die.
    /// </summary>
    /// <returns></returns>
    protected bool CanDie()
    {
        if (m_isDead)
        {
            return false;
        }

        return true;
    }

    protected virtual void OnDestroy()
    {
        if (s_allHealth.Contains(this))
        {
            s_allHealth.RemoveAll(h => h == this);
        }
    }

    /// <summary>
    /// Gets the list of all current health objects.
    /// </summary>
    /// <returns></returns>
    public static List<Health> GetAllHealth()
    {
        return s_allHealth;
    }

    // Custom Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(Health), true)]
    public class HealthEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Health health = (Health)target;
            bool didChange = false;

            base.OnInspectorGUI();

            // space
            GUILayout.Space(10);

            // begin DEBUG vertical
            GUILayout.BeginVertical("DEBUG", "window");

            if (GUILayout.Button("Damage 10"))
            {
                health.Damage(new DamageInfo(10.0f, null, Vector3.zero, Vector3.zero, Vector3.zero, 0f));
            }

            if (GUILayout.Button("Heal 10"))
            {
                health.Heal(10.0f);
            }

            if (GUILayout.Button("Die"))
            {
                health.Die();
            }

            GUILayout.EndVertical();

            // if (!Application.isPlaying)
            // {

            //     if (health.m_currentHealth != health.m_maxHealth)
            //     {
            //         health.m_currentHealth = health.m_maxHealth;
            //         didChange = true;
            //     }
            // }

            // if (didChange)
            // {
            //     EditorUtility.SetDirty(health);
            // }
        }
    }
#endif
}