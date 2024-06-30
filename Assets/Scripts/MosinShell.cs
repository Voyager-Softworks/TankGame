using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShellData = Player_Gun.ShellData;

/// <summary>
/// Simple script to hold references to the case and bullet transforms of a Mosin Nagant shell.
/// </summary>
public class MosinShell : MonoBehaviour
{
    public Transform m_case;
    public Transform m_bullet;

    private ShellData m_shellData;

    public float m_minLandVelocity = 1.0f;
    [Utils.ReadOnly] public float m_lastLandSoundTime = 0.0f;
    public float m_landSoundDelay = 0.1f;

    public void SetShellData(ShellData shellData)
    {
        m_shellData = shellData;

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
            m_bullet.GetComponent<Renderer>().material.color = ShellData.COPPER_COLOR;
        }

        // dirty
        if (m_shellData.IsDirty)
        {
            m_case.GetComponent<Renderer>().material.color = ShellData.DIRTY_COLOR;
        }
        // clean
        else
        {
            m_case.GetComponent<Renderer>().material.color = ShellData.BRASS_COLOR;
        }
    }

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
        if (other.relativeVelocity.magnitude > m_minLandVelocity)
        {
            TryPlayLandSound();
        }
    }
}
