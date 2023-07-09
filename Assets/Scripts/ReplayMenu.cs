using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayMenu : MonoBehaviour
{
    // Called on replay button press.
    public void ReplayGame()
    {
        SceneManager.LoadScene(1);
    }
}
