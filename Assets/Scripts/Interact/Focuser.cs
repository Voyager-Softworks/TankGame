using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// Focuser is a base class for things that can be focused on and focused on. <br/>
/// Usually controlled by the player.
/// </summary>
public class Focuser : MonoBehaviour
{
    [Tooltip("The range at which the focuser can focus on objects.")]
    public float m_focusRange = 5f; // takes the biggest, this or focusable's range
    [Tooltip("The focus angle from the cursor to the focusable object.")]
    public float m_maxFocusAngle = 15f;
    [Tooltip("The angle at which distance is prioritized over angle.")]
    public float m_minFocusAngle = 5f;

    public List<Focusable> m_nearbyFocusables = new List<Focusable>();
    public Focusable m_focused = null;

    public Transform m_grabPoint = null;
    public Grabbable m_grabbed = null;

    [Header("Input Actions")]
    public Camera m_camera = null;

    protected virtual void Update()
    {
        // check for focusable
        m_focused = GetBestFocusable();

        // check for interact input
        if (InputManager.PlayerSpecial.Interact.WasPerformedThisFrame())
        {
            if (m_focused is Interactable interactable)
            {
                interactable.OnInteract(this);
            }
        }

        // check for grab input
        if (InputManager.PlayerSpecial.Grab.WasPerformedThisFrame())
        {
            if (m_grabbed != null)
            {
                m_grabbed.OnRelease(this);
            }
            else if (m_focused is Grabbable grabbable)
            {
                grabbable.OnGrab(this);
            }
        }

        // Update UI
        if (m_focused == null)
        {
            // disable bounds
            Player.Instance.m_ui.m_focusTL.gameObject.SetActive(false);
            Player.Instance.m_ui.m_focusBR.gameObject.SetActive(false);
            Player.Instance.m_ui.m_focusTR.gameObject.SetActive(false);
            Player.Instance.m_ui.m_focusBL.gameObject.SetActive(false);
            Player.Instance.m_ui.m_focusC.gameObject.SetActive(false);
        }
        else
        {
            Vector3[] corners = m_focused.WorldCorners;

            // get screen points
            Vector3[] screenPoints = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                screenPoints[i] = m_camera.WorldToScreenPoint(corners[i]);
            }

            // get screen corners
            Vector3 tl = screenPoints[0];
            Vector3 tr = screenPoints[0];
            Vector3 bl = screenPoints[0];
            Vector3 br = screenPoints[0];
            for (int i = 1; i < 8; i++)
            {
                // tl = smallest x, largest y
                tl.x = Mathf.Min(tl.x, screenPoints[i].x);
                tl.y = Mathf.Max(tl.y, screenPoints[i].y);
                // tr = largest x, largest y
                tr.x = Mathf.Max(tr.x, screenPoints[i].x);
                tr.y = Mathf.Max(tr.y, screenPoints[i].y);
                // bl = smallest x, smallest y
                bl.x = Mathf.Min(bl.x, screenPoints[i].x);
                bl.y = Mathf.Min(bl.y, screenPoints[i].y);
                // br = largest x, smallest y
                br.x = Mathf.Max(br.x, screenPoints[i].x);
                br.y = Mathf.Min(br.y, screenPoints[i].y);
            }

            // prevent overlap with sprite
            float spriteWidth = Player.Instance.m_ui.m_focusTL.rect.width;
            float spriteHeight = Player.Instance.m_ui.m_focusTL.rect.height;
            // if top left is overlapping bottom right, move away form each other
            float TLBR_overlapX = (tl.x + spriteWidth) - (br.x - spriteWidth);
            if (TLBR_overlapX > 0)
            {
                tl.x -= TLBR_overlapX / 2;
                br.x += TLBR_overlapX / 2;
            }
            float TLBR_overlapY = (tl.y - spriteHeight) - (br.y + spriteHeight);
            if (TLBR_overlapY < 0)
            {
                tl.y -= TLBR_overlapY / 2;
                br.y += TLBR_overlapY / 2;
            }
            // if top right is overlapping bottom left, move away form each other
            float TRBL_overlapX = (tr.x - spriteWidth) - (bl.x + spriteWidth);
            if (TRBL_overlapX < 0)
            {
                tr.x -= TRBL_overlapX / 2;
                bl.x += TRBL_overlapX / 2;
            }
            float TRBL_overlapY = (tr.y - spriteHeight) - (bl.y + spriteHeight);
            if (TRBL_overlapY < 0)
            {
                tr.y -= TRBL_overlapY / 2;
                bl.y += TRBL_overlapY / 2;
            }

            // get center of object on screen
            Vector3 center = Vector3.zero;
            foreach (Vector3 point in corners)
            {
                center += point;
            }
            center /= corners.Length;
            center = m_camera.WorldToScreenPoint(center);

            // set bounds
            Player.Instance.m_ui.m_focusTL.gameObject.SetActive(true);
            Player.Instance.m_ui.m_focusBR.gameObject.SetActive(true);
            Player.Instance.m_ui.m_focusTR.gameObject.SetActive(true);
            Player.Instance.m_ui.m_focusBL.gameObject.SetActive(true);
            Player.Instance.m_ui.m_focusC.gameObject.SetActive(true);
            Player.Instance.m_ui.m_focusTL.position = tl;
            Player.Instance.m_ui.m_focusBR.position = br;
            Player.Instance.m_ui.m_focusTR.position = tr;
            Player.Instance.m_ui.m_focusBL.position = bl;
            Player.Instance.m_ui.m_focusC.position = center;
        }
    }

    /// <summary>
    /// Gets the best focusable object in range. <br/>
    /// Priority: <br/>
    /// 1. Raycast hit from screen center <br/>
    /// 2. Closest to screen center (within max angle) <br/>
    /// 3. Closest to focuser (within min angle)
    /// </summary>
    /// <returns></returns>
    private Focusable GetBestFocusable()
    {
        // clear list
        m_nearbyFocusables.Clear();

        // get all 
        foreach (Focusable focusable in Focusable.AllFocusables)
        {
            // skip inactive/disabled
            if (!focusable.isActiveAndEnabled)
            {
                continue;
            }

            // check if focusable is in range
            if (Vector3.Distance(focusable.transform.position, m_camera.transform.position) <= MaxRange(focusable))
            {
                // skip if not focusable
                if (focusable.IsFocusable == false)
                {
                    continue;
                }
                
                m_nearbyFocusables.Add(focusable);
            }
        }

        // Raycast:
        RaycastHit hit;
        LayerMask mask = ~Utils.Layers.PlayerIgnore;
        if (Physics.Raycast(m_camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0)), out hit, MaxRange(), mask))
        {
            if (hit.collider != null)
            {
                Focusable focusable = hit.collider.GetComponentInParent<Focusable>();
                if (focusable != null && m_nearbyFocusables.Contains(focusable))
                {
                    return focusable;
                }
            }
        }

        Focusable bestFocusable = null;

        // check inner angle
        List<Focusable> m_innerFocusables = new List<Focusable>();
        List<Focusable> m_outerFocusables = new List<Focusable>();
        foreach (Focusable focusable in m_nearbyFocusables)
        {
            Vector3 screenPos = m_camera.WorldToScreenPoint(focusable.transform.position);
            float angle = Vector3.Angle(m_camera.transform.forward, focusable.transform.position - m_camera.transform.position);

            if (angle <= m_minFocusAngle)
            {
                m_innerFocusables.Add(focusable);
            }
            else if (angle <= m_maxFocusAngle)
            {
                m_outerFocusables.Add(focusable);
            }
        }
        // if we have any min, check best dist
        if (m_innerFocusables.Count > 0)
        {
            bestFocusable = m_innerFocusables[0];
            float bestDistance = Vector3.Distance(bestFocusable.transform.position, m_camera.transform.position);
            foreach (Focusable focusable in m_innerFocusables)
            {
                float distance = Vector3.Distance(focusable.transform.position, m_camera.transform.position);
                if (distance < bestDistance)
                {
                    bestFocusable = focusable;
                    bestDistance = distance;
                }
            }
        }
        // otherwise check best angle of outer
        else if (m_outerFocusables.Count > 0)
        {
            bestFocusable = m_outerFocusables[0];
            float bestAngle = Vector3.Angle(m_camera.transform.forward, bestFocusable.transform.position - m_camera.transform.position);
            foreach (Focusable focusable in m_outerFocusables)
            {
                float angle = Vector3.Angle(m_camera.transform.forward, focusable.transform.position - m_camera.transform.position);
                if (angle < bestAngle)
                {
                    bestFocusable = focusable;
                    bestAngle = angle;
                }
            }
        }

        return bestFocusable;
    }

    /// <summary>
    /// Gets the biggest range between the focuser and the focusable. <br/>
    /// (or just the focuser's range if focusable is null)
    /// </summary>
    /// <param name="_focusable"></param>
    /// <returns>The maximum range of focus.</returns>
    private float MaxRange(Focusable _focusable = null)
    {
        if (_focusable != null)
        {
            return Mathf.Max(m_focusRange, _focusable.FocusRange);
        }
        else
        {
            return m_focusRange;
        }
    }
}