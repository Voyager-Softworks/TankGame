using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rabbit_Movement : MonoBehaviour
{
    private Rigidbody m_rigidbody;
    private Health_Animal m_health;

    [Header("Jump Settings")]
    [SerializeField] private float m_minJumpDelay = 0.5f;
    [SerializeField] private float m_maxJumpDelay = 1.5f;
    [SerializeField] private float m_jumpDelay = 0.0f;

    [Header("Movement Settings")]
    [SerializeField] private float m_moveSpeed = 1.0f;
    [SerializeField] private Vector3 m_moveDirection = Vector3.zero;
    [SerializeField] private float m_rotateSpeed = 0.1f;
    [SerializeField] private float m_dirRateOfChange = 2f;
    [SerializeField] private float m_currentDirChange = 0.0f;

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, m_moveDirection);
    }

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_health = GetComponent<Health_Animal>();

        m_jumpDelay = Random.Range(m_minJumpDelay, m_maxJumpDelay);

        m_moveDirection = transform.forward;
    }

    private void Update()
    {
        if (m_health.IsDead)
        {
            return;
        }

        m_jumpDelay -= Time.deltaTime;
        if (m_jumpDelay <= 0.0f)
        {
            Jump();
            m_jumpDelay = Random.Range(m_minJumpDelay, m_maxJumpDelay);
        }

        Move();
    }

    private void Jump()
    {
        m_rigidbody.AddForce(Vector3.up * 5.0f, ForceMode.Impulse);
    }

    private void Move()
    {
        // Rotate the rabbit
        Vector3 targetDirection = m_moveDirection;
        targetDirection.y = 0.0f;
        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            m_rigidbody.MoveRotation(Quaternion.Slerp(m_rigidbody.rotation, targetRotation, m_rotateSpeed));
        }

        // Update current direction change
        m_currentDirChange += Time.deltaTime * (Random.Range(-m_dirRateOfChange, m_dirRateOfChange));
        m_currentDirChange = Mathf.Clamp(m_currentDirChange, -5.0f, 5.0f);
        // add dir change to move direction
        m_moveDirection = Quaternion.Euler(0.0f, m_currentDirChange, 0.0f) * transform.forward;

        // Move the rabbit
        m_rigidbody.MovePosition(m_rigidbody.position + transform.forward * m_moveSpeed * Time.deltaTime);
    }
}
