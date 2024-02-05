using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the movement of the player.
/// </summary>
public class Player_Movement : MonoBehaviour
{
    [Header("References")]
    public Camera m_cam;
    public CharacterController m_controller;
    public Transform m_groundCheck;
    public LayerMask m_groundMask;

    [Header("Ground Check")]
    public float m_groundDistance = 0.4f;
    public bool m_isGrounded = true;
    [Utils.ReadOnly, SerializeField] private float m_airTime = 0f;
    [Utils.ReadOnly, SerializeField] private float m_groundedTime = 0f;

    [Header("Movement")]
    public float m_moveSpeed = 5f;

    [Header("Sprint")]
    public float m_sprintSpeedMultiplier = 1.5f;
    public float m_sprintUpTime = 0.5f;
    public float m_sprintDownTime = 0.25f;
    [Utils.ReadOnly, SerializeField] private float m_sprintAmount = 0f;
    [Utils.ReadOnly, SerializeField] private bool m_isSprinting = false;

    [Header("Crouch")]
    public float m_crouchSpeedMultiplier = 0.75f;
    public float m_crouchHeightMulti = 0.5f;
    public float m_crouchDownTime = 0.5f;
    public float m_crouchUpTime = 1.0f;
    [Utils.ReadOnly, SerializeField] public float m_currentHeight = 1f;
    [Utils.ReadOnly, SerializeField] private float m_normalHeight = 1f;
    [Utils.ReadOnly, SerializeField] private float m_crouchAmount = 0f;
    [Utils.ReadOnly, SerializeField] private bool m_isCrouching = false;

    [Header("Look")]
    public float m_mouseSensitivity = 0.05f;
    float m_camOrigHeight = 0;

    [Header("Steps")]
    [SerializeField] private float m_stepHeight = 0.02f;
    [SerializeField] private float m_stepLength = 0.5f;
    [Utils.ReadOnly, SerializeField] private float m_distanceTraveled = 0;
    [HideInInspector] public System.Action OnStep;
    private bool m_oneStep = true;

    [Header("Gravity")]
    public float m_gravity = -12f;
    public float m_groundedGravity = -12f;
    public float m_fallMultiplier = 2f;
    public float m_lowJumpMultiplier = 1.5f;
    public float m_jumpForce = 1f;
    public bool m_canJump = false;

    [Header("Velocity")]
    [SerializeField, Utils.ReadOnly] private float m_gravVel;
    [SerializeField, Utils.ReadOnly] private Vector2 m_moveVel;

    [Header("Gun Movement")]
    public bool m_moveGun = true;
    public float m_gunLerpSpeed = 10f;
    public float m_gunMoveAmount = 2f;
    public float m_gunMoveSprintAmount = 4f;
    public float m_airTransitionTime = 0.5f;

    public Vector3 CalcVelocity()
    {
        Vector3 vel = new Vector3
        (
            m_moveVel.x, 
            m_isGrounded && Mathf.Approximately(m_gravVel, m_groundedGravity) ? 0 : m_gravVel, 
            m_moveVel.y
        );
        return vel;
    }

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

        m_camOrigHeight = m_cam.transform.localPosition.y;

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
        // spawn landing sound
        if (m_isGrounded && !wasGrounded)
        {
            AudioManager.SpawnSound<AutoSound_PlayerLand>(transform.position);
        }
        // count air/ground time
        if (m_isGrounded)
        {
            m_airTime = 0f;
            m_groundedTime += Time.deltaTime;
        }
        else
        {
            m_airTime += Time.deltaTime;
            m_groundedTime = 0f;
        }

        // Movement (relative to camera)
        float x = InputManager.PlayerMove.Move.ReadValue<Vector2>().x;
        float z = InputManager.PlayerMove.Move.ReadValue<Vector2>().y;
        Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;

        // Sprint & Crouch
        if (InputManager.PlayerMove.Sprint.IsPressed())
        {
            m_isSprinting = true;
            m_sprintAmount = Mathf.Clamp(m_sprintAmount + Time.deltaTime / m_sprintUpTime, 0, 1);
        }
        else
        {
            m_isSprinting = false;
            m_sprintAmount = Mathf.Clamp(m_sprintAmount - Time.deltaTime / m_sprintDownTime, 0, 1);
        }
        
        if (!m_isSprinting && InputManager.PlayerMove.Crouch.IsPressed())
        {
            m_isCrouching = true;
            m_crouchAmount = Mathf.Clamp(m_crouchAmount + Time.deltaTime / m_crouchDownTime, 0, 1);
        }
        else
        {
            m_isCrouching = false;
            m_crouchAmount = Mathf.Clamp(m_crouchAmount - Time.deltaTime / m_crouchUpTime, 0, 1);
        }

        float currentHeight = model.transform.localScale.y;
        float targetHeight = Mathf.Lerp(m_normalHeight, m_normalHeight * m_crouchHeightMulti, m_crouchAmount);
        float toChange = (targetHeight - currentHeight);
        if (toChange != 0)
        {
            m_currentHeight += toChange;
            m_controller.Move(Vector3.up * toChange * 2f);
        }
        
