using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A light that can be interacted with.
/// </summary>
public class Electrical_Light : Electrical
{
    [Header("On/Off")]
    [SerializeField] private float m_onDelay = 0.1f; // how long till it starts turning on
    [SerializeField] private float m_onFlickerTime = 0.5f; // how long it flickers for
    [SerializeField] private float m_onFlickerInterval = 0.1f; // how fast it flickers
    [SerializeField] private float m_onFlickerChance = 0.5f; // chance of flickering
    [SerializeField] private float m_offDelay = 0f; // how long till it starts turning off
    [SerializeField] private float m_onFlickerIntensityMulti = 0.25f; // how bright it flickers

    [Header("Idle Flicker")]
    [SerializeField] private float m_flickerMinDelay = 0.1f; // min delay between flickers
    [SerializeField] private float m_flickerMaxDelay = 1f; // max delay between flickers
    [SerializeField] private float m_flickerDelayChance = 0.1f; // chance of flickering after delay
    [SerializeField] private float m_flickerMinTime = 0.1f; // min flicker time
    [SerializeField] private float m_flickerMaxTime = 0.5f; // max flicker time
    [SerializeField] private float m_flickerMinInterval = 0.1f; // min flicker interval
    [SerializeField] private float m_flickerMaxInterval = 0.5f; // max flicker interval
    [SerializeField] private float m_flickerIntensityMulti = 0.25f; // how bright it flickers

    [Header("References")]
    public Light m_Light = null;
    public Renderer m_lightRenderer = null;

    private float m_initialIntensity = 0f;
    private Color m_initialEmissionColor = Color.white;

    // track coroutines
    private Coroutine m_onCoroutine = null;
    private Coroutine m_offCoroutine = null;
    private Coroutine m_idleFlickerCoroutine = null;

    protected override void Awake()
    {
        base.Awake();

        m_initialIntensity = m_Light.intensity;
        m_initialEmissionColor = m_lightRenderer.material.GetColor("_EmissionColor");
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnPowerOn()
    {
        base.OnPowerOn();

        AudioManager.SpawnSound<AutoSound_GunEmpty>(transform.position); // temporary sound

        // stop coroutines
        if (m_offCoroutine != null)
        {
            StopCoroutine(m_offCoroutine);
            m_offCoroutine = null;
        }
        if (m_idleFlickerCoroutine != null)
        {
            StopCoroutine(m_idleFlickerCoroutine);
            m_idleFlickerCoroutine = null;
        }

        // start the on coroutine
        m_onCoroutine = StartCoroutine(TurnLightOn());
    }

    protected override void OnPowerOff()
    {   
        base.OnPowerOff();

        AudioManager.SpawnSound<AutoSound_GunEmpty>(transform.position); // temporary sound

        // stop coroutines
        if (m_onCoroutine != null)
        {
            StopCoroutine(m_onCoroutine);
            m_onCoroutine = null;
        }
        if (m_idleFlickerCoroutine != null)
        {
            StopCoroutine(m_idleFlickerCoroutine);
            m_idleFlickerCoroutine = null;
        }

        // start the off coroutine
        m_offCoroutine = StartCoroutine(TurnLightOff());
    }

    /// <summary>
    /// Turns on the light and flickers it.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TurnLightOn()
    {
        yield return new WaitForSeconds(m_onDelay);
        // on
        SetLightIntensity(1f);
        bool isOn = true;

        // flicker
        float flickerTime = m_onFlickerTime;
        while (flickerTime > 0)
        {
            if (Random.value < m_onFlickerChance)
            {
                isOn = !isOn;
                StartCoroutine(SetLightIntensityOverTime(isOn ? 1f : m_onFlickerIntensityMulti, m_onFlickerInterval));
            }

            flickerTime -= m_onFlickerInterval;
            yield return new WaitForSeconds(m_onFlickerInterval);
        }

        // on
        SetLightIntensity(1f);

        m_onCoroutine = null;

        // start idle flicker
        m_idleFlickerCoroutine = StartCoroutine(IdleFlicker());
    }

    /// <summary>
    /// Turns off the light.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TurnLightOff()
    {
        yield return new WaitForSeconds(m_offDelay);
        // off
        SetLightIntensity(0f);

        m_offCoroutine = null;
    }

    private IEnumerator IdleFlicker()
    {
        while (true)
        {
            // only when on
            if (!m_isOn)
            {
                yield return null;
                continue;
            }
            // if other coroutine is running
            if (m_onCoroutine != null || m_offCoroutine != null)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(Random.Range(m_flickerMinDelay, m_flickerMaxDelay));

            if (Random.value < m_flickerDelayChance)
            {
                float flickerTime = Random.Range(m_flickerMinTime, m_flickerMaxTime);
                while (flickerTime > 0)
                {
                    float interval = Random.Range(m_flickerMinInterval, m_flickerMaxInterval);
                    
                    StartCoroutine(SetLightIntensityOverTime(m_flickerIntensityMulti, interval));

                    yield return new WaitForSeconds(interval);

                    flickerTime -= Random.Range(m_flickerMinInterval, m_flickerMaxInterval);
                }

                SetLightIntensity(1f);
            }
        }
    }

    private void SetLightIntensity(float _multi)
    {
        m_Light.intensity = m_initialIntensity * _multi;
        m_lightRenderer.material.SetColor("_EmissionColor", m_initialEmissionColor * _multi);
    }

    // set light over time
    private IEnumerator SetLightIntensityOverTime(float _intensity, float _time)
    {
        float time = 0;
        float startIntensityMulti = m_Light.intensity / m_initialIntensity;
        float endIntensityMulti = _intensity / m_initialIntensity;

        while (time < _time)
        {
            time += Time.deltaTime;
            float t = time / _time;

            SetLightIntensity(Mathf.Lerp(startIntensityMulti, endIntensityMulti, t));

            yield return null;
        }
    }
}