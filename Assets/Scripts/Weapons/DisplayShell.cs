using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple script to hold references to the case and bullet transforms of a Mosin Nagant shell.
/// </summary>
public class DisplayShell : MonoBehaviour
{
    public Transform m_case;
    public Transform m_bullet;

    public float m_minLandVelocity = 1.0f;
    [Utils.ReadOnly] public float m_lastLandSoundTime = 0.0f;
    public float m_landSoundDelay = 0.1f;

    [Header("References")]
    [SerializeField, Tooltip("The type of shell this will represent")] protected ShellDefinition m_defaultShellDefinition = null;
    public ShellDefinition DefaultShellDefinition { get { return m_defaultShellDefinition; } }

    [Header("Data")]
    [Utils.ReadOnly, SerializeField, Tooltip("The current shell this is representing")] protected ShellDefinition m_shellData;
    public ShellDefinition ShellData { get { return m_shellData; } }

    public void SetShell(ShellDefinition _ShellDefinition)
    {
        // copy data
        m_shellData = _ShellDefinition;

        UpdateVisuals();
    }

    /// <summary>
    /// Updates the visuals of the shell to match the shell data.
    /// </summary>
    public void UpdateVisuals()
    {
        if (m_shellData == null)
        {
            return;
        }

        // spent
        if (m_shellData.IsSpent)
        {
            m_bullet.localScale = Vector3.zero;
            // not interactable
            GetComponent<Interactable_AmmoShell>().IsInteractable = false;
        }
        else
        {
            m_bullet.localScale = Vector3.one;
            m_bullet.GetComponent<Renderer>().material.color = m_shellData.CopperColor;
        }

        // dirty
        if (m_shellData.IsDirty)
        {
            m_case.GetComponent<Renderer>().material.color = m_shellData.DirtyColor;
        }
        // clean
        else
        {
            m_case.GetComponent<Renderer>().material.color = m_shellData.BrassColor;
        }
    }

    private void TouchGround()
    {
        // mark as dirty if not spent
        if (!m_shellData.IsSpent)
        {
            m_shellData.SetDirty(true);
        }

        // update visuals
        UpdateVisuals();
    }

    /// <summary>
    /// Spawns a landing sound if enough time has passed since the last one.
    /// </summary>
    public void TryPlayLandSound()
    {
        if (Time.time - m_lastLandSoundTime > m_landSoundDelay)
        {
            m_lastLandSoundTime = Time.time;

            // spawn and parent
            AutoSound shellSound = AudioManager.SpawnSound<AutoSound_CasingLand>(transform.position);
            shellSound.transform.parent = transform;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        // only if not kinematic
        if (GetComponent<Rigidbody>().isKinematic)
        {
            return;
        }

        // touch ground (not child of this, and not player
        if (!other.transform.IsChildOf(transform) && !other.transform.IsChildOf(Player.Instance.transform))
        {
            // @TODO: if surface is dirty?
            TouchGround();

            // play land sound if fast enough
            if (other.relativeVelocity.magnitude > m_minLandVelocity)
            {
                TryPlayLandSound();
            }
        }
    }
}
