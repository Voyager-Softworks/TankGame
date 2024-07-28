using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple script to hold references to the case and bullet transforms of a Mosin Nagant shell.
/// </summary>
public class MosinShell : MonoBehaviour
{
    public Transform m_case;
    public Transform m_bullet;

    public float m_minLandVelocity = 1.0f;
    [Utils.ReadOnly] public float m_lastLandSoundTime = 0.0f;
    public float m_landSoundDelay = 0.1f;

    [Header("References")]
    [SerializeField] protected ShellDefinition m_defaultShellDefinition = null;

    [Header("Data")]
    [Utils.ReadOnly, SerializeField] protected ShellDefinition m_shellData;
    public ShellDefinition ShellData { get { return m_shellData; } }

    public void SetShellDefinition(ShellDefinition _ShellDefinition)
    {
        // copy data
        m_shellData = _ShellDefinition.GetCopy();

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

    private void OnCollisionEnter(Collision other) {
        // play land sound
        if (other.relativeVelocity.magnitude > m_minLandVelocity)
        {
            TryPlayLandSound();
        }
    }
}
