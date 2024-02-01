using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif

/// <summary>
/// A 3D zone that can be used to trigger and control <see cref="AutoSound"/>(s).
/// <br/>Uses the current audio listener position.
/// </summary>
public class AutoSoundZone : MonoBehaviour
{
    /// <summary>
    /// The type of shape a zone can be
    /// </summary>
    public enum ZoneShape
    {
        Box,
        Sphere
    }

    /// <summary>
    /// A link to an <see cref="AutoSound"/> that can be used to control it as part of the zone.
    /// </summary>
    [System.Serializable]
    public class AutoSoundLink
    {
        [SerializeField, Utils.ReadOnly] protected string m_name = "";
        public AutoSound m_autoSound;
        public bool m_unpauseOnEnter = true;
        public bool m_playNewOnEnter = false;
        public bool m_pauseOnExit = true;
        public float m_fadeInTime = 1f;
        public float m_fadeOutTime = 1f;

        // constructor
        public AutoSoundLink(AutoSound _autoSound)
        {
            m_autoSound = _autoSound;
            m_unpauseOnEnter = true;
            m_playNewOnEnter = false;
            m_pauseOnExit = true;
            m_fadeInTime = 1f;
            m_fadeOutTime = 1f;

            Validate();
        }

        /// <summary>
        /// Validate the <see cref="AutoSound"/> reference and updates the name.
        /// </summary>
        /// <returns>If any values were changed.</returns>
        public bool Validate()
        {
            bool didChange = false;
            if (m_autoSound == null)
            {
                string emptyName = "!EMPTY!";
                if (m_name != emptyName)
                {
                    m_name = emptyName;
                    didChange = true;
                }
            }
            else
            {
                if (m_name != m_autoSound.name)
                {
                    m_name = m_autoSound.name;
                    didChange = true;
                }
            }

            return didChange;
        }
    }

    public static List<AutoSoundZone> AllAutoSoundZones = new List<AutoSoundZone>();

    [SerializeField] private ZoneShape m_zoneShape = ZoneShape.Box;

    [Header("Box")]
    [SerializeField] private Vector3 m_size = Vector3.one;

    [Header("Sphere")]
    [SerializeField] private float m_radius = 1f;

    [Header("Settings")]
    public int m_priority = 0;
    public List<AutoSoundLink> m_linkedAutoSounds = new List<AutoSoundLink>();
    [SerializeField, Utils.ReadOnly] private bool m_isInZone = false;
    [SerializeField, Utils.ReadOnly] private float m_currentZoneVolumeRatio = 1f;
    public float m_fadeTime = 1f;

    private void Awake()
    {
        // add to list
        if (!AllAutoSoundZones.Contains(this))
        {
            AllAutoSoundZones.Add(this);
        }
    }

    private void OnDestroy()
    {
        // remove from list
        if (AllAutoSoundZones.Contains(this))
        {
            AllAutoSoundZones.Remove(this);
        }
    }

    private void Start()
    {
        // set all auto sounds to 0 zone volume initially
        foreach (AutoSoundLink autoSoundLink in m_linkedAutoSounds)
        {
            if (autoSoundLink.m_autoSound != null)
            {
                autoSoundLink.m_autoSound.ZoneVolumeMultiplier = 0f;
            }
        }
    }

    private void Update()
    {
        Vector3 audioListenerPosition = Camera.main?.transform.position ?? Vector3.zero;

        // check if audio listener is in zone
        if (IsInZone(audioListenerPosition))
        {
            // perform desired actions
            if (!m_isInZone)
            {
                EnteredZone();
            }
        }
        else
        {
            // perform desired actions
            if (m_isInZone)
            {
                ExitedZone();
            }
        }
    }

    /// <summary>
    /// Called when the audio listener enters the zone.
    /// </summary>
    private void EnteredZone()
    {
        m_isInZone = true;

        UpdateAllZoneRatios();

        // enable auto sounds (if set to)
        foreach (AutoSoundLink autoSoundLink in m_linkedAutoSounds)
        {
            if (autoSoundLink.m_autoSound != null && autoSoundLink.m_unpauseOnEnter)
            {
                autoSoundLink.m_autoSound.audioSource.UnPause();

                // over time fade in VolumeMultiplier
                autoSoundLink.m_autoSound.FadeVolumeMultiplier(1f, autoSoundLink.m_fadeInTime);
            }
        }

        // play new auto sounds (if set to)
        foreach (AutoSoundLink autoSoundLink in m_linkedAutoSounds)
        {
            if (autoSoundLink.m_autoSound != null && autoSoundLink.m_playNewOnEnter)
            {
                autoSoundLink.m_autoSound.PlayRandomClip();
            }
        }
    }

    /// <summary>
    /// Called when the audio listener exits the zone.
    /// </summary>
    private void ExitedZone()
    {
        m_isInZone = false;

        UpdateAllZoneRatios();

        // disable auto sounds (if set to)
        foreach (AutoSoundLink autoSoundLink in m_linkedAutoSounds)
        {
            if (autoSoundLink.m_autoSound != null && autoSoundLink.m_pauseOnExit)
            {
                // over time fade out VolumeMultiplier
                autoSoundLink.m_autoSound.FadeVolumeMultiplier(0f, autoSoundLink.m_fadeOutTime);
            }
        }
    }

