using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Player_Movement : MonoBehaviour
{
    [Header("References")]
    public Camera m_cam;
    public CharacterController m_controller;
    public Transform m_groundCheck;
    public LayerMask m_groundMask;

    [Header("Ground Check")]
    public float m_groundDistance = 0.4f;
    public bool m_isGrounded;

    [Header("Movement")]
    public float m_moveSpeed = 10f;
    public float m_jumpForce = 10f;

    [Header("Sprint")]
    public float m_sprintSpeedMultiplier = 1.5f;

    [Header("Crouch")]
    public float m_crouchSpeedMulti = 0.75f;
    public float m_crouchHeightMulti = 0.5f;
    public float m_crouchDownTime = 0.25f;
    public float m_crouchUpTime = 0.5f;
    public float m_currentHeight = 1f;
    public float m_normalHeight = 1f;

    [Header("Look")]
    public float m_mouseSensitivity = 1f;
    float m_camY = 0;

    [Header("Steps")]
    [SerializeField] float stepHeight = 0.05f;
    [SerializeField] float stepLength = 1.0f;
    [SerializeField] float distanceTraveled = 0;
    public UnityEvent Step;
    bool oneStep = true;

    [Header("Gravity")]
    public float m_gravity = -9.81f;
    public float m_fallMultiplier = 2.5f;
    public float m_lowJumpMultiplier = 2f;

    Vector3 m_velocity;
    Vector2 m_inputDir;

    private void Awake()
    {
        if (m_cam == null)
        {
            Debug.LogError("Player_Movement.Awake | Camera not assigned!");
        }

        // lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        m_camY = m_cam.transform.localPosition.y;
    }

    private void Update()
    {
        // mouse look
        Vector2 mouseDelta = InputManager.PlayerLook.Move.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * mouseDelta.x * m_mouseSensitivity);
        m_cam.transform.Rotate(Vector3.left * mouseDelta.y * m_mouseSensitivity);

        // Ground Check
        m_isGrounded = Physics.CheckSphere(m_groundCheck.position, m_groundDistance, m_groundMask);

        // Movement (relative to camera)
        float x = InputManager.PlayerMove.Move.ReadValue<Vector2>().x;
        float z = InputManager.PlayerMove.Move.ReadValue<Vector2>().y;
        Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;

        // Sprint & Crouch
        if (InputManager.PlayerMove.Sprint.IsPressed())
        {
            moveDir *= m_sprintSpeedMultiplier;
        }
        else if (InputManager.PlayerMove.Crouch.IsPressed())
        {
            moveDir *= m_crouchSpeedMulti;

            // crouch down
            float currentHeight = transform.localScale.y;
            if (currentHeight > m_normalHeight * m_crouchHeightMulti)
            {
                // subtract from current height (after m_crouchDownTime seconds, we should be at m_normalHeight * m_crouchHeightMulti)
                float toSubtract = (m_normalHeight - (m_normalHeight * m_crouchHeightMulti)) / m_crouchDownTime * Time.deltaTime;
                m_currentHeight -= toSubtract;

                // move player down
                m_controller.Move(Vector3.down * toSubtract * 2f);
            }
        }
        else
        {
            // crouch up
            float currentHeight = transform.localScale.y;
            if (currentHeight < m_normalHeight)
            {
                // add to current height (after m_crouchUpTime seconds, we should be at m_normalHeight)
                float toAdd = (m_normalHeight - currentHeight) / m_crouchUpTime * Time.deltaTime;
                m_currentHeight += toAdd;

                // move player up
                m_controller.Move(Vector3.up * toAdd * 2f);
            }
        }

        // Apply Height
        Vector3 currentScale = transform.localScale;
        currentScale.y = m_currentHeight;
        transform.localScale = currentScale;

        Vector3 moveVector = moveDir * m_moveSpeed * Time.deltaTime;
        m_controller.Move(moveVector);

        // Jump
        if (InputManager.PlayerMove.Jump.WasPerformedThisFrame() && m_isGrounded)
        {
            m_velocity.y = Mathf.Sqrt(m_jumpForce * -2f * m_gravity);
        }

        // Gravity
        m_velocity.y += m_gravity * Time.deltaTime;

        // Apply Gravity
        m_controller.Move(m_velocity * Time.deltaTime);

        // Apply Gravity Multipliers
        if (m_velocity.y < 0)
        {
            m_velocity += Vector3.up * m_gravity * (m_fallMultiplier - 1) * Time.deltaTime;
        }
        else if (m_velocity.y > 0 && !InputManager.PlayerMove.Jump.IsPressed())
        {
            m_velocity += Vector3.up * m_gravity * (m_lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // Steps
        if (m_isGrounded)
        {
            distanceTraveled += new Vector3(moveVector.x, 0, moveVector.z).magnitude;
        }
        m_cam.transform.localPosition =
            (transform.up * m_camY * (1.0f - Mathf.Sin(distanceTraveled * (1.0f / stepLength))) * stepHeight)
            + (transform.up * m_camY)
            + (transform.right * (1.0f - Mathf.Sin(distanceTraveled * (1.0f / stepLength) / 2)) * stepHeight * 0.4f);

        if ((1.0f - Mathf.Sin(distanceTraveled * (1.0f / stepLength))) <= 0.1f)
        {
            if (oneStep)
            {
                oneStep = false;
                // footstepNoise.m_noise = (isSneaking ? sneakFootstep : normalFootstep);
                // footstepNoise.PlayNoise();
                Step.Invoke();
            }
        }
        else
        {
            oneStep = true;
        }

        // Debug
        Debug.DrawRay(transform.position, transform.forward * 2f, Color.red);
        Debug.DrawRay(transform.position, moveDir * 2f, Color.green);
    }
}