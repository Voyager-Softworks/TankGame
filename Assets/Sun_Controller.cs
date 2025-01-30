using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the day/night cycle of the sun
/// </summary>
public class Sun_Controller : MonoBehaviour
{
    private Light m_sunLight;
    public Light SunLight { get { return m_sunLight; } }

    [SerializeField] private float m_sunAngle = 100.0f;

    [Header("Time Settings (in hours)")]
    [SerializeField] private float m_sunriseTime = 6.0f;
    [SerializeField] private float m_sunsetTime = 18.0f;
    [SerializeField] private float m_currentTime = 6.0f;
    [SerializeField] private float m_timeMulti = 48.0f;

    [Header("Light Settings")]
    [SerializeField] private float m_midDayIntensity = 1.0f;
    [SerializeField] private float m_sunsetIntensity = 0.0f;

    private void Awake()
    {
        m_sunLight = GetComponent<Light>();
    }

    private void Update()
    {
        UpdateTime();
        UpdateSun();
    }

    private void UpdateTime()
    {
        // get delta time in hours
        float delta = (Time.deltaTime / 3600.0f) * m_timeMulti;
        m_currentTime += delta;
        if (m_currentTime >= 24.0f)
        {
            m_currentTime = 0.0f;
        }
        if (m_currentTime < 0.0f)
        {
            m_currentTime = 24.0f;
        }
    }

    private void UpdateSun()
    {
        // during day:
        if (m_currentTime >= m_sunriseTime && m_currentTime < m_sunsetTime)
        {
            // Sun Rotation: rotate from 0 to 180 degrees
            float dayLerp = (m_currentTime - m_sunriseTime) / (m_sunsetTime - m_sunriseTime);
            float angle = Mathf.Lerp(0.0f, 180.0f, dayLerp);
            transform.rotation = Quaternion.Euler(angle, m_sunAngle, 0.0f);

            // Sun Intensity:
            // map the current time to the intensity value, sunrise = 0, 12 = 1, sunset = 0
            float intensityLerp = Mathf.Abs(12.0f - m_currentTime) / 6.0f;
            m_sunLight.intensity = Mathf.Lerp(m_midDayIntensity, m_sunsetIntensity, intensityLerp);
        }
        // during night:
        else
        {
            // Sun Rotation: rotate from 180 to 360 degrees
            float timeToSunrise = (m_currentTime < m_sunriseTime) ? m_sunriseTime - m_currentTime : 24.0f - m_currentTime + m_sunriseTime;
            float rotLerp = timeToSunrise / (24.0f - m_sunsetTime + m_sunriseTime);
            float angle = Mathf.Lerp(180.0f, 360.0f, rotLerp);
            transform.rotation = Quaternion.Euler(angle, m_sunAngle, 0.0f);

            // Sun Intensity:
            // lerp to 0, to prevent sudden change
            m_sunLight.intensity = Mathf.Lerp(m_sunLight.intensity, 0.0f, Time.deltaTime);
            if (m_sunLight.intensity < 0.01f)
            {
                m_sunLight.intensity = 0.0f;
            }
        }
    }
}
