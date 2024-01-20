using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A tank movement script that uses wheel colliders.
/// The t-34-85 is 32 tons, 5.92 long (hull), 3.00m wide, and 2.45m tall.
/// The t34 tank has
/// - Throttle
/// - Brake
/// - Left and right clutches
/// - - Left and right clutches turn into brakes when pulled fully
/// </summary>
public class Tank_Movement : MonoBehaviour {

    [Header("References")]
    public Rigidbody m_rb;
    public List<WheelCollider> m_leftWheels;
    public List<WheelCollider> m_rightWheels;

    [Header("Stats")]
    public float m_mass = 32000f;
    public float m_width = 3f;
    public float m_length = 5.92f;
    public float m_height = 2.45f;

    [Header("Movement")]
    public float m_maxMotorTorque = 2200f;
    public float m_maxSpeed = 55f;
    public float m_maxBrakeTorque = 1000f;

    [Header("State")]
    [Utils.ReadOnly] public float m_throttle = 0f;
    public float m_throttleRate = 1f;
    [Utils.ReadOnly] public float m_brake = 0f;
    public float m_brakeRate = 2f;
    [Utils.ReadOnly] public float m_leftClutch = 0f;
    public float m_leftClutchRate = 1f;
    [Utils.ReadOnly] public float m_rightClutch = 0f;
    public float m_rightClutchRate = 1f;
    [Utils.ReadOnly] public float m_leftBrake = 0f;
    public float m_leftBrakeRate = 1f;
    [Utils.ReadOnly] public float m_rightBrake = 0f;
    public float m_rightBrakeRate = 1f;

    [Header("Debug")]
    [Utils.ReadOnly] public float m_currentThrottleTorque = 0f;
    [Utils.ReadOnly] public float m_currentBrakeTorque = 0f;
    [Utils.ReadOnly] public float m_currentLeftMotorTorque = 0f;
    [Utils.ReadOnly] public float m_currentRightMotorTorque = 0f;
    [Utils.ReadOnly] public float m_currentLeftBrake = 0f;
    [Utils.ReadOnly] public float m_currentRightBrake = 0f;
    [Utils.ReadOnly] public Vector3 m_currentVelocity = Vector3.zero;

    private void Start() {
        if (m_rb == null) {
            Debug.LogError("Tank_Movement.Start | Rigidbody not assigned!");
        }

        m_rb.mass = m_mass;
    }

    private void Update() {
        // throttle (increase and decrease)
        float throttleInput = InputManager.TankDrive.Throttle.ReadValue<float>();
        if (throttleInput > 0f) {
            m_throttle = Mathf.Clamp(m_throttle + throttleInput * Time.deltaTime * m_throttleRate, 0f, 1f);
        } else if (throttleInput < 0f) {
            m_throttle = Mathf.Clamp(m_throttle + throttleInput * Time.deltaTime * m_throttleRate, 0f, 1f);
        }

        // brake (increase)
        float brakeInput = InputManager.TankDrive.Brake.ReadValue<float>();
        if (InputManager.TankDrive.Brake.ReadValue<float>() > 0f) {
            m_brake = Mathf.Clamp(m_brake + brakeInput * Time.deltaTime * m_brakeRate, 0f, 1f);
        } else {
            m_brake = Mathf.Clamp(m_brake - Time.deltaTime * m_brakeRate, 0f, 1f);
        }

        // clutch left (increase and decrease)
        float clutchLeftInput = InputManager.TankDrive.ClutchLeft.ReadValue<float>();
        if (clutchLeftInput > 0f) {
            m_leftClutch = Mathf.Clamp(m_leftClutch + clutchLeftInput * Time.deltaTime * m_leftClutchRate, 0f, 1f);
        } else if (clutchLeftInput < 0f) {
            m_leftClutch = Mathf.Clamp(m_leftClutch + clutchLeftInput * Time.deltaTime * m_leftClutchRate, 0f, 1f);
        }

        // if is fully pulled, turn into brake
        if (m_leftClutch == 1f && clutchLeftInput > 0f) {
            m_leftBrake = Mathf.Clamp(m_leftBrake + clutchLeftInput * Time.deltaTime * m_leftBrakeRate, 0f, 1f);
        }
        else
        {
            m_leftBrake = Mathf.Clamp(m_leftBrake - Time.deltaTime * m_leftBrakeRate * 2f, 0f, 1f);
        }

        // clutch right (increase and decrease)
        float clutchRightInput = InputManager.TankDrive.ClutchRight.ReadValue<float>();
        if (clutchRightInput > 0f) {
            m_rightClutch = Mathf.Clamp(m_rightClutch + clutchRightInput * Time.deltaTime * m_rightClutchRate, 0f, 1f);
        } else if (clutchRightInput < 0f) {
            m_rightClutch = Mathf.Clamp(m_rightClutch + clutchRightInput * Time.deltaTime * m_rightClutchRate, 0f, 1f);
        }

        // if is fully pulled, turn into brake
        if (m_rightClutch == 1f && clutchRightInput > 0f) {
            m_rightBrake = Mathf.Clamp(m_rightBrake + clutchRightInput * Time.deltaTime * m_rightBrakeRate, 0f, 1f);
        }
        else
        {
            m_rightBrake = Mathf.Clamp(m_rightBrake - Time.deltaTime * m_rightBrakeRate * 2f, 0f, 1f);
        }
    }

    private void FixedUpdate() {
        // calculate motor torque
        m_currentThrottleTorque = m_maxMotorTorque * m_throttle;

        // calculate brake torque
        m_currentBrakeTorque = m_maxBrakeTorque * m_brake;

        // calculate left and right motor torque
        m_currentLeftMotorTorque = m_currentThrottleTorque * (1f - m_leftClutch);
        m_currentRightMotorTorque = m_currentThrottleTorque * (1f - m_rightClutch);

        // calculate left and right brake torque
        m_currentLeftBrake = m_maxBrakeTorque * m_leftBrake;
        m_currentRightBrake = m_maxBrakeTorque * m_rightBrake;

        // apply left and right motor torque
        foreach (WheelCollider wheel in m_leftWheels) {
            wheel.motorTorque = m_currentLeftMotorTorque;
        }
        foreach (WheelCollider wheel in m_rightWheels) {
            wheel.motorTorque = m_currentRightMotorTorque;
        }

        // apply left and right brake torque
        foreach (WheelCollider wheel in m_leftWheels) {
            wheel.brakeTorque = Mathf.Max(m_currentLeftBrake, m_currentBrakeTorque);
        }
        foreach (WheelCollider wheel in m_rightWheels) {
            wheel.brakeTorque = Mathf.Max(m_currentRightBrake, m_currentBrakeTorque);
        }

        // calculate speed
        m_currentVelocity = m_rb.velocity;

        // clamp speed
        if (m_currentVelocity.magnitude > m_maxSpeed) {
            m_currentVelocity = m_currentVelocity.normalized * m_maxSpeed;
            m_rb.velocity = m_currentVelocity;
        }
    }
}