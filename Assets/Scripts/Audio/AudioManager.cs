using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


/// <summary>
/// This class is responsible for managing the audio of the game.
/// </summary>
public class AudioManager : MonoBehaviour
{
    const string EXPOSED_VOLUME_NAME = "Vol";

    // singleton
    public static AudioManager Instance { get; private set; } = null;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer m_audioMixer = null;
    public static AudioMixer Mixer { get { return Instance?.m_audioMixer; } }

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup m_masterGroup = null;
    public static AudioMixerGroup Master { get { return Instance?.m_masterGroup; } }
    [SerializeField] private AudioMixerGroup m_ambienceGroup = null;
    public static AudioMixerGroup Ambience { get { return Instance?.m_ambienceGroup; } }
    [SerializeField] private AudioMixerGroup m_dialogueGroup = null;
    public static AudioMixerGroup Dialogue { get { return Instance?.m_dialogueGroup; } }
    [SerializeField] private AudioMixerGroup m_musicGroup = null;
    public static AudioMixerGroup Music { get { return Instance?.m_musicGroup; } }
    [SerializeField] private AudioMixerGroup m_SFXGroup = null;
    public static AudioMixerGroup SFX { get { return Instance?.m_SFXGroup; } }
    [SerializeField] private AudioMixerGroup m_UIGroup = null;
    public static AudioMixerGroup UI { get { return Instance?.m_UIGroup; } }

    private List<AudioMixerGroup> m_mixerGroups = new List<AudioMixerGroup>();
    public static List<AudioMixerGroup> MixerGroups { get { return Instance?.m_mixerGroups; } }

    private Dictionary<string, AutoSound.AutoSound_Prefab> m_autoSoundPrefabs = new Dictionary<string, AutoSound.AutoSound_Prefab>();

    private void Awake()
    {
        // if instance already exists, destroy this one
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // set instance
        Instance = this;

        // don't destroy on load
        DontDestroyOnLoad(gameObject);

        // add mixer groups to list
        m_mixerGroups.Add(m_masterGroup);
        m_mixerGroups.Add(m_ambienceGroup);
        m_mixerGroups.Add(m_dialogueGroup);
        m_mixerGroups.Add(m_musicGroup);
        m_mixerGroups.Add(m_SFXGroup);
        m_mixerGroups.Add(m_UIGroup);
    }

    private void Start()
    {
        // async load coroutine
        StartCoroutine(LoadAllPrefabsAsync());
    }

    /// <summary>
    /// Gets the name of the exposed volume parameter of the given AudioMixerGroup.
    /// </summary>
    /// <param name="_group"></param>
    /// <returns></returns>
    public static string GetExposedVolumeName(AudioMixerGroup _group)
    {
        return _group.name + EXPOSED_VOLUME_NAME;
    }

    /// <summary>
    /// Gets the volume of the given AudioMixerGroup.
    /// </summary>
    /// <param name="_group"></param>
    /// <returns>0 to 1</returns>
    public static float GetVolume(AudioMixerGroup _group)
    {
        // get the name of the exposed volume parameter
        string exposedName = GetExposedVolumeName(_group);

        // get the volume of the group (-80 to 20)
        float volume = 0f;
        _group.audioMixer.GetFloat(exposedName, out volume);

        // scale to 0-1
        volume = Mathf.InverseLerp(-80f, 20f, volume);

        // return volume
        return volume;
    }

    /// <summary>
    /// Sets and saves the volume of the given AudioMixerGroup.
    /// <br/>Volume is scaled from 0-1.
    /// </summary>
    /// <param name="_group"></param>
    /// <param name="_volume">0 to 1</param>
    public static void SetVolume(AudioMixerGroup _group, float _volume)
    {
        // get the name of the exposed volume parameter
        string exposedName = GetExposedVolumeName(_group);

        // scale to -80-20
        _volume = Mathf.Lerp(-80f, 20f, _volume);

        // set the volume of the group
        _group.audioMixer.SetFloat(exposedName, _volume);

        SaveVolume(_group);
    }

    /// <summary>
    /// Sets and saves the volume of the given AudioMixerGroup.
    /// </summary>
    public static void SaveVolume(AudioMixerGroup _group)
    {
        if (Instance == null)
        {
            Debug.LogError("AudioManager.SaveVolume | Instance is null");
            return;
        }

        // get the name of the exposed volume parameter
        string exposedName = GetExposedVolumeName(_group);

        // get the volume of the group
        float volume = 0f;
        _group.audioMixer.GetFloat(exposedName, out volume);

        // save the volume
        PlayerPrefs.SetFloat(exposedName, volume);
    }

    /// <summary>
    /// Saves the volume of all AudioMixerGroups.
    /// </summary>
    public static void SaveAllVolumes()
    {
        if (Instance == null)
        {
            Debug.LogError("AudioManager.SaveAllVolumes | Instance is null");
            return;
        }

        // save all volumes
        foreach (AudioMixerGroup group in Instance.m_mixerGroups)
        {
            SaveVolume(group);
        }
    }

