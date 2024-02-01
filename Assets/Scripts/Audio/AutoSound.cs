using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Used on sound prefabs to automatically destroy after the sound plays
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AutoSound : MonoBehaviour
{
    /// <summary>
    /// Added to specific prefabs to enable loading them easily from resources in scripts (e.g. PlaySound<AutoSound_Footstep>() to play a footstep sound)
    /// </summary>
    public class AutoSound_Prefab : MonoBehaviour
    {
        public const string SCRIPT_FOLDER = "Assets/Scripts/Audio/PrefabScripts/";
        public const string SCRIPT_TEMPLATE = SCRIPT_FOLDER + "AutoSound_Template";

        public const string RESOURCES_FOLDER = "AutoSounds/";

        public AutoSound m_autoSound;
    }

    private static List<AutoSound> AllAutoSounds = new List<AutoSound>();

    [Header("Settings")]
    [SerializeField] private bool m_playOnStart = true;
    public bool PlayOnStart { get { return m_playOnStart; } set { m_playOnStart = value; } }
    [SerializeField] private bool m_destroyOnEnd = true;
    public bool DestroyOnEnd { get { return m_destroyOnEnd; } set { m_destroyOnEnd = value; } }
    [SerializeField] private bool m_newClipOnEnd = false;
    public bool NewClipOnEnd { get { return m_newClipOnEnd; } set { m_newClipOnEnd = value; } }
    [SerializeField] private bool m_hasPlayed = false;
    public bool HasPlayed { get { return m_hasPlayed; } }
    public float m_minTimeBeforeStart = 0.0f;
    public float m_maxTimeBeforeStart = 0.0f;
    [SerializeField, Utils.ReadOnly] private bool m_isDistancePaused = false;

    [Header("Volume")]
    public float m_minVolume = 0.9f;
    public float m_maxVolume = 1.0f;
    [SerializeField, Utils.ReadOnly] private float m_targetVolume = 1.0f;
    [SerializeField, Utils.ReadOnly] private float m_volumeMultiplier = 1.0f;
    public float VolumeMultiplier { get { return m_volumeMultiplier; } set { m_volumeMultiplier = value; UpdateVolume(); } }
    [SerializeField, Utils.ReadOnly] private float m_zoneVolumeMultiplier = 1.0f;
    public float ZoneVolumeMultiplier { get { return m_zoneVolumeMultiplier; } set { m_zoneVolumeMultiplier = value; UpdateVolume(); } }

    [Header("Pitch")]
    public float m_minPitch = 0.9f;
    public float m_maxPitch = 1.1f;

    [Header("Clips")]
    [Range(0f, 1f), Tooltip("Chance to play sound when PlayRandomClip(_useChance: true) is called")]
    public float m_playChance = 1.0f;
    public float m_minTimeBetweenPlays = 0.0f;
    public float m_maxTimeBetweenPlays = 0.0f;
    public AudioClip[] clips;

    [Header("References")]
    public AudioSource audioSource;

    [Header("Routines")]
    public IEnumerator m_volumeMultiplierRoutine = null;
    public IEnumerator m_zoneVolumeMultiplierRoutine = null;
    public IEnumerator m_randomClipRoutine = null;
    [SerializeField, Utils.ReadOnly] private float m_countdownTimer = 0.0f;

    private void Awake()
    {
        // get audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // add to list
        if (!AllAutoSounds.Contains(this))
        {
            AllAutoSounds.Add(this);
        }
    }

    private void OnDestroy()
    {
        // remove from list
        if (AllAutoSounds.Contains(this))
        {
            AllAutoSounds.Remove(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_hasPlayed = false;

        // play on start
        if (m_playOnStart)
        {
            // get random time between plays
            float timeBetweenPlays = UnityEngine.Random.Range(m_minTimeBeforeStart, m_maxTimeBeforeStart);

            // play random clip after time
            PlayRandomClipAfterTime(timeBetweenPlays, _useChance: true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!audioSource) return;

        // listen to volume multiplier
        UpdateVolume();

        // after sound has played, destroy or play new clip
        if (m_hasPlayed && !audioSource.isPlaying)
        {
            if (m_destroyOnEnd)
            {
                Destroy(gameObject);
            }
            // only play if not already queued
            else if (m_newClipOnEnd && m_randomClipRoutine == null)
            {
                // get random time between plays
                float timeBetweenPlays = UnityEngine.Random.Range(m_minTimeBetweenPlays, m_maxTimeBetweenPlays);

                // play random clip after time
                PlayRandomClipAfterTime(timeBetweenPlays, _notSame: true, _useChance: true);
            }
        }

        Vector3 audioListenerPosition = Camera.main?.transform.position ?? Vector3.zero;

        // if not 2D sound (as they don't have a max distance)
        // And if not part of a zone (as zone will control distance volume/pausing)
        if (audioSource.spatialBlend > 0 && AutoSoundZone.IsAutoSoundLinkedToAnyZone(this) == null)
        {
            // if listener is past max distance
            if (Vector3.Distance(transform.position, audioListenerPosition) > audioSource.maxDistance)
            {
                if (!m_isDistancePaused && audioSource.isPlaying)
                {
                    m_isDistancePaused = true;
                    // force pause regardless of volume
                    audioSource.Pause();
                }
            }
            // if listener is within max distance, unpause
            else
            {
                if (m_isDistancePaused)
                {
                    m_isDistancePaused = false;
                    // UpdateVolume will unpause if volume is not 0
                    UpdateVolume();
                }
            }
        }
    }

    /// <summary>
    /// Plays one of the clips randomly
    /// </summary>
    public void PlayRandomClip(bool _notSame = false, bool _useChance = false)
    {
        // if no clips are set, do nothing
        if (clips.Length == 0) return;

        // if chance is set, check if it should play
        if (_useChance && UnityEngine.Random.Range(0f, 1f) > m_playChance) return;

        m_hasPlayed = true;

        AudioClip clip = audioSource.clip;
        // can only play a different clip if there are at least 2 clips
        if (_notSame && clips.Length >= 2 && clip != null)
        {
            // remove current clip from list
            List<AudioClip> tempList = new List<AudioClip>(clips);
            tempList.Remove(clip);

            // get random clip from list
            clip = tempList[UnityEngine.Random.Range(0, tempList.Count)];
        }
        // otherwise, just get a random clip
        else
        {
            clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        }


        audioSource.clip = clip;
        m_targetVolume = UnityEngine.Random.Range(m_minVolume, m_maxVolume);
        UpdateVolume();
        audioSource.pitch = UnityEngine.Random.Range(m_minPitch, m_maxPitch);
        audioSource.Play();
    }

    /// <summary>
    /// Waits for a set amount of time, then plays a random clip
    /// <br/>Use <see cref="PlayRandomClipAfterTime(float)"/> instead
    /// </summary>
    /// <param name="_time"></param>
    /// <returns></returns>
    private IEnumerator RandomClipRoutine(float _time, bool _notSame = false, bool _useChance = false)
    {
        m_countdownTimer = _time;
        float startTime = Time.time;
        float endTime = startTime + _time;
        while (Time.time < endTime)
        {
            m_countdownTimer = endTime - Time.time;
            yield return null;
        }
        PlayRandomClip(_notSame, _useChance);
        m_randomClipRoutine = null;
    }

    /// <summary>
    /// Plays a random clip after a set amount of time
    /// </summary>
    /// <param name="_time"></param>
    public void PlayRandomClipAfterTime(float _time, bool _notSame = false, bool _useChance = false)
    {
        if (m_randomClipRoutine != null) StopCoroutine(m_randomClipRoutine);
        m_randomClipRoutine = RandomClipRoutine(_time, _notSame, _useChance);
        StartCoroutine(m_randomClipRoutine);
    }

    /// <summary>
    /// Fades the volume multiplier over time
    /// <br/>Use <see cref="FadeVolumeMultiplier(float, float)"/> instead
    /// </summary>
    /// <param name="_targetVolume"></param>
    /// <param name="_fadeTime"></param>
    /// <returns></returns>
    private IEnumerator FadeVolumeMultiplierRoutine(float _targetVolume, float _fadeTime)
    {
        float startVolume = VolumeMultiplier;
        float startTime = Time.time;
        float endTime = startTime + _fadeTime;
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / _fadeTime;
            VolumeMultiplier = Mathf.Lerp(startVolume, _targetVolume, t);
            yield return null;
        }
        VolumeMultiplier = _targetVolume;

        // clear routine
        m_volumeMultiplierRoutine = null;
    }

    /// <summary>
    /// Fades the volume multiplier over time
    /// </summary>
    /// <param name="_targetVolume"></param>
    /// <param name="_fadeTime"></param>
    public void FadeVolumeMultiplier(float _targetVolume, float _fadeTime)
    {
        if (m_volumeMultiplierRoutine != null) StopCoroutine(m_volumeMultiplierRoutine);
        m_volumeMultiplierRoutine = FadeVolumeMultiplierRoutine(_targetVolume, _fadeTime);
        StartCoroutine(m_volumeMultiplierRoutine);
    }

    /// <summary>
    /// Fades the zone volume multiplier over time
    /// <br/>Use <see cref="FadeZoneVolumeMultiplier(float, float)"/> instead
    /// </summary>
    /// <param name="_targetVolume"></param>
    /// <param name="_fadeTime"></param>
    /// <returns></returns>
    private IEnumerator FadeZoneVolumeMultiplierRoutine(float _targetVolume, float _fadeTime)
    {
        float startVolume = ZoneVolumeMultiplier;
        float startTime = Time.time;
        float endTime = startTime + _fadeTime;
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / _fadeTime;
            ZoneVolumeMultiplier = Mathf.Lerp(startVolume, _targetVolume, t);
            yield return null;
        }
        ZoneVolumeMultiplier = _targetVolume;

        // clear routine
        m_zoneVolumeMultiplierRoutine = null;
    }

    /// <summary>
    /// Fades the zone volume multiplier over time
    /// </summary>
    /// <param name="_targetVolume"></param>
    /// <param name="_fadeTime"></param>
    public void FadeZoneVolumeMultiplier(float _targetVolume, float _fadeTime)
    {
        if (m_zoneVolumeMultiplierRoutine != null) StopCoroutine(m_zoneVolumeMultiplierRoutine);
        m_zoneVolumeMultiplierRoutine = FadeZoneVolumeMultiplierRoutine(_targetVolume, _fadeTime);
        StartCoroutine(m_zoneVolumeMultiplierRoutine);
    }

    /// <summary>
    /// Updates the volume of the audio source according to the target volume and volume multipliers
    /// <br/>Call after changing any of the volume variables
    /// </summary>
    public void UpdateVolume()
    {
        audioSource.volume = m_targetVolume * VolumeMultiplier * m_zoneVolumeMultiplier;

        // if volume is 0, pause
        if (audioSource.volume <= 0f)
        {
            audioSource.Pause();
        }
        // if volume is not 0 and is not distance paused, unpause
        else if (!m_isDistancePaused)
        {
            audioSource.UnPause();
        }
    }

#if UNITY_EDITOR
    // static bool m_addAfterCompile = false;
    // static AutoSound m_waitingParent = null;
    // static string m_toAddName = null;
    const string PREF_ADD_AFTER_COMPILE = "AutoSound_AddAfterCompile";
    const string PREF_WAITING_PARENT = "AutoSound_WaitingParent";
    const string PREF_TO_ADD_NAME = "AutoSound_ToAddName";

    private void OnValidate()
    {
        // get audio source + save changes
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            EditorUtility.SetDirty(this);
        }
    }

    /// <summary>
    /// Creates a prefab script for this prefab, and marks it to be added to the prefab after scripts are compiled.
    /// </summary>
    private void CreatePrefabScript()
    {
        if (GetComponent<AutoSound_Prefab>() != null)
        {
            Debug.LogError("AutoSound: Prefab script already exists on this object.\n" +
                "If you want to create a new script, destroy the old one first.");
            return;
        }

        // ensure files/folders exist
        if (!Directory.Exists(AutoSound_Prefab.SCRIPT_FOLDER))
        {
            Directory.CreateDirectory(AutoSound_Prefab.SCRIPT_FOLDER);
        }
        if (!File.Exists(AutoSound_Prefab.SCRIPT_TEMPLATE))
        {
            Debug.LogError("AutoSound: Template file not found at " + AutoSound_Prefab.SCRIPT_TEMPLATE);
            return;
        }

        string prefabName = gameObject.name;

        // get template file content as list of lines
        List<string> templateFileContent = new List<string>();
        templateFileContent = File.ReadAllLines(AutoSound_Prefab.SCRIPT_TEMPLATE).ToList();

        // replace [NAME] with prefab name
        templateFileContent = templateFileContent.Select(line => line.Replace("[NAME]", prefabName)).ToList();

        // create new file
        string newFilePath = AutoSound_Prefab.SCRIPT_FOLDER + prefabName + ".cs";
        // if file already exists, error
        if (File.Exists(newFilePath))
        {
            Debug.LogError("AutoSound: File already exists at " + newFilePath);
            return;
        }
        File.WriteAllLines(newFilePath, templateFileContent);

        // save vars for later (after scripts are compiled)
        EditorPrefs.SetBool(PREF_ADD_AFTER_COMPILE, true);
        EditorPrefs.SetString(PREF_TO_ADD_NAME, "Trimble.TVW.Audio." + prefabName);
        EditorPrefs.SetInt(PREF_WAITING_PARENT, GetInstanceID());

        // set dirty to prevent user leaving
        EditorUtility.SetDirty(this);

        // import, open, and refresh
        AssetDatabase.ImportAsset(newFilePath);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Called after scripts are compiled, to add the prefab script to the marked prefab.
    /// </summary>
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void CreateAssetWhenReady()
    {
        // get vars
        bool addAfterCompile = EditorPrefs.GetBool(PREF_ADD_AFTER_COMPILE, false);
        string toAddName = EditorPrefs.GetString(PREF_TO_ADD_NAME, null);
        int waitingParentID = EditorPrefs.GetInt(PREF_WAITING_PARENT, -1);

        if (addAfterCompile && !string.IsNullOrEmpty(toAddName) && waitingParentID != -1)
        {
            // clear vars
            EditorPrefs.SetBool(PREF_ADD_AFTER_COMPILE, false);
            EditorPrefs.SetString(PREF_TO_ADD_NAME, null);
            EditorPrefs.SetInt(PREF_WAITING_PARENT, -1);

            // get waiting parent
            AutoSound waitingParent = EditorUtility.InstanceIDToObject(waitingParentID) as AutoSound;
            if (waitingParent == null)
            {
                Debug.LogError("AutoSound: Waiting parent not found.");
                return;
            }

            // create and add prefab script
            AutoSound_Prefab prefabScript = waitingParent.gameObject.AddComponent(Type.GetType(toAddName)) as AutoSound_Prefab;

            // if prefab script is null, error
            if (prefabScript == null)
            {
                Debug.LogError("AutoSound: Prefab script not found on this object.");
                return;
            }

            // set reference
            prefabScript.m_autoSound = waitingParent;

            // set dirty
            EditorUtility.SetDirty(waitingParent);
        }
    }

    // Custom Editor
    [CustomEditor(typeof(AutoSound))]
    public class AutoSoundEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            AutoSound myScript = (AutoSound)target;

            // play random clip button
            if (GUILayout.Button("Play Random Clip"))
            {
                myScript.PlayRandomClip();
            }

            // play random not the same clip button
            if (GUILayout.Button("Play Random (Not same as last)"))
            {
                myScript.PlayRandomClip(true);
            }

            // create prefab script button
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Create Prefab Script"))
            {
                myScript.CreatePrefabScript();
            }
            GUI.backgroundColor = Color.white;

            // delete and destroy prefab script button
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Destroy Prefab Script"))
            {
                AutoSound_Prefab prefabScript = myScript.GetComponent<AutoSound_Prefab>();
                if (prefabScript == null)
                {
                    Debug.LogError("AutoSound: Prefab script not found on this object.");
                    return;
                }

                string filePath = AutoSound_Prefab.SCRIPT_FOLDER + prefabScript.name + ".cs";

                // destroy and remove component
                if (prefabScript != null)
                {
                    DestroyImmediate(prefabScript, true);

                    // set dirty
                    EditorUtility.SetDirty(myScript);
                }

                // delete file and remove from project
                if (File.Exists(filePath))
                {
                    AssetDatabase.DeleteAsset(filePath);
                    AssetDatabase.Refresh();
                }
            }
            GUI.backgroundColor = Color.white;
        }
    }
#endif
}