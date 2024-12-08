using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static Player_Gun;

/// <summary>
/// Controls the visual representation of a clip.
/// </summary>
public class DisplayClip : MonoBehaviour
{
    [Header("Settings")]
    public Vector3 m_rotationOffset = Vector3.zero;

    [Header("References")]
    public Transform m_clip;
    [FormerlySerializedAs("m_clipShells")]
    public List<Transform> m_shellPoints;

    private List<GameObject> m_spawnedShells = new List<GameObject>();
    [SerializeField, Tooltip("The type of clip this will represent")] protected ClipDefinition m_defaultClipDefinition = null;
    public ClipDefinition DefaultClipDefinition { get { return m_defaultClipDefinition; } }

    [Header("Data")]
    [Utils.ReadOnly, SerializeField, Tooltip("The current clip this is representing")] protected ClipDefinition m_clipData = null;
    public ClipDefinition ClipData { get { return m_clipData; } }

    public System.Action OnClipChanged;

    /// <summary>
    /// Sets the clip to display the given clip data.
    /// </summary>
    /// <param name="_ClipDefinition"></param>
    public void SetClip(ClipDefinition _ClipDefinition)
    {
        // destroy all temp shells
        DestroyShells();

        // set the clip definition
        m_clipData = _ClipDefinition;

        // only update if not null
        if (_ClipDefinition != null)
        {
            int clipAmount = _ClipDefinition.GetShellCount();
            int emptyShellPoints = Mathf.Max(m_shellPoints.Count - clipAmount, 0);
            for (int i = emptyShellPoints; i < m_shellPoints.Count; i++)
            {
                Transform shellPoint = m_shellPoints[i];
                shellPoint.localScale = Vector3.zero;

                ShellDefinition shellDefinition = _ClipDefinition.GetShell(i - emptyShellPoints);

                GameObject tempShell = shellDefinition.InstantiateCosmeticShell(shellPoint.parent, _parent: true);
                // cant focus on the shells
                tempShell.GetComponent<Interactable_AmmoShell>().IsFocusable = false;

                m_spawnedShells.Add(tempShell);

                tempShell.transform.position = shellPoint.position;
                // add offset
                tempShell.transform.rotation = shellPoint.rotation * Quaternion.Euler(m_rotationOffset);
                tempShell.transform.localScale = Vector3.one;

                StartCoroutine(CosmeticTrackShells(tempShell, shellPoint));
            }
        }

        // invoke event
        OnClipChanged?.Invoke();
    }

    /// <summary>
    /// Destroys the instantiated shells.
    /// </summary>
    public void DestroyShells()
    {
        for (int i = m_spawnedShells.Count - 1; i >= 0; i--)
        {
            Destroy(m_spawnedShells[i]);
        }
        m_spawnedShells.Clear();
    }

    /// <summary>
	/// Coroutine for temp shells to track the animation.
	/// </summary>
	/// <returns></returns>
	private IEnumerator CosmeticTrackShells(GameObject _shell, Transform _pos, float _time = 10f)
    {
        // during the time, track the shell to the position
        float timeStart = Time.time;
        while (Time.time - timeStart < _time)
        {
            // null check
            if (_shell == null)
            {
                yield break;
            }

            // a little forward of the position
            _shell.transform.position = _pos.position /* + _shell.forward * 0.1f */;
            yield return null;
        }
    }
}