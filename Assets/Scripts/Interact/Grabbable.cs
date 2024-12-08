using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grabbable is a base class for all grabbable objects in the game. <br/>
/// Usually to be grabbed by <see cref="Focuser"/>.
/// </summary>
public class Grabbable : Focusable
{
    protected static List<Grabbable> s_allGrabbables = new List<Grabbable>();
    public static List<Grabbable> AllGrabbables { get { return s_allGrabbables; } }

    [Header("Settings")]
    [SerializeField] private bool m_isGrabbable = true;
    public bool IsGrabbable { get { return m_isGrabbable; } set { m_isGrabbable = value; } }

    protected override void Awake()
    {
        base.Awake();

        // add to list
        if (!s_allGrabbables.Contains(this))
        {
            s_allGrabbables.Add(this);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

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

        transform.SetParent(_focuser.m_grabPoint); 
        transform.localPosition = Vector3.zero;

        // cant be focused while grabbed
        IsFocusable = false;

        // disable rigidbody
        Rigidbody rb = GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    /// <summary>
    /// Called when this object is released
    /// </summary>
    /// <param name="_focuser"></param>
    public virtual void OnRelease(Focuser _focuser)
    {
        if (!m_isGrabbable)
        {
            return;
        }

        Debug.Log($"Grabbable.OnRelease | {gameObject.name} was released by {_focuser.name}.");

        _focuser.m_grabbed = null;

        transform.SetParent(null);

        // can be focused again
        IsFocusable = true;

        // enable rigidbody
        Rigidbody rb = GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }
}