    /// <summary>
    /// Spawns an AutoSound prefab at the given position.
    /// </summary>
    /// <typeparam name="T">The type of AutoSound_Prefab prefab to spawn.</typeparam>
    /// <param name="_position"></param>
    public static AutoSound SpawnSound<T>(Vector3 _position) where T : AutoSound.AutoSound_Prefab
    {
        // get name of the type, it will be the name of the prefab
        string prefabName = typeof(T).Name;
        return SpawnSound(prefabName, _position);
    }

    /// <summary>
    /// Spawns an AutoSound prefab at the given position.
    /// </summary>
    /// <param name="_autoSoundPrefabName">The name of the AutoSound_Prefab prefab to spawn.</param>
    /// <param name="_position"></param>
    /// <returns></returns>
    public static AutoSound SpawnSound(string _autoSoundPrefabName, Vector3 _position)
    {
        string prefabResourcePath = AutoSound.AutoSound_Prefab.RESOURCES_FOLDER + _autoSoundPrefabName;

        GameObject prefab = null;
        // if already exists in dictionary, get it
        if (Instance.m_autoSoundPrefabs.ContainsKey(_autoSoundPrefabName))
        {
            prefab = Instance.m_autoSoundPrefabs[_autoSoundPrefabName].gameObject;
        }
        // otherwise, get prefab from resources
        else
        {
            prefab = Resources.Load<GameObject>(prefabResourcePath);

            // if prefab doesn't exist, error
            if (prefab == null)
            {
                Debug.LogError("AudioManager.SpawnSound | Prefab at " + prefabResourcePath + " doesn't exist");
                return null;
            }

            // store prefab in dictionary
            Instance.m_autoSoundPrefabs.Add(_autoSoundPrefabName, prefab.GetComponent<AutoSound.AutoSound_Prefab>());
        }

        // instantiate prefab
        GameObject instance = Instantiate(prefab, _position, Quaternion.identity);
        AutoSound autoSound = instance.GetComponent<AutoSound>();

        // if no AutoSound component, error
        if (autoSound == null)
        {
            Debug.LogError("AudioManager.SpawnSound | Prefab at " + prefabResourcePath + " doesn't have an AutoSound component");
            return null;
        }

        return autoSound;
    }

    /// <summary>
    /// Loads all AutoSound_Prefab(s) in the Resources folder, into the dictionary.
    /// <br/>Expensive and is not fast, use <see cref="LoadAllPrefabsAsync"/> instead (even if not doing async)
    /// </summary>
    private static void ForceLoadAllPrefabs()
    {
        // find all AutoSound's in resources folder AutoSound.AutoSound_Prefab.RESOURCES_FOLDER
        Object[] prefabs = Resources.LoadAll(AutoSound.AutoSound_Prefab.RESOURCES_FOLDER, typeof(AutoSound.AutoSound_Prefab));
        // store in dictionary
        foreach (Object prefab in prefabs)
        {
            AutoSound.AutoSound_Prefab autoSoundPrefab = (AutoSound.AutoSound_Prefab)prefab;
            Instance.m_autoSoundPrefabs[autoSoundPrefab.name] = autoSoundPrefab;
        }
    }

    /// <summary>
    /// Loads all AutoSound_Prefab child types that have resources, into the dictionary.
    /// <br/>Use this instead of <see cref="ForceLoadAllPrefabs"/> as it is faster.
    /// </summary>
    /// <returns></returns>
    private static IEnumerator LoadAllPrefabsAsync()
    {
        // use reflection to get all AutoSound_Prefab types
        System.Reflection.Assembly assembly = typeof(AutoSound.AutoSound_Prefab).Assembly;
        System.Type[] types = assembly.GetTypes();
        List<System.Type> autoSoundPrefabTypes = new List<System.Type>();
        foreach (System.Type type in types)
        {
            if (type.IsSubclassOf(typeof(AutoSound.AutoSound_Prefab)))
            {
                autoSoundPrefabTypes.Add(type);
            }
        }

        // // notification
        // Notification_Load loadNotif = NotificationManager.ShowNotification<Notification_Load>(new Notification_Load.Notification_Data_Load(
        //     _title: "Loading Audio",
        //     _dismissOnComplete: false,
        //     _fadeDelay: 2f
        // ));

        // load all AutoSound_Prefab prefabs
        int loadedCount = 0;
        foreach (System.Type type in autoSoundPrefabTypes)
        {
            string prefabResourcePath = AutoSound.AutoSound_Prefab.RESOURCES_FOLDER + type.Name;
            ResourceRequest request = Resources.LoadAsync<GameObject>(prefabResourcePath);
            yield return request;

            // loadNotif.SetLoadPercent01((float)loadedCount / (float)autoSoundPrefabTypes.Count);

            // if prefab doesn't exist, error
            if (request.asset == null)
            {
                Debug.LogError("AudioManager.LoadAllPrefabsAsync | Prefab at " + prefabResourcePath + " doesn't exist");
                continue;
            }

            // store prefab in dictionary (if prefab is valid)
            GameObject prefab = (GameObject)request.asset;
            if (prefab != null && prefab.GetComponent<AutoSound.AutoSound_Prefab>() != null)
            {
                Instance.m_autoSoundPrefabs[type.Name] = prefab.GetComponent<AutoSound.AutoSound_Prefab>();
            }

            loadedCount++;
        }

        // // set 100% loaded
        // loadNotif.SetLoadPercent01(1f);
        // loadNotif.SetTitle("Audio Loaded");
    }
}