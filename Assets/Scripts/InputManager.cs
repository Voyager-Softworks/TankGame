using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputManager
{
    private static InputMap m_inputMap;

    public static InputMap.PlayerLookActions PlayerLook { get { return m_inputMap.PlayerLook; } }
    public static InputMap.PlayerMoveActions PlayerMove { get { return m_inputMap.PlayerMove; } }
    public static InputMap.TankDriveActions TankDrive { get { return m_inputMap.TankDrive; } }

    static InputManager()
    {
        Debug.Log("InputManager | static constructor called");

        if (m_inputMap == null)
        {
            m_inputMap = new InputMap();
        }

        m_inputMap.PlayerLook.Enable();
        m_inputMap.PlayerMove.Enable();
        m_inputMap.TankDrive.Enable();
    }
}
