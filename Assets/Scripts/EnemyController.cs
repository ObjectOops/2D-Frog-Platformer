using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private float AOE, speed;
    [SerializeField]
    GameObject target, aggressionCircle;
    [SerializeField]
    Color aggressive, unaggressive;

    SpriteRenderer spriteRenderer, aggressionCircleSpriteRenderer;

    private Vector2 spawn, lastPos;
    private bool returnToSpawn = false, targetInRange;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        aggressionCircleSpriteRenderer = aggressionCircle.GetComponent<SpriteRenderer>();
        aggressionCircleSpriteRenderer.color = unaggressive;
        spawn = new Vector2(transform.position.x, transform.position.y);
    }

    void Update()
    {
        bool atSpawn = transform.position.Equals(spawn);
        float distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
        if (targetInRange)
        {
            MoveToTarget();
            OrientSprite();
            aggressionCircleSpriteRenderer.color = aggressive;
        }
        if (distanceToTarget <= AOE)
        {
            target.GetComponent<PlayerController>().Respawn();
            transform.position = new Vector2(spawn.x, spawn.y);
        }
        if (returnToSpawn)
        {
            MoveToSpawn();
            OrientSprite();
            aggressionCircleSpriteRenderer.color = unaggressive;
        }
        if (atSpawn)
        {
            returnToSpawn = false;
        }
    }

    private void MoveToSpawn()
    {
        lastPos = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, spawn, speed * Time.deltaTime);
    }

    private void MoveToTarget()
    {
        lastPos = transform.position;
        transform.position = Vector2.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
    }

    private void OrientSprite()
    {
        if (transform.position.x > lastPos.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.Equals(target))
        {
            targetInRange = true;
            returnToSpawn = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.Equals(target))
        {
            targetInRange = false;
            returnToSpawn = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 circlePos = new(transform.position.x, transform.position.y);
        Gizmos.DrawWireSphere(circlePos, AOE);
    }

}
