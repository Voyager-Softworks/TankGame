using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_AmmoClip : Interactable
{
    [Header("Settings")]
    [SerializeField] protected bool m_generateRandomClip = true;

    [Header("References")]
    [SerializeField] protected ClipDefinition m_clipType = null;
    [SerializeField] protected DisplayClip m_displayClip = null;

    protected override void Awake()
    {
        base.Awake();

        if (m_generateRandomClip)
        {
            m_displayClip.SetClip(m_clipType.GetRandomInstance());
        }
    }

    public override void OnInteract(Interacter _interacter)
    {
        base.OnInteract(_interacter);

        // if interacter is player
        if (_interacter.GetComponentInParent<Player>() != null)
        {
            // add ammo to player
            Player.Instance.m_gun.SpareClips.Add(m_displayClip.ClipData);
            m_displayClip.SetClip(null);
        }
    }
}
