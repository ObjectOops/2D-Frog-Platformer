using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

/*
 * 
 * This code is not properly modularized.
 * Refactoring is a later objective at the moment. Variable and tag names may no longer reflect behavior.
 * Also some unecessary code was kept during migration from the old to new input system.
 * 
 * Known Bugs:
 * 1. Because the player collider is a capsule, 
 *    if the player dashes into a tilemap corner, it gives them extra vertical velocity.
 * 2. When dashing into a sticky wall, 
 *    if the movement direction changes to be the opposite direction of the wall during the dash, 
 *    the animated sprite renders the wrong way.
 * 3. Enemies will not attack the player when sticking to a wall.
 * 
*/

public class PlayerController : MonoBehaviour
{
    // Adjustable Parameters
    [SerializeField]
    private float speed, jumpForce, doubleJumpForce, sprintSpeed;
    [SerializeField]
    private float dashForce, dashForceVertical, dashCoolDown, dashVelocityThreshold, dashSlowFactor;
    [SerializeField]
    private float stickCoolDown;
    [SerializeField]
    private float onDamageKnockback;

    // UI and Health
    [SerializeField]
    private GameObject ui;
    [SerializeField]
    private int health;
    [SerializeField]
    private float invulnerableTime;
    [SerializeField]
    private GameObject preTransitionMessage;

    // Player Components
    private Rigidbody2D rigidBody;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private new CapsuleCollider2D collider;
    [SerializeField]
    private AudioSource hitSound, runSound, jumpSound, dashSound;

    // Inputs
    private float inputHorizontal, inputHorizontalRaw;
    private bool jumpNow, sprintNow, dashNow;

    // Ground Detection
    // [SerializeField]
    // private float castLength;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private float checkRadius;
    [SerializeField]
    private float checkOffsetY;

    // Additional Variables
    [SerializeField]
    private float velocityAnimationThreshold;

