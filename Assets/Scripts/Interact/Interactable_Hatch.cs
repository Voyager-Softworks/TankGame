using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Hatch : Interactable
{
    public override void OnInteract(Interacter _interacter)
    {
        base.OnInteract(_interacter);

        Tank.Instance.OnPlayerEnter();
        Player.Instance.DisablePlayer();
    }
}
