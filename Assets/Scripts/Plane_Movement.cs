using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Controls the movement of the plane to simulate airdrops.
/// </summary>
public class Plane_Movement : MonoBehaviour
{
    [Header("Plane Parts")]
    public GameObject m_aileronLeft;
    public GameObject m_aileronRight;
    public GameObject m_elevator;
    public GameObject m_rudder;
    public GameObject m_propellerLeft;
    public GameObject m_propellerRight;

    [Header("Audio")]
    public AudioSource m_engineSound;

    [Header("Particles")]
    public ParticleSystem m_engineParticlesLeft;
    public ParticleSystem m_engineParticlesRight;

    [Header("Airdrop Settings")]
    // just for basic straight airdrop movement
    public float m_airSpeed = 185f; // km/h
    public float m_altitude = 200f; // m
    public float m_propSpinSpeed = 360f; // deg/s

    public GameObject m_airdropPrefab;
    [SerializeField] private Vector2 m_targetXZ;
    [SerializeField] private Vector2 m_startXZ;

    private bool m_isFlying = false;
    private bool m_hasDropped = false;

    private void Update()
    {
        // if we are flying
        if (m_isFlying)
        {
            // move the plane forwards
            float metersPerSecond = m_airSpeed * 1000 / 3600; // km/h to m/s
            transform.position += transform.forward * metersPerSecond * Time.deltaTime;

            // if drop location is behind us, drop the airdrop (better than distance check, as drop still occurs if we overshoot)
            Vector2 planeDir = new Vector2(transform.forward.x, transform.forward.z);
            Vector2 toTarget = m_targetXZ - new Vector2(transform.position.x, transform.position.z);
            if (Vector2.Dot(planeDir, toTarget) < 0)
            {
                TryDropAirdrop();
            }
        }
    }

    /// <summary>
    /// Begins the airdrop sequence.
    /// </summary>
    /// <param name="_from">Where should the airdrop start from? XZ</param>
    /// <param name="_target">Where should the airdrop be dropped? XZ</param>
    public void StartAirdrop(Vector2 _from, Vector2 _target)
    {
        m_startXZ = _from;
        m_targetXZ = _target;
        m_isFlying = true;
        m_hasDropped = false;

        StartCoroutine(SpinProps());
        m_engineSound.Play();
        m_engineParticlesLeft.Play();
        m_engineParticlesRight.Play();

        // calc the y position of the airdrop based on map height
        float flightHeight = Terrain.activeTerrain.SampleHeight(new Vector3(_target.x, 0, _target.y));
        flightHeight += m_altitude;

        // set start position and altitude
        transform.position = new Vector3(_from.x, flightHeight, _from.y);

        // set rotation to face the target (ignore y)
        Vector3 targetPos = new Vector3(_target.x, flightHeight, _target.y);
        transform.LookAt(targetPos);
    }

    /// <summary>
    /// Drop the airdrop.
    /// </summary>
    public void TryDropAirdrop()
    {
        if (m_hasDropped)
        {
            return;
        }

        m_hasDropped = true;

        // drop the airdrop
        GameObject airdrop = Instantiate(m_airdropPrefab, transform.position, Quaternion.identity);
        if (airdrop.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            //rb.velocity = transform.forward * m_airSpeed / 3.6f;
        }
    }

    /// <summary>
    /// Stop the airdrop sequence.
    /// </summary>
    public void StopAirdrop()
    {
        m_isFlying = false;
        m_engineSound.Stop();
        StopCoroutine(SpinProps());
        m_engineParticlesLeft.Stop();
        m_engineParticlesRight.Stop();
    }

    /// <summary>
    /// Spin props while flying.
    /// </summary>
    /// <returns></returns>
    IEnumerator SpinProps()
    {
        while (m_isFlying)
        {
            m_propellerLeft.transform.Rotate(Vector3.up, m_propSpinSpeed * Time.deltaTime);
            m_propellerRight.transform.Rotate(Vector3.up, -m_propSpinSpeed * Time.deltaTime);
            yield return null;
        }
    }

    public void DebugAirdrop()
    {
        StartAirdrop(
            _from: new Vector2(transform.position.x, transform.position.z),
            _target: new Vector2(Player.Instance.transform.position.x, Player.Instance.transform.position.z)
        );
    }

    // Custom Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(Plane_Movement))]
    public class Plane_Movement_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Plane_Movement plane = (Plane_Movement)target;

            if (GUILayout.Button("Start Airdrop"))
            {
                plane.DebugAirdrop();
            }

            if (GUILayout.Button("Stop Airdrop"))
            {
                plane.StopAirdrop();
            }
        }
    }
#endif
}
