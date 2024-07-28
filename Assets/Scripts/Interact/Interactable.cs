using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Interactable is a base class for all interactable objects in the game. <br/>
/// Usually to be interacted by <see cref="Interacter"/>.
/// </summary>
public class Interactable : MonoBehaviour
{
    protected static List<Interactable> s_allInteractables = new List<Interactable>();
    public static List<Interactable> AllInteractables { get { return s_allInteractables; } }

    [Header("Settings")]
    public bool m_isInteractable = true;
    [SerializeField] protected float m_interactRange = 5f; // takes the biggest, this or interacter's range
    public float InteractRange { get { return m_interactRange; } }
    [SerializeField] protected bool m_destroyOnInteract = false;

    protected virtual void Awake()
    {
        // add to list
        if (!s_allInteractables.Contains(this))
        {
            s_allInteractables.Add(this);
        }
    }

    protected virtual void OnDestroy()
    {
        // remove from list
        if (s_allInteractables.Contains(this))
        {
            s_allInteractables.Remove(this);
        }
    }

    /// <summary>
    /// Called when this object is interacted with.
    /// </summary>
    public virtual void OnInteract(Interacter _interacter)
    {
        Debug.Log($"Interactable.OnInteract | {gameObject.name} was interacted with by {_interacter.name}.");

        // destroy if needed
        if (m_destroyOnInteract)
        {
            Destroy(gameObject);
        }
    }
}