        // Apply Speed Multipliers
        moveDir *= Mathf.Lerp(1, m_sprintSpeedMultiplier, m_sprintAmount);
        moveDir *= Mathf.Lerp(1, m_crouchSpeedMultiplier, (m_normalHeight - m_currentHeight) / (m_normalHeight * m_crouchHeightMulti)); 

        // Apply Height
        Vector3 currentScale = model.transform.localScale;
        currentScale.y = Mathf.Clamp(m_currentHeight, m_normalHeight * m_crouchHeightMulti, m_normalHeight);
        // cosmetic model (@TODO: Remove when we have a proper player model)
        model.transform.localScale = currentScale;
        // collider
        m_controller.height = currentScale.y * 2f;
        m_controller.radius = currentScale.x / 2f;

        Vector3 moveVector = moveDir * m_moveSpeed * Time.deltaTime;
        m_moveVel = new Vector2(moveVector.x, moveVector.z) / Time.deltaTime;
        m_controller.Move(moveVector);

        // Jump
        if (m_canJump && InputManager.PlayerMove.Jump.WasPerformedThisFrame() && m_isGrounded)
        {
            m_gravVel = Mathf.Sqrt(m_jumpForce * -2f * m_gravity);

            AudioManager.SpawnSound<AutoSound_PlayerJump>(transform.position);
        }

        // Gravity
        m_gravVel += m_gravity * Time.deltaTime;

        // Apply Gravity Multipliers
        if (m_gravVel < 0)
        {
            m_gravVel += m_gravity * (m_fallMultiplier - 1) * Time.deltaTime;
        }
        else if (m_gravVel > 0 && !InputManager.PlayerMove.Jump.IsPressed())
        {
            m_gravVel += m_gravity * (m_lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // if grounded, reset gravity
        if (m_isGrounded && m_gravVel < 0)
        {
            m_gravVel = m_groundedGravity;
        }

        // Apply Gravity
        m_controller.Move(Vector3.up * m_gravVel * Time.deltaTime);

        // Steps
        if (m_isGrounded)
        {
            m_distanceTraveled += new Vector3(moveVector.x, 0, moveVector.z).magnitude;
        }
        float camScale = m_currentHeight / m_normalHeight;
        m_cam.transform.localPosition =
        (
            // normal height (with crouch)
            (transform.up * m_camOrigHeight * camScale)
            // crouch correction (keep fixed distance from top of head) 
            + (transform.up * (m_normalHeight - m_currentHeight) * m_normalHeight)
            // step vert bobbing
            + (transform.up * m_camOrigHeight * (1.0f - Mathf.Cos(m_distanceTraveled * (1.0f / m_stepLength))) * m_stepHeight)
            // step horiz bobbing
            + (transform.right * (1.0f - Mathf.Cos(m_distanceTraveled * (1.0f / m_stepLength) / 2)) * m_stepHeight * 0.4f)
        );

        // apply steps to the gun too
        if (m_moveGun)
        {
            Player_Gun gun = Player.Instance.m_gun;
            if (gun != null)
            {
                Vector3 origPos = gun.m_originalCamPos;
                Vector3 targetLocalPos = 
                (
                    // normal height (without crouch)
                    (Vector3.up * origPos.y)
                    // step vert bobbing
                    + (Vector3.up * origPos.y * (1.0f - Mathf.Cos(m_distanceTraveled * (1.0f / m_stepLength))) * m_stepHeight * Mathf.Lerp(m_gunMoveAmount, m_gunMoveSprintAmount, m_sprintAmount))
                    // step horiz bobbing
                    + (Vector3.right * (1.0f - Mathf.Cos(m_distanceTraveled * (1.0f / m_stepLength) / 2)) * m_stepHeight * 0.4f * Mathf.Lerp(m_gunMoveAmount, m_gunMoveSprintAmount, m_sprintAmount))
                );

                // if we are grounded, lerp to the target position
                if (m_isGrounded)
                {
                    gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, targetLocalPos, Time.deltaTime * m_gunLerpSpeed);
                }
                // if we are not grounded, lerp to the original position
                else
                {
                    gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, origPos, Time.deltaTime * m_gunLerpSpeed);
                }

                #region Unused Air Transition
                /*Feels less nice, but only lerps when transitioning
                // if we are grounded, move to the target position
                if (m_isGrounded)
                {
                    if (m_groundedTime < m_airTransitionTime)
                    {
                        gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, targetLocalPos, Time.deltaTime * 10f);
                    }
                    else
                    {
                        gun.transform.localPosition = targetLocalPos;
                    }
                }
                // if we are not grounded, move back to the original position
                else if (!m_isGrounded)
                {
                    if (m_airTime < m_airTransitionTime)
                    {
                        gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, origPos, Time.deltaTime * 10f);
                    }
                    else
                    {
                        gun.transform.localPosition = origPos;
                    }
                }
                */
                #endregion
            }
        }

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