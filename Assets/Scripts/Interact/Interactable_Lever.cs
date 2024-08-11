using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A lever that can be interacted with.
/// </summary>
public class Interactable_Lever : Interactable
{
    public Animator m_Animator;
    [Utils.ReadOnly, SerializeField] private bool m_IsDown = false;

    public override void OnInteract(Interacter _interacter)
    {
        base.OnInteract(_interacter);

        SetLeverState(!m_IsDown);
    }

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