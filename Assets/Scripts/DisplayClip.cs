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
    [SerializeField] protected ClipDefinition m_defaultClipDefinition = null;

    [Header("Data")]
    [Utils.ReadOnly, SerializeField] protected ClipDefinition m_clipData = null;
    public ClipDefinition ClipData { get { return m_clipData; } }

    /// <summary>
    /// Sets the clip to display the given clip data.
    /// </summary>
    /// <param name="_ClipDefinition"></param>
    public void SetClip(ClipDefinition _ClipDefinition, int _amount = -1)
    {
        // destroy all temp shells
        DestroyShells();

        // set the clip definition
        m_clipData = _ClipDefinition;

        // null check
        if (_ClipDefinition == null)
        {
            return;
        }

        // spawn in cosmetic shells instead of animating
        for (int i = 0; i < m_shellPoints.Count; i++)
        {
            Transform shell = m_shellPoints[i];
            shell.localScale = Vector3.zero;

            if (i < _ClipDefinition.m_shells.Count && (i >= m_shellPoints.Count - _amount || _amount == -1))
            {
                ShellDefinition shellDefinition = _ClipDefinition.m_shells[i];

                GameObject tempShell = shellDefinition.InstantiateCosmeticShell(shell.parent, _parent: true);
                m_spawnedShells.Add(tempShell);

                tempShell.transform.position = shell.position;
                // add offset
                tempShell.transform.rotation = shell.rotation * Quaternion.Euler(m_rotationOffset);
                tempShell.transform.localScale = Vector3.one;

                StartCoroutine(CosmeticTrackShells(tempShell, shell));
            }
        }
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