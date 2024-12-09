using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grabbable is a base class for all grabbable objects in the game. <br/>
/// Usually to be grabbed by <see cref="Focuser"/>.
/// </summary>
[RequireComponent(typeof(Focusable))]
public class Grabbable : MonoBehaviour
{
    protected static List<Grabbable> s_allGrabbables = new List<Grabbable>();
    public static List<Grabbable> AllGrabbables { get { return s_allGrabbables; } }

    [Header("Settings")]
    [SerializeField] private bool m_isGrabbable = true;
    public bool IsGrabbable { get { return m_isGrabbable; } set { m_isGrabbable = value; } }

    private Focuser m_grabbedBy = null;

    private Rigidbody m_rb = null;
    public Rigidbody RB
    {
        get
        {
            if (m_rb == null)
            {
                m_rb = GetComponentInChildren<Rigidbody>();
            }
            return m_rb;
        }
    }

    Focusable m_focusable = null;
    public Focusable Focusable
    {
        get
        {
            if (m_focusable == null)
            {
                m_focusable = GetComponent<Focusable>();
            }
            return m_focusable;
        }
    }

    protected virtual void Awake()
    {
        // add to list
        if (!s_allGrabbables.Contains(this))
        {
            s_allGrabbables.Add(this);
        }

        // get rigidbody
        m_rb = GetComponentInChildren<Rigidbody>();

        if (m_rb == null)
        {
            Debug.LogWarning($"Grabbable.Awake | {gameObject.name} does not have a Rigidbody component.");
        }
    }

    protected virtual void OnDestroy()
    {
        // drop if grabbed
        if (m_grabbedBy != null)
        {
            OnDrop(m_grabbedBy);
        } 

        // remove from list
        if (s_allGrabbables.Contains(this))
        {
            s_allGrabbables.Remove(this);
        }
    }

    /// <summary>
    /// Called when this object is grabbed
    /// </summary>
    /// <param name="_focuser"></param>
    public virtual void OnGrab(Focuser _focuser)
    {
        if (!m_isGrabbable)
        {
            return;
        }

        Debug.Log($"Grabbable.OnGrab | {gameObject.name} was grabbed by {_focuser.name}.");

        _focuser.m_grabbed = this;
        m_grabbedBy = _focuser;
    }

    /// <summary>
    /// Called when this object is dropped
    /// </summary>
    /// <param name="_focuser"></param>
    public virtual void OnDrop(Focuser _focuser)
    {
        if (!m_isGrabbable)
        {
            return;
        }

        Debug.Log($"Grabbable.OnDrop | {gameObject.name} was dropped by {_focuser.name}.");

        _focuser.m_grabbed = null;
        m_grabbedBy = null;
    }
}
