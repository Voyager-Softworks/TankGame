using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public Player_Gun m_gun;
    public Collider m_collider;
    public GameObject m_model;

    public RectTransform DEBUG_interactShower;

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
    private void Update()
    {
        // f5 reloads the scene
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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