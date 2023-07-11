using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private float AOE, speed;
    [SerializeField]
    public GameObject target, aggressionCircle;
    [SerializeField]
    private Color aggressive, unaggressive;
    [SerializeField]
    private bool horizontalOnly = false;

    private SpriteRenderer spriteRenderer, aggressionCircleSpriteRenderer;
    private Rigidbody2D rigidBody;

    private Vector2 spawn, lastPos;
    private bool returnToSpawn = false;
    public bool targetInRange = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        aggressionCircleSpriteRenderer = aggressionCircle.GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();
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
            target.GetComponent<PlayerController>().TakeDamage(1, transform.position);
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
        if (!horizontalOnly)
        {
            transform.position = Vector2.MoveTowards(transform.position, spawn, speed * Time.deltaTime);
        }
        else
        {
            float horizontalDiff = spawn.x - transform.position.x;
            if (horizontalDiff > 0)
            {
                rigidBody.velocity = new Vector2(speed, rigidBody.velocity.y);
            }
            else if (horizontalDiff < 0)
            {
                rigidBody.velocity = new Vector2(-speed, rigidBody.velocity.y);
            }
            else
            {
                rigidBody.velocity = new Vector2(0, rigidBody.velocity.y);
            }
        }
    }

    private void MoveToTarget()
    {
        lastPos = transform.position;
        if (!horizontalOnly)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        }
        else
        {
            float horizontalDiff = target.transform.position.x - transform.position.x;
            if (horizontalDiff > 0)
            {
                rigidBody.velocity = new Vector2(speed, rigidBody.velocity.y);
            }
            else if (horizontalDiff < 0)
            {
                rigidBody.velocity = new Vector2(-speed, rigidBody.velocity.y);
            }
            else
            {
                rigidBody.velocity = new Vector2(0, rigidBody.velocity.y);
            }
        }
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

    public void ReturnToSpawnInsant()
    {
        transform.position = new Vector2(spawn.x, spawn.y);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.Equals(target))
        {
            targetInRange = true;
            returnToSpawn = false;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.Equals(target))
        {
            targetInRange = false;
            returnToSpawn = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (horizontalOnly && 
           (collision.gameObject.CompareTag("Death") || 
            collision.gameObject.CompareTag("Player")))
        {
            // Fall out of map, otherwise it rests on the death zone. The player could then land on it...
            // It would be better to make the death zone a trigger, but that would require more refactoring.
            GetComponent<CapsuleCollider2D>().enabled = false;
        }
        else if (horizontalOnly && collision.gameObject.CompareTag("Trap"))
        {
            // Temporary solution to prevent being pushed around by trap.
            rigidBody.velocity = Vector2.zero;
            rigidBody.simulated = false;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (horizontalOnly && collision.gameObject.CompareTag("Trap"))
        {
            // Temporary solution to prevent being pushed around by trap.
            rigidBody.velocity = Vector2.zero;
            rigidBody.simulated = true;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 circlePos = new(transform.position.x, transform.position.y);
        Gizmos.DrawWireSphere(circlePos, AOE);
    }

}