    /// <summary>
    /// Is the given position in the zone?
    /// </summary>
    /// <param name="_position">The position to check.</param>
    /// <returns>True if the position is in the zone.</returns>
    public bool IsInZone(Vector3 _position)
    {
        if (m_zoneShape == ZoneShape.Box)
        {
            return IsInBox(_position);
        }
        else if (m_zoneShape == ZoneShape.Sphere)
        {
            return IsInSphere(_position);
        }

        return false;
    }

    /// <summary>
    /// Is the given <see cref="AutoSound"/> in the zone?
    /// <br/>Overload of <see cref="IsInZone(Vector3)"/>.
    /// </summary>
    /// <param name="_autoSound"></param>
    /// <returns></returns>
    public bool IsInZone(AutoSound _autoSound)
    {
        return IsInZone(_autoSound.transform.position);
    }

    /// <summary>
    /// Is the given position in the box?
    /// </summary>
    /// <param name="_position">The position to check.</param>
    /// <returns>True if the position is in the box.</returns>
    private bool IsInBox(Vector3 _position)
    {
        Vector3 localPosition = transform.InverseTransformPoint(_position);
        return (localPosition.x >= -m_size.x / 2f && localPosition.x <= m_size.x / 2f &&
                localPosition.y >= -m_size.y / 2f && localPosition.y <= m_size.y / 2f &&
                localPosition.z >= -m_size.z / 2f && localPosition.z <= m_size.z / 2f);
    }

    /// <summary>
    /// Is the given position in the sphere?
    /// </summary>
    /// <param name="_position">The position to check.</param>
    /// <returns>True if the position is in the sphere.</returns>
    private bool IsInSphere(Vector3 _position)
    {
        Vector3 localPosition = transform.InverseTransformPoint(_position);
        return (localPosition.magnitude <= m_radius);
    }

    /// <summary>
    /// Automatically add all <see cref="AutoSound"/>s in the zone to the list.
    /// <br/>And remove any <see cref="AutoSound"/>s that are no longer in the zone.
    /// </summary>
    private void FindContainedAutoSounds()
    {
        // get all auto sounds in scene
        AutoSound[] autoSounds = FindObjectsOfType<AutoSound>(true);

        // get all auto sounds in zone
        List<AutoSound> autoSoundsInZone = new List<AutoSound>();
        List<AutoSound> autoSoundsNotInZone = new List<AutoSound>();
        foreach (AutoSound autoSound in autoSounds)
        {
            if (IsInZone(autoSound.transform.position))
            {
                autoSoundsInZone.Add(autoSound);
            }
            else
            {
                autoSoundsNotInZone.Add(autoSound);
            }
        }

        // check if any inZone auto sounds are already in another zone
        foreach (AutoSound autoSound in autoSoundsInZone)
        {
#if !UNITY_EDITOR
                foreach (AutoSoundZone autoSoundZone in AllAutoSoundZones)
                {
                    if (autoSoundZone != this && autoSoundZone.m_linkedAutoSounds.Find(x => x.m_autoSound == autoSound) != null)
                    {
                        // remove auto sound from list
                        autoSoundsInZone.Remove(autoSound);
                        break;
                    }
                }
#else
            // AllAutoSoundZones wont be populated in editor, so find all
            AutoSoundZone[] allAutoSoundZones = FindObjectsOfType<AutoSoundZone>(true);
            foreach (AutoSoundZone autoSoundZone in allAutoSoundZones)
            {
                if (autoSoundZone != this && autoSoundZone.m_linkedAutoSounds.Find(x => x.m_autoSound == autoSound) != null)
                {
                    // remove auto sound from list
                    autoSoundsInZone.Remove(autoSound);
                    break;
                }
            }
#endif
        }

        // add in zone auto sounds to list
        foreach (AutoSound autoSound in autoSoundsInZone)
        {
            // check if auto sound is already in list
            if (m_linkedAutoSounds.Find(x => x.m_autoSound == autoSound) == null)
            {
                // add auto sound to list
                m_linkedAutoSounds.Add(new AutoSoundLink(autoSound));
            }
        }

        // remove out of zone auto sounds from list
        foreach (AutoSound autoSound in autoSoundsNotInZone)
        {
            // check if auto sound is in list
            AutoSoundLink autoSoundLink = m_linkedAutoSounds.Find(x => x.m_autoSound == autoSound);
            if (autoSoundLink != null)
            {
                // remove auto sound from list
                m_linkedAutoSounds.Remove(autoSoundLink);
            }
        }
    }

    public bool ValidateAllAutoSoundLinks()
    {
        bool didChange = false;
        foreach (AutoSoundLink autoSoundLink in m_linkedAutoSounds)
        {
            if (autoSoundLink.Validate())
            {
                didChange = true;
            }
        }

        return didChange;
    }

