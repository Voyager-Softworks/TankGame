using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A light that can be interacted with.
/// </summary>
public class Interactable_Light : Interactable
{
    public Animator m_Animator;
    [SerializeField] private bool m_IsOn = false;

    public override void OnInteract(Interacter _interacter)
    {
        base.OnInteract(_interacter);

        SetLightState(!m_IsOn);
    }

    /// <summary>
    /// Set the state of the light (on or off).
    /// </summary>
    /// <param name="_isOn"></param>
    public void SetLightState(bool _isOn)
    {
        m_IsOn = _isOn;
        m_Animator.SetBool("IsOn", _isOn);

        // Play sound
        if (m_IsOn)
        {
            AudioManager.SpawnSound<AutoSound_GunEmpty>(transform.position); // temporary sound
        }
        else
        {
            AudioManager.SpawnSound<AutoSound_GunEmpty>(transform.position); // temporary sound
        }
    }
}