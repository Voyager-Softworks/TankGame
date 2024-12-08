using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interactable is a base class for all interactable objects in the game. <br/>
/// Usually to be interacted by <see cref="Focuser"/>.
/// </summary>
public class Interactable : Focusable
{
    protected static List<Interactable> s_allInteractables = new List<Interactable>();
    public static List<Interactable> AllInteractables { get { return s_allInteractables; } }

    [Header("Settings")]
    [SerializeField] private bool m_isInteractable = true;
    public bool IsInteractable { get { return m_isInteractable; } set { m_isInteractable = value; } }
    [SerializeField] protected bool m_destroyOnInteract = false;

    protected override void Awake()
    {
        base.Awake();

        // add to list
        if (!s_allInteractables.Contains(this))
        {
            s_allInteractables.Add(this);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // remove from list
        if (s_allInteractables.Contains(this))
        {
            s_allInteractables.Remove(this);
        }
    }

    /// <summary>
    /// Called when this object is interacted with.
    /// </summary>
    public virtual void OnInteract(Focuser _interacter)
    {
        if (!m_isInteractable)
        {
            return;
        }

        Debug.Log($"Interactable.OnInteract | {gameObject.name} was interacted with by {_interacter.name}.");

        // destroy if needed
        if (m_destroyOnInteract)
        {
            Destroy(gameObject);
        }
    }
}
