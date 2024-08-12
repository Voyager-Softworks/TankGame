using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A lever that can be interacted with.
/// @TODO: Probably create a new class for switches and lights and such (So they can interact with each other)
/// </summary>
public class Interactable_Lever : Interactable
{
    public Animator m_Animator;
    [SerializeField] private bool m_IsDown = false;
    public Interactable m_linkedInteractable = null;

    public override void OnInteract(Interacter _interacter)
    {
        base.OnInteract(_interacter);

        SetLeverState(!m_IsDown);

        // Activate linked interactable
        if (m_linkedInteractable != null)
        {
            m_linkedInteractable.OnInteract(_interacter);
        }
    }

    /// <summary>
    /// Set the state of the lever (down or up).
    /// </summary>
    /// <param name="_isDown"></param>
    public void SetLeverState(bool _isDown)
    {
        m_IsDown = _isDown;
        m_Animator.SetBool("IsDown", _isDown);

        // Play sound
        if (m_IsDown)
        {
            AudioManager.SpawnSound<AutoSound_GunEmpty>(transform.position); // temporary sound
        }
        else
        {
            AudioManager.SpawnSound<AutoSound_GunEmpty>(transform.position); // temporary sound
        }
    }
}