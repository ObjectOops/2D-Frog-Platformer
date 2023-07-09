using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Adjustable Parameters
    [SerializeField]
    private float speed, jumpForce, doubleJumpForce, sprintSpeed;
    [SerializeField]
    private float dashForce, dashForceVertical, dashCoolDown, dashVelocityThreshold, dashSlowFactor;
    [SerializeField]
    private float stickCoolDown;

    // Player Components
    private Rigidbody2D rigidBody;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private new CapsuleCollider2D collider;

    // Inputs
    private float inputHorizontal, previousInputHorizontal, inputHorizontalRaw;
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
    private float respawnX, respawnY;
    private bool doubleJumpNow = false;
    private bool doubleJumpAnimationComplete = false;
    private float deltaDash = 0;
    private bool disableControl = false;
    private float playerWidth;
    private float deltaStick = 0;
    private float wallDirection = 0;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<CapsuleCollider2D>();
        respawnX = transform.position.x;
        respawnY = transform.position.y;
        playerWidth = collider.size.x;
    }

    void Update()
    {
        previousInputHorizontal = inputHorizontal;
        inputHorizontal = Input.GetAxis("Horizontal");
        inputHorizontalRaw = Input.GetAxisRaw("Horizontal");
        jumpNow = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
        sprintNow = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        dashNow = Input.GetKeyDown(KeyCode.Space);

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

        if ((onGround || onWall) && jumpNow) // Jump.
        {
            if (onWall)
            {
                UnStickToWall();
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

        if (dashNow && deltaDash >= dashCoolDown && !isDoubleJumping) // Dash.
        {
            if (onWall)
            {
                UnStickToWall();
            }
            Dash();
            deltaDash = 0;
            // Debug.Log("Dashed");
        }

        if (onWall && inputHorizontalRaw != wallDirection && inputHorizontalRaw != 0) // Unstick with movement.
        {
            UnStickToWall();
            // Debug.Log("Unstuck: Moving");
        }

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

        OrientSprite();

        // Debug.Log("Latency: " + Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (!disableControl)
        {
            float realSpeed = sprintNow ? sprintSpeed : speed; // Sprint.
            rigidBody.velocity = new Vector2(inputHorizontal * realSpeed, rigidBody.velocity.y);
        }
        else
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
            onWall = true;
            spriteRenderer.flipX = wallDirection != 1;
        }
    }

    private void UnStickToWall()
    {
        rigidBody.simulated = true;
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

    private void Respawn()
    {
        transform.position = new Vector3(respawnX, respawnY, transform.position.z);
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            // Debug.Log("Win!: " + pointY + " " + transform.position.y);
        }
        else if (other.gameObject.CompareTag("Death"))
        {
            Respawn();
            // Debug.Log("Died.");
        }
        else if (other.gameObject.CompareTag("Sticky") && 
                 Mathf.Abs(pointX - transform.position.x) > playerWidth / 4)
        {
            StickToWall();
            wallDirection = pointX > transform.position.x ? 1 : -1;
            // Debug.Log("Stuck to wall.");
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 circlePos = new(transform.position.x, transform.position.y - checkOffsetY);
        Gizmos.DrawWireSphere(circlePos, checkRadius);
    }
}
