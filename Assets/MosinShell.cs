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

    public float m_minVelocity = 1.0f;
    [Utils.ReadOnly] public float m_lastLandSoundTime = 0.0f;
    public float m_landSoundDelay = 0.1f;

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
        if (other.relativeVelocity.magnitude > m_minVelocity)
        {
            TryPlayLandSound();
        }
    }
}
