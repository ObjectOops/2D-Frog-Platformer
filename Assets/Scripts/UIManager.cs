using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerHealthUI;

    public void SetHealth(int health)
    {
        int i = 0;
        foreach (Transform heart in playerHealthUI.transform)
        {
            heart.gameObject.SetActive(i < health);
            ++i;
        }
    }
}
