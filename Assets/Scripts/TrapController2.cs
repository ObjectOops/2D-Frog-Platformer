using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController2 : MonoBehaviour
{
    [SerializeField]
    List<GameObject> points;

    void Start()
    {
        transform.position = new Vector2(points[0].transform.position.x, points[0].transform.position.y);
    }

    void Update()
    {
        
    }
}
