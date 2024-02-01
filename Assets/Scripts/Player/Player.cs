using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The owner script of the player. Manages the state of the player.
/// <br/>This player is currently just for outside the tank.
/// </summary>
public class Player : MonoBehaviour
{
    // singleton pattern
    public static Player Instance { get; private set; }

    [Header("References")]
    public Player_Movement m_movement;
    public Collider m_collider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (m_movement == null)
        {
            Debug.LogError("Player.Awake | Movement not assigned!");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Enables the player.
    /// </summary>
    public void EnablePlayer()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Disables the player.
    /// </summary>
    public void DisablePlayer()
    {
        gameObject.SetActive(false);
    }
}