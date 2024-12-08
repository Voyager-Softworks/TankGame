using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interactable object that represents an ammo clip.
/// </summary>
public class Interactable_AmmoClip : Interactable
{
    [Header("Settings")]
    [SerializeField] protected bool m_generateRandomClip = true;
    public bool GenerateRandomClip { get { return m_generateRandomClip; } set { m_generateRandomClip = value; } }

    [Header("References")]
    [SerializeField] protected DisplayClip m_displayClip = null;

    protected override void Awake()
    {
        base.Awake();

        m_displayClip.OnClipChanged += UpdateCornerPositions;
    }

    protected void Start()
    {
        if (m_generateRandomClip)
        {
            m_displayClip.SetClip(m_displayClip.DefaultClipDefinition.GetRandomInstance());
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_displayClip.OnClipChanged -= UpdateCornerPositions;
    }

    public override void OnInteract(Interacter _interacter)
    {
        base.OnInteract(_interacter);

        // if interacter is player
        if (_interacter.GetComponentInParent<Player>() != null && m_displayClip.ClipData != null)
        {
            // add ammo to player
            Player.Instance.m_gun.AddSpareClip(m_displayClip.ClipData);
            m_displayClip.SetClip(null);
        }
    }
}
