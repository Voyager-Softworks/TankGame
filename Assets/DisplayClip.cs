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
    [Header("References")]
    public GameObject m_shellPrefab;
    public Transform m_clip;
    [FormerlySerializedAs("m_clipShells")]
    public List<Transform> m_shellPoints;

    private List<GameObject> m_tempShells = new List<GameObject>();

    /// <summary>
    /// Sets the clip to display the given clip data.
    /// </summary>
    /// <param name="_clipData"></param>
    public void SetClip(ClipData _clipData, int _amount = -1)
    {
        // destroy all temp shells
        DestroyShells();

        // null check
        if (_clipData == null)
        {
            return;
        }

        // spawn in cosmetic shells instead of animating
        for (int i = 0; i < m_shellPoints.Count; i++)
        {
            Transform shell = m_shellPoints[i];
            shell.localScale = Vector3.zero;

            if (i < _clipData.m_shells.Count && (i >= m_shellPoints.Count - _amount || _amount == -1))
            {
                ShellData shellData = _clipData.m_shells[i];

                GameObject tempShell = Player_Gun.InstantiateCosmeticShell(m_shellPrefab, shellData, shell.parent, _parent: true);
                m_tempShells.Add(tempShell);

                tempShell.transform.position = shell.position;
                // subtract 90 degrees to x rotation
                tempShell.transform.rotation = shell.rotation * Quaternion.Euler(-90f, 0f, 0f);
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
        for (int i = m_tempShells.Count - 1; i >= 0; i--)
        {
            Destroy(m_tempShells[i]);
        }
        m_tempShells.Clear();
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