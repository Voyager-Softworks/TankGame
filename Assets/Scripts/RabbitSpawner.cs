using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitSpawner : MonoBehaviour
{
    public GameObject m_rabbitPrefab;
    public int m_maxRabbitCount = 10;
    public float m_spawnRadius = 50.0f;
    public float m_spawnDelayMin = 2.0f;
    public float m_spawnDelayMax = 10.0f;
    public float m_spawnDelay = 0.0f;
    public float m_deadDestroyDelay = 10.0f;

    [SerializeField] private List<GameObject> m_rabbits = new List<GameObject>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_spawnRadius);
    }

    private void Awake()
    {
        m_spawnDelay = Random.Range(m_spawnDelayMin, m_spawnDelayMax);
    }

    private void Update()
    {
        m_spawnDelay -= Time.deltaTime;
        if (m_spawnDelay <= 0.0f)
        {
            SpawnRabbit();
            m_spawnDelay = Random.Range(m_spawnDelayMin, m_spawnDelayMax);
        }
    }

    private void SpawnRabbit()
    {
        if (m_rabbits.Count >= m_maxRabbitCount)
        {
            return;
        }

        // get random spawn position within radius
        Vector3 spawnPos = transform.position + Random.insideUnitSphere * m_spawnRadius;
        spawnPos.y = 100.0f;
        // raycast down to find ground
        RaycastHit hit;
        if (Physics.Raycast(spawnPos, Vector3.down, out hit, 200.0f))
        {
            spawnPos = hit.point;
        }
        GameObject rabbit = Instantiate(m_rabbitPrefab, spawnPos, Quaternion.identity);
        m_rabbits.Add(rabbit);

        Health_Animal health = rabbit.GetComponent<Health_Animal>();
        if (health != null)
        {
            health.OnDeath += (_) => RemoveRabbit(health);
        }
    }

    /// <summary>
    /// Remove a rabbit from the list and destroy it after a delay.
    /// </summary>
    /// <param name="_health"></param>
    private void RemoveRabbit(Health_Animal _health)
    {
        GameObject rabbit = _health.gameObject;
        m_rabbits.Remove(rabbit);
        Destroy(rabbit, m_deadDestroyDelay);
    }
}
