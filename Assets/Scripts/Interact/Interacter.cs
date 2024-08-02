using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Interacter is a base class for things that can interact with <see cref="Interactable"/> objects.
/// </summary>
public class Interacter : MonoBehaviour
{
    [Tooltip("The range at which the interacter can interact with objects.")]
    public float m_interactRange = 5f; // takes the biggest, this or interactable's range
    [Tooltip("The interact angle from the cursor to the interactable object.")]
    public float m_maxInteractAngle = 15f;
    [Tooltip("The angle at which distance is prioritized over angle.")]
    public float m_minInteractAngle = 5f;

    public List<Interactable> m_nearbyInteractables = new List<Interactable>();
    public Interactable m_focusedInteractable = null;

    [Header("Input Actions")]
    public Camera m_camera = null;

    private void OnDrawGizmos()
    {
        // min angle LRUD lines from camera
        Vector3 left = Quaternion.Euler(0, -m_minInteractAngle, 0) * m_camera.transform.forward;
        Vector3 right = Quaternion.Euler(0, m_minInteractAngle, 0) * m_camera.transform.forward;
        Vector3 up = Quaternion.Euler(-m_minInteractAngle, 0, 0) * m_camera.transform.forward;
        Vector3 down = Quaternion.Euler(m_minInteractAngle, 0, 0) * m_camera.transform.forward;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_camera.transform.position, m_camera.transform.position + left * m_interactRange);
        Gizmos.DrawLine(m_camera.transform.position, m_camera.transform.position + right * m_interactRange);
        Gizmos.DrawLine(m_camera.transform.position, m_camera.transform.position + up * m_interactRange);
        Gizmos.DrawLine(m_camera.transform.position, m_camera.transform.position + down * m_interactRange);

        // max angle LRUD lines from camera
        left = Quaternion.Euler(0, -m_maxInteractAngle, 0) * m_camera.transform.forward;
        right = Quaternion.Euler(0, m_maxInteractAngle, 0) * m_camera.transform.forward;
        up = Quaternion.Euler(-m_maxInteractAngle, 0, 0) * m_camera.transform.forward;
        down = Quaternion.Euler(m_maxInteractAngle, 0, 0) * m_camera.transform.forward;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(m_camera.transform.position, m_camera.transform.position + left * m_interactRange);
        Gizmos.DrawLine(m_camera.transform.position, m_camera.transform.position + right * m_interactRange);
        Gizmos.DrawLine(m_camera.transform.position, m_camera.transform.position + up * m_interactRange);
        Gizmos.DrawLine(m_camera.transform.position, m_camera.transform.position + down * m_interactRange);
    }

    protected virtual void Update()
    {
        // check for interactable
        m_focusedInteractable = GetBestInteractable();

        if (m_focusedInteractable != null)
        {
            // check for input
            if (InputManager.PlayerSpecial.Interact.WasPerformedThisFrame())
            {
                InteractWith(m_focusedInteractable);
            }

            // get renderer bounds
            List<Renderer> renderers = new List<Renderer>(m_focusedInteractable.GetComponentsInChildren<Renderer>());
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            // debug shower is circle, so we just need the radius
            float radius = Vector3.Distance(m_camera.WorldToScreenPoint(bounds.min), m_camera.WorldToScreenPoint(bounds.max)) / 2;
            
            // set debug shower
            Player.Instance.DEBUG_interactShower.gameObject.SetActive(true);
            Player.Instance.DEBUG_interactShower.position = m_camera.WorldToScreenPoint(bounds.center);
            Player.Instance.DEBUG_interactShower.sizeDelta = new Vector2(radius * 2, radius * 2);
        }
        else
        {
            // disable debug shower
            Player.Instance.DEBUG_interactShower.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Makes this object interact with the given interactable object.
    /// </summary>
    /// <param name="_interactable"></param>
    public virtual void InteractWith(Interactable _interactable)
    {
        // null check
        if (_interactable == null)
        {
            return;
        }

        // interact
        _interactable.OnInteract(this);
    }

    /// <summary>
    /// Gets the best interactable object in range. <br/>
    /// Priority: <br/>
    /// 1. Raycast hit from screen center <br/>
    /// 2. Closest to screen center (within max angle) <br/>
    /// 3. Closest to interacter (within min angle)
    /// </summary>
    /// <returns></returns>
    private Interactable GetBestInteractable()
    {
        // clear list
        m_nearbyInteractables.Clear();

        // get all interactables
        foreach (Interactable interactable in Interactable.AllInteractables)
        {
            // check if interactable is in range
            if (Vector3.Distance(interactable.transform.position, m_camera.transform.position) <= MaxRange(interactable))
            {
                // skip if not interactable
                if (interactable.IsInteractable == false)
                {
                    continue;
                }
                
                m_nearbyInteractables.Add(interactable);
            }
        }

        // Raycast:
        RaycastHit hit;
        if (Physics.Raycast(m_camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, MaxRange()))
        {
            if (hit.collider != null)
            {
                Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
                if (interactable != null && m_nearbyInteractables.Contains(interactable))
                {
                    return interactable;
                }
            }
        }

        Interactable bestInteractable = null;

        // check inner angle
        List<Interactable> m_innerInteractables = new List<Interactable>();
        List<Interactable> m_outerInteractables = new List<Interactable>();
        foreach (Interactable interactable in m_nearbyInteractables)
        {
            Vector3 screenPos = m_camera.WorldToScreenPoint(interactable.transform.position);
            float angle = Vector3.Angle(m_camera.transform.forward, interactable.transform.position - m_camera.transform.position);

            if (angle <= m_minInteractAngle)
            {
                m_innerInteractables.Add(interactable);
            }
            else if (angle <= m_maxInteractAngle)
            {
                m_outerInteractables.Add(interactable);
            }
        }
        // if we have any min, check best dist
        if (m_innerInteractables.Count > 0)
        {
            bestInteractable = m_innerInteractables[0];
            float bestDistance = Vector3.Distance(bestInteractable.transform.position, m_camera.transform.position);
            foreach (Interactable interactable in m_innerInteractables)
            {
                float distance = Vector3.Distance(interactable.transform.position, m_camera.transform.position);
                if (distance < bestDistance)
                {
                    bestInteractable = interactable;
                    bestDistance = distance;
                }
            }
        }
        // otherwise check best angle of outer
        else if (m_outerInteractables.Count > 0)
        {
            bestInteractable = m_outerInteractables[0];
            float bestAngle = Vector3.Angle(m_camera.transform.forward, bestInteractable.transform.position - m_camera.transform.position);
            foreach (Interactable interactable in m_outerInteractables)
            {
                float angle = Vector3.Angle(m_camera.transform.forward, interactable.transform.position - m_camera.transform.position);
                if (angle < bestAngle)
                {
                    bestInteractable = interactable;
                    bestAngle = angle;
                }
            }
        }

        return bestInteractable;
    }

    /// <summary>
    /// Gets the biggest range between the interacter and the interactable. <br/>
    /// (or just the interacter's range if interactable is null)
    /// </summary>
    /// <param name="_interactable"></param>
    /// <returns>The maximum range of interaction.</returns>
    private float MaxRange(Interactable _interactable = null)
    {
        if (_interactable != null)
        {
            return Mathf.Max(m_interactRange, _interactable.InteractRange);
        }
        else
        {
            return m_interactRange;
        }
    }
}