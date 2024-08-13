using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scriptable object to define a clip.
/// </summary>
[CreateAssetMenu(fileName = "ClipDefinition", menuName = "Clip Definition", order = 1)]
public class ClipDefinition : ScriptableObject
{
    [Header("Data")]
    public string m_name = "Clip";
    private int m_maxSize = 5;
    public int MaxSize { get { return m_maxSize; } }
    public ShellDefinition m_ammoType;
    /// <summary> i=0 is the bottom of the clip, i=last is the top of the clip. </summary>
    private List<ShellDefinition> m_shells = new List<ShellDefinition>();

    [Header("References")]
    [SerializeField] protected GameObject m_clipPrefab;

    /// <summary>
    /// Protected constructor to prevent instantiation.
    /// </summary>
    protected ClipDefinition()
    {
    }

    protected void Copy(ClipDefinition _toCopy)
    {
        m_name = _toCopy.m_name;
        m_maxSize = _toCopy.m_maxSize;
        m_ammoType = _toCopy.m_ammoType;
        m_shells = new List<ShellDefinition>(_toCopy.m_shells);
        m_clipPrefab = _toCopy.m_clipPrefab;
    }

    /// <summary>
    /// Create a new clip with the same data as this clip.
    /// </summary>
    /// <returns></returns>
    public ClipDefinition GetCopy()
    {
        ClipDefinition copy = ScriptableObject.CreateInstance<ClipDefinition>();
        copy.Copy(this);
        return copy;
    }

    /// <summary>
    /// Create a new clip of shells with random shell data.
    /// </summary>
    /// <param name="_shells"></param>
    /// <returns></returns>
    public ClipDefinition GetRandomInstance(int _shells = -1)
    {
        ClipDefinition copy = this.GetCopy();

        // if -1, use max size
        if (_shells == -1)
        {
            _shells = m_maxSize;
        }

        // reset/create list
        copy.m_shells = new List<ShellDefinition>();

        // add shells
        for (int i = 0; i < _shells; i++)
        {
            copy.m_shells.Add(m_ammoType.GetRandomInstance());
        }

        return copy;
    }

    /// <summary>
    /// Get the number of shells in the clip.
    /// </summary>
    /// <returns></returns>
    public int GetShellCount()
    {
        return m_shells?.Count ?? 0;
    }

    /// <summary>
    /// Get the shell at the given index.
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    public ShellDefinition GetShell(int _index)
    {
        if (_index < 0 || _index >= m_shells.Count)
        {
            return null;
        }
        return m_shells[_index];
    }

    /// <summary>
    /// Add a shell to the clip.
    /// </summary>
    /// <param name="_shell"></param>
    /// <returns>False if the clip is full or the shell is null.</returns>
    public bool AddShell(ShellDefinition _shell)
    {
        // null check
        if (_shell == null)
        {
            return false;
        }

        if (m_shells.Count < m_maxSize)
        {
            m_shells.Add(_shell);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Get the shell at the top of the clip (aka the last shell).
    /// </summary>
    /// <param name="_remove">Should the shell be removed from the clip?</param>
    /// <returns></returns>
    public ShellDefinition TopShell(bool _remove = false)
    {
        if (m_shells.Count == 0)
        {
            return null;
        }
        ShellDefinition shell = m_shells[m_shells.Count - 1];
        if (_remove)
        {
            RemoveAt(m_shells.Count - 1);
        }
        return shell;
    }

    /// <summary>
    /// Remove the shell at the given index.
    /// </summary>
    /// <param name="_index"></param>
    /// <returns></returns>
    public ShellDefinition RemoveAt(int _index)
    {
        if (_index < 0 || _index >= m_shells.Count)
        {
            return null;
        }
        ShellDefinition shell = m_shells[_index];
        m_shells.RemoveAt(_index);
        return shell;
    }

    /// <summary>
    /// Get a string representation of the clip.<br/>
    /// [
    /// </summary>
    /// <param name="_clip"></param>
    /// <returns></returns>
    public static string GetClipString(ClipDefinition _clip)
    {
        if (_clip == null)
        {
            return "_";
        }

        string clipString = "";
        for (int i = 0; i < _clip.MaxSize; i++)
        {
            clipString += ShellDefinition.GetShellString(i < _clip.m_shells.Count ? _clip.m_shells[i] : null);
            clipString += i < _clip.MaxSize - 1 ? "|" : "";
        }
        return clipString;
    }
}