    // Mutable Variables
    private bool onGround, onWall = false;
    private bool doubleJumpNow = false;
    private bool doubleJumpAnimationComplete = false;
    private float deltaDash = 0;
    private bool disableControl = false;
    private float playerWidth;
    private float deltaStick = 0;
    private float wallDirection = 0;
    private bool wasHit = false;
    private float respawnX, respawnY;
    private bool transition = false, isWin;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<CapsuleCollider2D>();
        playerWidth = collider.size.x;
        respawnX = transform.position.x;
        respawnY = transform.position.y;
    }

    void Update()
    {
        if (transition)
        {
            if (isWin)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                SceneManager.LoadScene("LoseScene");
            }
        }

        // inputHorizontal = Input.GetAxis("Horizontal");
        // inputHorizontalRaw = Input.GetAxisRaw("Horizontal");
        // jumpNow = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
        // sprintNow = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        // dashNow = Input.GetKeyDown(KeyCode.Space);

        // Ground detection.
        // onGround = Physics2D.Raycast(transform.position, Vector2.down, castLength, groundLayer);
        // Debug.DrawRay(transform.position, Vector2.down * castLength, Color.red);
        Vector2 circlePos = new(transform.position.x, transform.position.y - checkOffsetY);
        onGround = Physics2D.OverlapCircle(circlePos, checkRadius, groundLayer);
        // Debug.Log("On Ground: " + onGround);

        bool isRunning = onGround && Mathf.Abs(rigidBody.velocity.x) > velocityAnimationThreshold;
        bool isJumping = !onGround && rigidBody.velocity.y > velocityAnimationThreshold;
        bool isFalling = !onGround && rigidBody.velocity.y < -velocityAnimationThreshold;
        bool isDoubleJumping = doubleJumpNow && !doubleJumpAnimationComplete;
        bool isSprinting = isRunning && sprintNow;
        bool isDashing = Mathf.Abs(rigidBody.velocity.x) > dashVelocityThreshold;

        deltaDash += Time.deltaTime;
        deltaStick += Time.deltaTime;

        // Reset double jump upon contact with ground or sticking to wall.
        doubleJumpNow = !(onGround || onWall) && doubleJumpNow;

        animator.SetBool("running", isRunning);
        animator.SetBool("jumping", isJumping);
        animator.SetBool("falling", isFalling);
        animator.SetBool("doubleJumping", isDoubleJumping);
        animator.SetBool("sprinting", isSprinting);
        animator.SetBool("dashing", isDashing);

        animator.SetBool("perching", onWall);
        animator.SetBool("hurting", wasHit);

        OrientSprite();

        // Debug.Log("Latency: " + Time.deltaTime);
        // Debug.Log("Velocity: " + rigidBody.velocity);
    }

    void FixedUpdate()
    {
        if (!disableControl)
        {
            if (inputHorizontalRaw != 0 && !runSound.isPlaying && onGround)
            {
                runSound.Play();
            }

            float realSpeed = sprintNow ? sprintSpeed : speed; // Sprint.
            rigidBody.velocity = new Vector2(inputHorizontal * realSpeed, rigidBody.velocity.y);
        }
        else if (!wasHit)
        {
            float currentvx = rigidBody.velocity.x;
            float newvx = currentvx > 0 ? currentvx - dashSlowFactor : currentvx + dashSlowFactor;
            rigidBody.velocity = new Vector2(newvx, rigidBody.velocity.y);
            if (Mathf.Abs(rigidBody.velocity.x) < dashVelocityThreshold)
            {
                disableControl = false;
            }
        }
    }

    private void Jump()
    {
        rigidBody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
    }

    private void DoubleJump()
    {
        rigidBody.AddForce(new Vector2(0, doubleJumpForce), ForceMode2D.Impulse);
    }

    private void Dash()
    {
        Vector2 newvxy = new(spriteRenderer.flipX ? -dashForce : dashForce, dashForceVertical);
        rigidBody.AddForce(newvxy, ForceMode2D.Impulse);
        disableControl = true;
    }

    private void StickToWall()
    {
        if (deltaStick >= stickCoolDown)
        {
            rigidBody.velocity = new Vector2(0, 0);
            rigidBody.simulated = false;
            // rigidBody.bodyType = RigidbodyType2D.Static;
            onWall = true;
            spriteRenderer.flipX = wallDirection != 1;
        }
    }

    private void UnStickToWall()
    {
        rigidBody.simulated = true;
        // rigidBody.bodyType = RigidbodyType2D.Dynamic;
        onWall = false;
        deltaStick = 0;
    }

    private void OrientSprite()
    {
        if (inputHorizontal > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (inputHorizontal < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

/*    public void Respawn()
    {
        // Just reload scene for now.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        *//*        transform.position = new Vector3(respawnX, respawnY, transform.position.z);*/
        /*        ResetEnemies();*//*
    }*/

    public void TakeDamage(int damage, Vector2 damagerPos)
    {
        if (wasHit)
        {
            return;
        }

        hitSound.Play();

        wasHit = disableControl = true;
        health -= damage;
        ui.GetComponent<UIManager>().SetHealth(health);

        // Debug.Log("Player: " + transform.position + ", Damage: " + damagerPos);
        rigidBody.velocity = Vector2.zero;

        if (!damagerPos.Equals(Vector2.zero))
        {
            // float angle = Mathf.Deg2Rad * Vector2.SignedAngle(damagerPos, transform.position);
            float angle = Mathf.Atan2(transform.position.y - damagerPos.y, transform.position.x - damagerPos.x);
            // Debug.Log(Mathf.Rad2Deg * angle);
            rigidBody.AddForce(new Vector2(onDamageKnockback * Mathf.Cos(angle), onDamageKnockback * Mathf.Sin(angle)), 
                               ForceMode2D.Impulse);
        }

        StartCoroutine(WasHitPauseTime(invulnerableTime));

        if (health <= 0)
        {
            isWin = false;
            SetTransitionMessage("Lose");
            StartCoroutine(WaitBeforeTransitioning(2));
        }
    }

    private void SetTransitionMessage(string result)
    {
        StartCoroutine(WaitBeforeTransitioning(2));
        preTransitionMessage.SetActive(true);
        switch (result)
        {
            case "Win":
                preTransitionMessage.GetComponent<TextMeshProUGUI>().text = "Yippee!";
                break;
            case "Lose":
                preTransitionMessage.GetComponent<TextMeshProUGUI>().text = "Noo...";
                break;
        }
    }

    private IEnumerator WasHitPauseTime(float seconds)
    {
        // Give the player a chance to escape while still invulnerable.
        yield return new WaitForSeconds(seconds / 2);
        disableControl = false;
        yield return new WaitForSeconds(seconds / 2);
        wasHit = false;
    }

    private IEnumerator WaitBeforeTransitioning(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        transition = true;
    }

    private void ResetEnemies()
    {
        GameObject[] startEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] clones = GameObject.FindGameObjectsWithTag("Clone");
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Enemy Spawner");

        // Debug.Log("Reset Enemies Things Found: " + startEnemies.Length + ' ' + clones.Length + ' ' + spawners.Length);

        foreach (GameObject enemy in startEnemies)
        {
            EnemyController ec = enemy.GetComponent<EnemyController>();
            ec.ReturnToSpawnInsant();
            ec.targetInRange = false;
        }
        foreach (GameObject spawner in spawners)
        {
            spawner.GetComponent<Spawner>().active = false;

            // Debug.Log("Disabled spawner.");
        }
        foreach (GameObject clone in clones)
        {
            Destroy(clone);
        }
    }

    // Called by an Animation Event at the end.
    public void CompleteDoubleJumpAnimation()
    {
        doubleJumpAnimationComplete = true;
        // Debug.Log("Double jump animation complete.");
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        Vector2 point = other.GetContact(0).point;
        float pointX = point[0], pointY = point[1];
        if (other.gameObject.CompareTag("Goal") && pointY < transform.position.y)
        {
            // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            isWin = true;
            health = 100; // In case the player is attacked after reaching the goal.
            SetTransitionMessage("Win");
            StartCoroutine(WaitBeforeTransitioning(2));
            // Debug.Log("Win!: " + pointY + " " + transform.position.y);
        }
        else if (other.gameObject.CompareTag("Death"))
        {
            transform.position = new Vector2(respawnX, respawnY);
            TakeDamage(1, Vector2.zero);
            ResetEnemies();
            // Debug.Log("Died.");
        }
        else if (other.gameObject.CompareTag("Trap"))
        {
            TakeDamage(1, other.gameObject.transform.position);
        }
        else if (other.gameObject.CompareTag("StaticTrap"))
        {
            TakeDamage(1, new Vector2(transform.position.x + Random.Range(-2, 2), transform.position.y - 1));
        }
        else if (other.gameObject.CompareTag("Sticky") && 
                 Mathf.Abs(pointX - transform.position.x) > playerWidth / 4)
        {
            StickToWall();
            wallDirection = pointX > transform.position.x ? 1 : -1;
            // Debug.Log("Stuck to wall.");
        }
    }

    public void OnMove(InputValue action)
    {
        Vector2 values = action.Get<Vector2>();
        inputHorizontal = values.x;
        inputHorizontalRaw = values.x == 0 ? 0 : (values.x > 0 ? 1 : -1);

        if (onWall && inputHorizontalRaw != wallDirection && inputHorizontalRaw != 0) // Unstick with movement.
        {
            UnStickToWall();
            // Debug.Log("Unstuck: Moving");
        }

        // Debug.Log("Movement detected.");
    }
    public void OnJump()
    {
        jumpNow = true;
        if (Keyboard.current.wKey.wasPressedThisFrame || 
            Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            if ((onGround || onWall) && jumpNow) // Jump.
            {
                if (onWall)
                {
                    UnStickToWall();
                }

                if (!jumpSound.isPlaying)
                {
                    jumpSound.Play();
                }

                Jump();
                doubleJumpAnimationComplete = false;
                // Debug.Log("Jumped");
            }
            else if (!onGround && jumpNow && !doubleJumpNow) // Double jump.
            {
                DoubleJump();
                doubleJumpNow = true;
                // Debug.Log("Double Jumped");
            }
        }
    }
    public void OnSprint()
    {
        sprintNow = Keyboard.current.shiftKey.isPressed;
    }
    public void OnDash()
    {
        dashNow = true;
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (dashNow && deltaDash >= dashCoolDown/* && !isDoubleJumping*/) // Dash.
            {
                if (onWall)
                {
                    UnStickToWall();
                }

                if (!dashSound.isPlaying)
                {
                    dashSound.Play();
                }

                Dash();
                deltaDash = 0;
                // Debug.Log("Dashed");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 circlePos = new(transform.position.x, transform.position.y - checkOffsetY);
        Gizmos.DrawWireSphere(circlePos, checkRadius);
    }
}
