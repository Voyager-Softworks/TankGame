using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic movement for the rabbit
/// </summary>
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

    private Vector3 m_lastPosition = Vector3.zero;
    private float m_distanceTraveled = 0.0f;

    [Header("Footprint Settings")]
    [SerializeField] private GameObject m_frontLeftFootprintDecal = null;
    [SerializeField] private GameObject m_backLeftFootprintDecal = null;

    [SerializeField] private int m_maxFootprints = 10;

    [SerializeField] private float m_feedWidth = 0.5f;
    [SerializeField] private float m_footprintDistance = 0.5f;
    [SerializeField] private float m_lastFootprintDistance = 0.0f;
    private int m_footprintIndex = 0;

    private List<GameObject> m_spawnedFootprints = new List<GameObject>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, m_moveDirection);
    }

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_health = GetComponent<Health_Animal>();

        m_jumpDelay = Random.Range(m_minJumpDelay, m_maxJumpDelay);

        m_moveDirection = transform.forward;

        m_lastPosition = transform.position;
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

        Footprints();
    }

    /// <summary>
    /// Make the rabbit jump
    /// </summary>
    private void Jump()
    {
        m_rigidbody.AddForce(Vector3.up * 5.0f, ForceMode.Impulse);
    }

    /// <summary>
    /// Move the rabbit in a random direction
    /// </summary>
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

        // Update distance traveled (ignore y axis)
        m_distanceTraveled += Vector3.Distance(new Vector3(transform.position.x, 0.0f, transform.position.z), new Vector3(m_lastPosition.x, 0.0f, m_lastPosition.z));
        m_lastPosition = transform.position;
    }

    /// <summary>
    /// Create footprints for the rabbit based on distance traveled
    /// </summary>
    private void Footprints()
    {
        if (m_distanceTraveled - m_lastFootprintDistance >= m_footprintDistance)
        {
            m_lastFootprintDistance = m_distanceTraveled;

            // rabbits do front, then front, then both back

            // raycast down to find ground
            RaycastHit groundHit = new RaycastHit();
            if (Physics.Raycast(transform.position, Vector3.down, out groundHit, 1.0f))
            {
                // index 0 = front left, index 1 = front right, index 2 = back left AND back right
                if (m_footprintIndex == 0)
                {
                    GameObject fontLeftPrint = Instantiate(m_frontLeftFootprintDecal, groundHit.point, transform.rotation);
                    // move slightly to the left
                    fontLeftPrint.transform.position += -transform.right * m_feedWidth;

                    // add to spawned footprints
                    m_spawnedFootprints.Add(fontLeftPrint);
                }
                else if (m_footprintIndex == 1)
                {
                    GameObject frontRightPrint = Instantiate(m_frontLeftFootprintDecal, groundHit.point, transform.rotation);
                    // move slightly to the right
                    frontRightPrint.transform.position += transform.right * m_feedWidth;

                    // add to spawned footprints
                    m_spawnedFootprints.Add(frontRightPrint);
                }
                else if (m_footprintIndex == 2)
                {
                    GameObject backLeftPrint = Instantiate(m_backLeftFootprintDecal, groundHit.point, transform.rotation);
                    GameObject backRightPrint = Instantiate(m_backLeftFootprintDecal, groundHit.point, transform.rotation);

                    // move slightly to the left
                    backLeftPrint.transform.position += -transform.right * m_feedWidth;
                    // move slightly to the right
                    backRightPrint.transform.position += transform.right * m_feedWidth;

                    // add to spawned footprints
                    m_spawnedFootprints.Add(backLeftPrint);
                    m_spawnedFootprints.Add(backRightPrint);
                }

                // increment index
                m_footprintIndex++;
                if (m_footprintIndex > 2)
                {
                    m_footprintIndex = 0;
                }

                // remove old footprints
                while (m_spawnedFootprints.Count > m_maxFootprints)
                {
                    Destroy(m_spawnedFootprints[0]);
                    m_spawnedFootprints.RemoveAt(0);
                }
            }
        }
    }
}
