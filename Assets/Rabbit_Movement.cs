using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Basic movement for the rabbit
/// </summary>
public class Rabbit_Movement : MonoBehaviour
{
    private Health_Animal m_health;

    [Header("Movement Settings")]
    [SerializeField] private Animator m_animator;
    [SerializeField] private NavMeshAgent m_navMeshAgent;
    [SerializeField] private float m_minSpeed = 0.5f;
    [SerializeField] private float m_maxSpeed = 5.0f;

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

    private void Awake()
    {
        m_health = GetComponent<Health_Animal>();

        m_lastPosition = transform.position;

        m_health.OnDeath += OnDeath;
    }

    private void Update()
    {
        if (m_health.IsDead)
        {
            return;
        }

        Move();

        Footprints();
    }

    /// <summary>
    /// Move the rabbit in a random direction
    /// </summary>
    private void Move()
    {
        // pick a random point to move to, and a random speed
        if (m_navMeshAgent.remainingDistance <= 0.1f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 5.0f;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 5.0f, NavMesh.AllAreas);
            m_navMeshAgent.SetDestination(hit.position);

            m_navMeshAgent.speed = Random.Range(m_minSpeed, m_maxSpeed);

            // set animator speed
            float speed01 = m_navMeshAgent.speed / m_maxSpeed;
            m_animator.SetFloat("Speed", speed01 * 2f);
        }

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

    /// <summary>
    /// Called when the rabbit dies
    /// </summary>
    /// <param name="_lastDamage"></param>
    private void OnDeath(Health.DamageInfo _lastDamage)
    {
        // set dead bool
        m_animator.SetBool("Dead", true);

        // stop moving
        m_navMeshAgent.isStopped = true;

        StartCoroutine(DeathMove(_lastDamage.Direction));
    }

    /// <summary>
    /// Short coroutine to move the rabbit in the direction of death
    /// </summary>
    private IEnumerator DeathMove(Vector3 _direction, float _speed = 5f, float _duration = 0.2f)
    {
        // also rotate so left faces the direction
        Vector3 targetRightDir = -_direction;

        float timer = 0.0f;
        while (timer < _duration)
        {
            timer += Time.deltaTime;

            // move
            transform.position += _direction * _speed * Time.deltaTime;

            // rotate
            transform.right = Vector3.Lerp(transform.right, targetRightDir, Time.deltaTime * 5.0f);

            yield return null;
        }
    }
}
