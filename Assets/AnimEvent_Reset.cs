using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimEvent_Reset : MonoBehaviour
{
    public void OnAnimationEvent()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
