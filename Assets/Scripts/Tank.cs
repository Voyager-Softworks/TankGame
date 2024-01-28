using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// The owner script of the tank. Manages the state of the tank.
/// </summary>
public class Tank : MonoBehaviour
{
    // singleton pattern
    public static Tank Instance { get; private set; }

    [Header("References")]
    public Tank_Movement m_movement;
    public Interactable_Tank m_interactable;
    public Camera m_camera;
    public Transform m_playerExitPoint;
    public Collider m_fullCollider;

    [Header("State")]
    [Utils.ReadOnly] public bool m_isPlayerInside = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Tank.Awake | Multiple instances of Tank!");
            Destroy(gameObject);
            return;
        }

        if (m_movement == null)
        {
            Debug.LogError("Tank.Awake | Movement not assigned!");
        }
        if (m_interactable == null)
        {
            Debug.LogError("Tank.Awake | Interactable not assigned!");
        }
    }

    private void Start() {
        m_movement.UpdateAudio();
        m_movement.UpdateWheels();
        
        DisableTank();
    }

    private void Update()
    {
        if (m_isPlayerInside)
        {
            // check if player is trying to exit [O Key]
            if (InputManager.TankSpecial.Exit.triggered)
            {
                OnPlayerExit();
                Player.Instance.EnablePlayer();

                // move player to exit point (world space)
                Player.Instance.m_movement.m_controller.enabled = false;
                Player.Instance.transform.position = m_playerExitPoint.position;
                Player.Instance.m_movement.m_controller.enabled = true;
                // look same direction as tank (but not up/down)
                Player.Instance.transform.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, 0);

            }
        }
    }

    /// <summary>
    /// Called when the player enters the tank.
    /// </summary>
    public void OnPlayerEnter()
    {
        m_isPlayerInside = true;
        EnableTank();
    }

    /// <summary>
    /// Called when the player exits the tank.
    /// </summary>
    public void OnPlayerExit()
    {
        m_isPlayerInside = false;
        DisableTank();
    }

    /// <summary>
    /// Enables scripts, and makes it moveable.
    /// </summary>
    private void EnableTank()
    {
        // enable tank movement
        m_movement.enabled = true;
        //m_interactable.enabled = false;  @Keane : removed for the mean time so the player can hope in and out of the tank :)
        m_camera.enabled = true;

        // wheels on
        EnableWheelColliders();

        // turn off full collider
        m_fullCollider.enabled = false;
    }

    /// <summary>
    /// Disables scripts, and makes it immovable.
    /// </summary>
    private void DisableTank()
    {
        // disable tank movement
        m_movement.enabled = false;
        //m_interactable.enabled = true;
        m_camera.enabled = false;

        // wheels off
        DisableWheelColliders();

        // turn on full collider
        m_fullCollider.enabled = true;
    }

    /// <summary>
    /// Enables all wheel colliders.
    /// </summary>
    private void EnableWheelColliders()
    {
        foreach (WheelCollider wheel in m_movement.m_leftWheels)
        {
            wheel.enabled = true;
        }
        foreach (WheelCollider wheel in m_movement.m_rightWheels)
        {
            wheel.enabled = true;
        }
    }

    /// <summary>
    /// Disables all wheel colliders.
    /// </summary>
    private void DisableWheelColliders()
    {
        foreach (WheelCollider wheel in m_movement.m_leftWheels)
        {
            wheel.enabled = false;
        }
        foreach (WheelCollider wheel in m_movement.m_rightWheels)
        {
            wheel.enabled = false;
        }
    }
}