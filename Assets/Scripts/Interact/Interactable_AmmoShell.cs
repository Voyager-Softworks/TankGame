using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interactable object that represents an ammo shell.
/// </summary>
public class Interactable_AmmoShell : Interactable
{
    [Header("Settings")]
    [SerializeField] protected bool m_generateRandomShell = true;
    public bool GenerateRandomShell { get { return m_generateRandomShell; } set { m_generateRandomShell = value; } }

    [Header("References")]
    [SerializeField] protected DisplayShell m_displayShell = null;

    protected void Start()
    {
        if (m_generateRandomShell)
        {
            m_displayShell.SetShell(m_displayShell.DefaultShellDefinition.GetRandomInstance());
        }
    }

    public override void OnInteract(Interacter _interacter)
    {
        base.OnInteract(_interacter);

        // if interacter is player
        if (_interacter.GetComponentInParent<Player>() != null && m_displayShell.ShellData != null)
        {
            // add ammo to player
            // @TODO: allow spare shells &/or ballance clips on add
            ClipDefinition newCLip = Player.Instance.m_gun.m_defaultClip.GetRandomInstance(0);
            newCLip.m_shells.Add(m_displayShell.ShellData);
            Player.Instance.m_gun.SpareClips.Add(newCLip);
        }
    }
}