    /// <summary>
    /// Is the given <see cref="AutoSound"/> linked to this <see cref="AutoSoundZone"/>?
    /// </summary>
    /// <param name="_autoSound"></param>
    /// <returns></returns>
    public bool IsAutoSoundLinked(AutoSound _autoSound)
    {
        return m_linkedAutoSounds.Find(x => x.m_autoSound == _autoSound) != null;
    }

    /// <summary>
    /// Is the given <see cref="AutoSound"/> linked to any <see cref="AutoSoundZone"/>?
    /// </summary>
    /// <param name="_autoSound"></param>
    /// <returns>The first <see cref="AutoSoundZone"/> the <see cref="AutoSound"/> is linked to. Null if not linked to any.</returns>
    public static AutoSoundZone IsAutoSoundLinkedToAnyZone(AutoSound _autoSound)
    {
        foreach (AutoSoundZone autoSoundZone in AllAutoSoundZones)
        {
            if (autoSoundZone.IsAutoSoundLinked(_autoSound))
            {
                return autoSoundZone;
            }
        }

        return null;
    }

    /// <summary>
    /// Updates the <see cref="AutoSoundZone.m_currentZoneVolumeRatio"/> of all <see cref="AutoSoundZone"/>s.
    /// <br/>This is used to control the <see cref="AutoSound.ZoneVolumeMultiplier"/> of all <see cref="AutoSound"/>s in the zone.
    /// </summary>
    private static void UpdateAllZoneRatios()
    {
        Vector3 audioListenerPosition = Camera.main?.transform.position ?? Vector3.zero;

        // get all of the sound zones, and get the highest priority
        float highestPriority = float.MinValue;
        foreach (AutoSoundZone autoSoundZone in AllAutoSoundZones)
        {
            if (autoSoundZone.IsInZone(audioListenerPosition))
            {
                if (autoSoundZone.m_priority > highestPriority)
                {
                    highestPriority = autoSoundZone.m_priority;
                }
            }
        }
        // update all ratios
        foreach (AutoSoundZone autoSoundZone in AllAutoSoundZones)
        {
            if (autoSoundZone.IsInZone(audioListenerPosition))
            {
                // the current ratio is based on the highest priority (highest will have 1)
                autoSoundZone.m_currentZoneVolumeRatio = autoSoundZone.m_priority / highestPriority;
                // if the highest is 10x above this, then the ratio will be 0
                if (autoSoundZone.m_currentZoneVolumeRatio <= 0.1f)
                {
                    autoSoundZone.m_currentZoneVolumeRatio = 0f;
                }
                foreach (AutoSoundLink autoSoundLink in autoSoundZone.m_linkedAutoSounds)
                {
                    if (autoSoundLink.m_autoSound != null)
                    {
                        // over time fade in ZoneVolumeMultiplier
                        autoSoundLink.m_autoSound.FadeZoneVolumeMultiplier(autoSoundZone.m_currentZoneVolumeRatio, autoSoundZone.m_fadeTime);
                    }
                }
            }
        }
    }

    // custom editor
#if UNITY_EDITOR
    [CustomEditor(typeof(AutoSoundZone))]
    public class AutoSoundZone_Editor : Editor
    {
        BoxBoundsHandle m_boxHandle = new BoxBoundsHandle();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            AutoSoundZone m_target = (AutoSoundZone)target;

            // validate all auto sound links
            if (m_target.ValidateAllAutoSoundLinks())
            {
                // set dirty
                EditorUtility.SetDirty(m_target);
            }

            if (GUILayout.Button(new GUIContent("Find Contained AutoSounds", "Automatically add all AutoSounds in the zone to the list.\nAnd remove any AutoSounds that are no longer in the zone.")))
            {
                m_target.FindContainedAutoSounds();

                m_target.ValidateAllAutoSoundLinks();

                // set dirty
                EditorUtility.SetDirty(m_target);
            }
        }

        private void OnSceneGUI()
        {
            AutoSoundZone m_target = (AutoSoundZone)target;

            // draw handles
            if (m_target.m_zoneShape == ZoneShape.Box)
            {
                Vector3 position = m_target.transform.position;
                Quaternion rotation = m_target.transform.rotation;
                Vector3 size = m_target.m_size;

                // box handles
                m_boxHandle.center = position;
                m_boxHandle.size = size;
                m_boxHandle.SetColor(Color.white);
                m_boxHandle.handleColor = Color.red;
                EditorGUI.BeginChangeCheck();
                m_boxHandle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_target, "Change Zone Size");
                    // move zone
                    m_target.transform.position = m_boxHandle.center;
                    // resize zone
                    m_target.m_size = m_boxHandle.size;
                }

            }
            else if (m_target.m_zoneShape == ZoneShape.Sphere)
            {
                float radius = m_target.m_radius;
                Vector3 position = m_target.transform.position;
                Quaternion rotation = m_target.transform.rotation;

                // sphere handles
                EditorGUI.BeginChangeCheck();
                radius = Handles.RadiusHandle(rotation, position, radius);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_target, "Change Zone Radius");
                    // resize zone
                    m_target.m_radius = radius;
                }
            }
        }
    }
#endif
}