using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple util script to destroy the game object it is attached to.
/// </summary>
public class DestroySelf : MonoBehaviour
{
    public bool m_destroyOnStart = true;
    public float m_startDelay = 0.0f;

    private void Start() {
        if (m_destroyOnStart) {
            Destroy(gameObject, m_startDelay);
        }
    }

    public void DestroySelfNow() {
        Destroy(gameObject);
    }
}
