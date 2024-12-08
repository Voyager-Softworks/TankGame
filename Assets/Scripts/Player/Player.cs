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
    private static Player s_instance = null;
    public static Player Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = FindObjectOfType<Player>();
            }
            return s_instance;
        }
        private set
        {
            s_instance = value;
        }
    }

    [Header("References")]
    public Player_Movement m_movement;
    public Player_Gun m_gun;

    public PlayerUI m_ui;
    public Collider m_collider;
    public GameObject m_model;

    public GameObject m_clipDisplayParent;
    public GameObject m_clipDisplayTemplate;
    public Animator deathAnimator;

    public List<GameObject> m_DEBUGCorners = new List<GameObject>();

    private void Awake()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }
        else if (s_instance != this)
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

    /// <summary>
    ///  Deactivates Movement & Weapon, activates death animation.
    /// </summary>
    public void PlayerDeath()
    {
        deathAnimator.enabled = true;
    }
}