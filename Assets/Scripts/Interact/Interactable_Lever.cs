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
    [SerializeField] private bool m_lockDown = false;
    [SerializeField] private bool m_lockUp = false;
    [SerializeField] private Electrical m_linkedElectrical;

    public override void OnInteract(Interacter _interacter)
    {
        base.OnInteract(_interacter);

        SetLeverState(!m_IsDown);
    }

    /// <summary>
    /// Set the state of the lever (down or up).
    /// </summary>
    /// <param name="_isDown"></param>
    public void SetLeverState(bool _isDown)
    {
        // Locks
        if (m_lockDown && _isDown)
        {
            return;
        }
        if (m_lockUp && !_isDown)
        {
            return;
        }

        // Set the state
        m_IsDown = _isDown;
        m_Animator.SetBool("IsDown", _isDown);

        // Update the linked electrical
        if (m_linkedElectrical != null)
        {
            m_linkedElectrical.SetPowerSource(_isDown);
        }

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