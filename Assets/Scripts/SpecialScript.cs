using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialScript : MonoBehaviour
{
    [SerializeField]
    private GameObject spawner;

    private EnemyController controller;
    private Spawner spawnerController;

    void Start()
    {
        spawnerController = spawner.GetComponent<Spawner>();
        controller = GetComponent<EnemyController>();
    }

    void Update()
    {
        if (controller.targetInRange)
        {
            spawnerController.active = true;
        }
    }
}
