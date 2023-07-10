using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    [SerializeField]
    private GameObject startPoint, endPoint;
    [SerializeField]
    private float speed;

    private float direction = 1;

    void Start()
    {
        transform.position = new Vector2(startPoint.transform.position.x, startPoint.transform.position.y);
    }

    void Update()
    {
        Vector2 trapPos = transform.position;
        Vector2 startPointPos = startPoint.transform.position, endPointPos = endPoint.transform.position;
        if (trapPos.Equals(endPointPos))
        {
            direction = -1;
            // Debug.Log("Reached end.");
        }
        else if (trapPos.Equals(startPointPos))
        {
            direction = 1;
            // Debug.Log("Reached start.");
        }
        transform.position = Vector2.MoveTowards(transform.position, 
            direction == 1 ? endPointPos : startPointPos, 
            speed * Time.deltaTime);
    }
}
