using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The gun of the player.
/// </summary>
public class Player_Gun : MonoBehaviour
{
    [Header("References")]
    public Transform m_shellPoint;
    public Transform m_firePoint;

    [Header("Settings")]
    public bool m_followCam = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // null checks
        if (Player.Instance == null)
        {
            return;
        }

        FollowCam();
    }

    /// <summary>
    /// Looks at where the player cam is looking.
    /// </summary>
    private void FollowCam()
    {
        if (!m_followCam)
        {
            return;
        }

        // null checks
        Player_Movement movement = Player.Instance.m_movement;
        if (movement == null)
        {
            return;
        }

        // look at where camera is looking (raycast to hit point)
        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;
        LayerMask ignoreMask = LayerMask.GetMask("Player");
        if (Physics.Raycast(movement.m_cam.transform.position, movement.m_cam.transform.forward, out hit, 100f, ~ignoreMask))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = movement.m_cam.transform.position + movement.m_cam.transform.forward * 100f;
        }

        // move gun to look at hit point
        transform.LookAt(hitPoint);
    }
}
