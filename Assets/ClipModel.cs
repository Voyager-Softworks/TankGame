using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player_Gun;

/// <summary>
/// Controls the visual representation of a clip.
/// </summary>
public class ClipModel : MonoBehaviour
{
    [Header("References")]
    public GameObject m_shellPrefab;
    public Transform m_clip;
    public List<Transform> m_clipShells;

    private List<GameObject> m_tempShells = new List<GameObject>();

    public void SetClip(ClipData _clipData)
    {
        // destroy all temp shells
        for (int i = m_tempShells.Count - 1; i >= 0; i--)
        {
            Destroy(m_tempShells[i]);
        }

        // null check
        if (_clipData == null)
        {
            return;
        }

        // spawn in cosmetic shells instead of animating
        for (int i = 0; i < m_clipShells.Count; i++)
        {
            Transform shell = m_clipShells[i];
            shell.localScale = Vector3.zero;

            if (i < _clipData.m_shells.Count)
            {
                ShellData shellData = _clipData.m_shells[i];

                GameObject tempShell = InstantiateCosmeticShell(shellData, shell.parent, _parent: true);
                m_tempShells.Add(tempShell);

                tempShell.transform.position = shell.position;
                // subtract 90 degrees to x rotation
                tempShell.transform.rotation = shell.rotation * Quaternion.Euler(-90f, 0f, 0f);
                tempShell.transform.localScale = Vector3.one;
            }
        }
    }

    private GameObject InstantiateCosmeticShell(ShellData _toCopy, Transform _pos, bool _parent = false)
    {
        GameObject shell = Instantiate(m_shellPrefab, _pos.position, _pos.rotation, _parent ? _pos : null);

        // no physics if parented
        Rigidbody shellRb = shell.GetComponent<Rigidbody>();
        if (shellRb != null && _parent)
        {
            Destroy(shellRb);
            // destroy all colliders
            Collider[] colliders = shell.GetComponentsInChildren<Collider>();
            for (int i = colliders.Length - 1; i >= 0; i--)
            {
                Destroy(colliders[i]);
            }
        }

        // update visuals
        MosinShell mosinShell = shell.GetComponent<MosinShell>();
        mosinShell.SetShellData(_toCopy);

        return shell;
    }
}