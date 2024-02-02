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
    [Utils.ReadOnly, SerializeField] public float m_currentHeight = 1f;
    [Utils.ReadOnly, SerializeField] private float m_normalHeight = 1f;

    [Header("Look")]
    public float m_mouseSensitivity = 1f;
    float m_camY = 0;

    [Header("Steps")]
    [SerializeField] float m_stepHeight = 0.02f;
    [SerializeField] float m_stepLength = 0.5f;
    [SerializeField] float m_distanceTraveled = 0;
    [HideInInspector] public UnityEvent OnStep;
    bool m_oneStep = true;

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
        // null checks
        if (Player.Instance == null)
        {
            Debug.LogError("Player_Movement.UpdateMovement | Player.Instance is null!");
            return;
        }
        GameObject model = Player.Instance.m_model;
        if (model == null)
        {
            Debug.LogError("Player_Movement.UpdateMovement | Player.Instance.m_model is null!");
            return;
        }

        m_camY = m_cam.transform.localPosition.y;

        m_normalHeight = model.transform.localScale.y;
        m_currentHeight = m_normalHeight;
    }

    private void Update()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        // null checks
        if (Player.Instance == null)
        {
            Debug.LogError("Player_Movement.UpdateMovement | Player.Instance is null!");
            return;
        }
        GameObject model = Player.Instance.m_model;
        if (model == null)
        {
            Debug.LogError("Player_Movement.UpdateMovement | Player.Instance.m_model is null!");
            return;
        }

        // mouse look
        Vector2 mouseDelta = InputManager.PlayerLook.Move.ReadValue<Vector2>();
        transform.Rotate(Vector3.up * mouseDelta.x * m_mouseSensitivity);

        // limit vertical look to 89 degrees up and down
        Vector3 currentRotation = m_cam.transform.localRotation.eulerAngles;
        currentRotation.z = 0;
        if (currentRotation.x > 180)
        {
            currentRotation.x -= 360;
        }

        if (currentRotation.x > 89)
        {
            // forbid looking down
            if (mouseDelta.y < 0)
            {
                mouseDelta.y = 0;
            }
        }
        else if (currentRotation.x < -89)
        {
            // forbid looking up
            if (mouseDelta.y > 0)
            {
                mouseDelta.y = 0;
            }
        }

        currentRotation.x += -mouseDelta.y * m_mouseSensitivity;
        m_cam.transform.localRotation = Quaternion.Euler(currentRotation);

        // Ground Check
        bool wasGrounded = m_isGrounded;
        m_isGrounded = Physics.CheckSphere(m_groundCheck.position, m_groundDistance, m_groundMask);
        if (m_isGrounded && !wasGrounded)
        {
            AudioManager.SpawnSound<AutoSound_PlayerLand>(transform.position);
        }

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
            float currentHeight = model.transform.localScale.y;
            if (currentHeight > m_normalHeight * m_crouchHeightMulti)
            {
                // subtract from current height (after m_crouchDownTime seconds, we should be at m_normalHeight * m_crouchHeightMulti)
                float toSubtract = m_normalHeight * m_crouchHeightMulti / m_crouchDownTime * Time.deltaTime;
                m_currentHeight -= toSubtract;

                // move player down
                m_controller.Move(Vector3.down * toSubtract * 2f);
            }
        }
        else
        {
            // crouch up
            float currentHeight = model.transform.localScale.y;
            if (currentHeight < m_normalHeight)
            {
                // add to current height (after m_crouchUpTime seconds, we should be at m_normalHeight)
                float toAdd = m_normalHeight / m_crouchUpTime * Time.deltaTime;
                m_currentHeight += toAdd;

                // move player up
                m_controller.Move(Vector3.up * toAdd * 2f);
            }
        }

        // Apply Height
        Vector3 currentScale = model.transform.localScale;
        currentScale.y = Mathf.Clamp(m_currentHeight, m_normalHeight * m_crouchHeightMulti, m_normalHeight);
        // cosmetic model (@TODO: Remove when we have a proper player model)
        model.transform.localScale = currentScale;
        // collider
        m_controller.height = currentScale.y * 2f;
        m_controller.radius = currentScale.x / 2f;

        Vector3 moveVector = moveDir * m_moveSpeed * Time.deltaTime;
        m_controller.Move(moveVector);

        // Jump
        if (InputManager.PlayerMove.Jump.WasPerformedThisFrame() && m_isGrounded)
        {
            m_velocity.y = Mathf.Sqrt(m_jumpForce * -2f * m_gravity);

            AudioManager.SpawnSound<AutoSound_PlayerJump>(transform.position);
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
            m_distanceTraveled += new Vector3(moveVector.x, 0, moveVector.z).magnitude;
        }
        float camScale = m_currentHeight / m_normalHeight;
        m_cam.transform.localPosition =
        (
            // normal height (with crouch)
            (transform.up * m_camY * camScale)
            // crouch correction (keep fixed distance from top of head) 
            + (transform.up * (m_normalHeight - m_currentHeight) * m_normalHeight)
            // step vert bobbing
            + (transform.up * m_camY * (1.0f - Mathf.Cos(m_distanceTraveled * (1.0f / m_stepLength))) * m_stepHeight)
            // step horiz bobbing
            + (transform.right * (1.0f - Mathf.Cos(m_distanceTraveled * (1.0f / m_stepLength) / 2)) * m_stepHeight * 0.4f)
        );

        // Check if we are very close to a step (as we will probably not hit the exact distance traveled to trigger a step)
        if ((1.0f - Mathf.Cos(m_distanceTraveled * (1.0f / m_stepLength))) <= 0.1f)
        {
            // m_oneStep prevents multiple footstep sounds from playing in the same step
            if (m_oneStep)
            {
                m_oneStep = false;
                // footstepNoise.m_noise = (isSneaking ? sneakFootstep : normalFootstep);
                AudioManager.SpawnSound<AutoSound_Footstep>(transform.position);
                OnStep?.Invoke();
            }
        }
        else
        {
            m_oneStep = true;
        }

        // Debug
        Debug.DrawRay(transform.position, transform.forward * 2f, Color.red);
        Debug.DrawRay(transform.position, moveDir * 2f, Color.green);
    }
}