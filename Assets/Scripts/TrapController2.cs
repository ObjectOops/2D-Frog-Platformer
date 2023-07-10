using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController2 : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> points;
    [SerializeField]
    private int index;
    [SerializeField]
    private float speed;
    [SerializeField]
    private bool loop; // Goes back and forth when false.

    private int direction = 1;

    void Start()
    {
        transform.position = new Vector2(points[0].transform.position.x, points[0].transform.position.y);
    }

    void Update()
    {
        Vector2 trapPos = transform.position;
        Vector2 nextPointPos = points[index].transform.position;
        if (trapPos.Equals(nextPointPos))
        {
            if (loop && index == points.Count - 1)
            {
                index = -1;
            }
            else if (!loop && index == points.Count - 1)
            {
                direction = -1;
            }
            else if (!loop && index == 0)
            {
                direction = 1;
            }

            if (loop)
            {
                ++index;
            }
            else
            {
                index += direction;
            }
        }
        transform.position = Vector2.MoveTowards(transform.position, nextPointPos, speed * Time.deltaTime);
    }
}
