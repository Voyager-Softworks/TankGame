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
    private Vector3 m_originalCamPos;
    private Vector3 m_originalPlayerPos;

    // Start is called before the first frame update
    void Start()
    {
        // null checks
        if (Player.Instance == null)
        {
            Debug.LogError("Player_Gun.Awake | Player instance not found!");
            return;
        }
        Player_Movement movement = Player.Instance.m_movement;

        // set original positions
        m_originalCamPos = transform.position - movement.m_cam.transform.position;
        m_originalPlayerPos = transform.position - Player.Instance.transform.position;
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
            // parent to camera if not already
            if (transform.parent != Player.Instance.m_movement.m_cam.transform)
            {
                transform.SetParent(Player.Instance.m_movement.m_cam.transform);
                // reset position and rotation
                transform.localPosition = m_originalCamPos;
                transform.localRotation = Quaternion.identity;
            }
            return;
        }
        else
        {
            // parent to player if not already
            if (transform.parent != Player.Instance.transform)
            {
                transform.SetParent(Player.Instance.transform);
                // reset position and rotation
                transform.localPosition = m_originalPlayerPos;
                transform.localRotation = Quaternion.identity;
            }
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
