using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject entity, target;
    [SerializeField]
    public bool active;
    [SerializeField]
    private float frequency;
    [SerializeField]
    private int limit;

    private float deltaSpawn = 0;

    void Update()
    {
        if (active)
        {
            if (deltaSpawn >= frequency && 
                GameObject.FindGameObjectsWithTag("Clone").Length < limit)
            {
                Spawn();
                deltaSpawn = 0;
            }
            deltaSpawn += Time.deltaTime;
        }
    }

    private void Spawn()
    {
        GameObject newEntity = Instantiate(entity);
        newEntity.GetComponent<EnemyController>().target = target;
        newEntity.transform.position = new Vector2(Random.Range(7, 9), Random.Range(3, 5));
    }
}
