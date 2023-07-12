using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialMushroomScript : MonoBehaviour
{
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private float launchForce;

    private Rigidbody2D rigidBody;
    private bool launched = false;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!launched && collision.gameObject.Equals(target))
        {
            float angle = Mathf.Atan2(target.transform.position.y - transform.position.y, 
                                      target.transform.position.x - transform.position.x);
            rigidBody.AddForce(new Vector2(launchForce * Mathf.Cos(angle), launchForce * Mathf.Sin(angle)),
                               ForceMode2D.Impulse);
            launched = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.Equals(target))
        {
            target.GetComponent<PlayerController>().TakeDamage(1, transform.position);
            Destroy(gameObject);
        }
    }
}
