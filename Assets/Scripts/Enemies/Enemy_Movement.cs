using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Enemy_Movement : MonoBehaviour
{
    public NavMeshAgent m_navMeshAgent;
    [Utils.ReadOnly] public Transform m_target;
    [Utils.ReadOnly] public bool m_isRunning = false;
    [Utils.ReadOnly] public Enemy m_Enemy;

    public float m_detectRange = 20.0f;

    private void Awake()
    {
        m_Enemy = GetComponentInParent<Enemy>();
    }

    private void Start()
    {
        m_navMeshAgent = GetComponent<NavMeshAgent>();
        m_target = Player.Instance.m_model.transform;
        AutoSound idle = AudioManager.SpawnSound<AutoSound_FusedIdle>(transform.position);
        idle.transform.SetParent(transform);
    }

    private void Update()
    {
        // if dead, disable navmesh agent
        if (GetComponent<Health>().IsDead)
        {
            m_navMeshAgent.enabled = false;
            m_Enemy.Animator.SetTrigger("Die");
            return;
        }

        if (Vector3.Distance(transform.position, m_target.position) <= m_detectRange)
        {
            if (m_isRunning == false)
            {
                AutoSound scream = AudioManager.SpawnSound<AutoSound_FusedScream>(transform.position);
                scream.transform.SetParent(transform);
            }

            m_isRunning = true;
            m_navMeshAgent.isStopped = false;
            m_navMeshAgent.SetDestination(m_target.position);
        }
        else
        {
            m_navMeshAgent.isStopped = true;
            m_isRunning = false;
        }

        m_Enemy.Animator.SetBool("IsRunning", m_isRunning);

        // if touches player, restart scene
        if (Vector3.Distance(transform.position, m_target.position) <= 1.5f)
        {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Player.Instance.PlayerDeath();
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);

        if (m_navMeshAgent != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, m_navMeshAgent.destination);
        }

        if (m_target != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, m_target.position);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_detectRange);
    }
}
