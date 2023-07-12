using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelAndTimerScript : MonoBehaviour
{
    [SerializeField]
    private GameObject level, timer;
    private TextMeshProUGUI timeUI;

    private float time = 0;

    void Start()
    {
        timeUI = timer.GetComponent<TextMeshProUGUI>();
        level.GetComponent<TextMeshProUGUI>().text = "Level: " + SceneManager.GetActiveScene().buildIndex;
    }

    void FixedUpdate()
    {
        timeUI.text = $"Time Elapsed: {(int)(time * 100) / 100f}";
        time += Time.fixedDeltaTime;
    }
}
