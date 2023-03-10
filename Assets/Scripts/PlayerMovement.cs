using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public LayerMask layermask;
    public float moveSpeed;
    public float moveSpeedFalloffFactor;
    public float jumpSpeed;
    public float gravityOnAscent;
    public float gravityOnAscentWhildHoldingJump;
    public float gravityOnDescent;
    public float preJumpLeniencyTime;
    [Range(0f, 1f)] public float ceilingBumpSpeedReduction;
    public float groundedCheckDistance;
    public float maxVSpeed;
    public float preventGroundSnapVspeed;

    private BoxCollider2D boxCollider;
    private bool isGrounded;
    private float jumpQueueTimer;
    private Rigidbody2D rb;
    private Vector2 velocity;

    // TODO coyote timer
    // TODO "almost made it up the ledge" forgiveness
    // TODO bug: jump down to lowest level and jump while standing
    //      still in diff spots. you'll glitch out when landing sometimes

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        var hInput = Input.GetAxisRaw("Horizontal");
        UpdateJumpQueueTimer();
        UpdateGroundedState();

        if (HasInput(hInput)) {
            Vector2 movement = new Vector2(hInput, 0f);
            RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center,
                new Vector2(boxCollider.bounds.size.x, boxCollider.bounds.size.y - 0.1f), 0f, movement, 0.1f,
                layermask);
            if (hit.collider == null) {
                velocity.x = hInput * moveSpeed;
            }
            else {
                velocity.x = 0f;
            }
        }
        else {
            velocity.x = moveTowardZero(velocity.x, moveSpeedFalloffFactor * Time.deltaTime);
        }

        CheckCeilingBump();
        rb.velocity = new Vector2(velocity.x, Mathf.Clamp(rb.velocity.y, -maxVSpeed, maxVSpeed));


        if (IsJumpQueued() && isGrounded) {
            ResetJumpQueueTimer();
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            isGrounded = false;
            PlayerEvents.instance.e_PlayerJump();
        }

        UpdateContextualGravity();
    }

    private void LateUpdate() {
        UpdateSpriteDirection();
    }

    private void UpdateSpriteDirection() {
        float newLocalX;
        if (rb.velocity.x < 0) {
            newLocalX = -1f;
        }
        else if (rb.velocity.x > 0) {
            newLocalX = 1f;
        }
        else {
            newLocalX = transform.localScale.x;
        }

        transform.localScale = new Vector3(newLocalX, 1, 1);
    }

    private void UpdateGroundedState() {
        Bounds colliderBounds = boxCollider.bounds;

        // TODO these raycasts need to come from the player center and stretch out to slightly below the player min y
        //  to avoid phasing through floors at high speeds
        Vector2 bottomLeft = new Vector2(colliderBounds.min.x, colliderBounds.min.y);
        Vector2 bottomRight = new Vector2(colliderBounds.max.x, colliderBounds.min.y);


        RaycastHit2D hitLeft = Physics2D.Raycast(bottomLeft, Vector2.down, groundedCheckDistance, layermask);
        RaycastHit2D hitRight = Physics2D.Raycast(bottomRight, Vector2.down, groundedCheckDistance, layermask);


        if (hitLeft.collider != null || hitRight.collider != null) {
            if (!isGrounded) {
                RaycastHit2D hit = hitLeft.collider == null ? hitRight : hitLeft;
                SnapPlayerToGround(hit);
                Debug.Log($"v_velocity:  {rb.velocity.y}");
            }

            isGrounded = true;
            return;
        }

        isGrounded = false;
    }

    private void SnapPlayerToGround(RaycastHit2D hit) {
        if (rb.velocity.y >= preventGroundSnapVspeed) {
            // FIXME this logic is why i get an animation stutter when barely clearing a jump
            return;
        }

        Bounds colliderBounds = boxCollider.bounds;
        var distFromGround = colliderBounds.min.y - hit.point.y;

        transform.position = new Vector2(transform.position.x, transform.position.y - distFromGround);
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        PlayerEvents.instance.e_PlayerLand();
    }

    private void CheckCeilingBump() {
        Bounds colliderBounds = boxCollider.bounds;

        Vector2 topLeft = new Vector2(colliderBounds.min.x, colliderBounds.max.y);
        Vector2 topRight = new Vector2(colliderBounds.max.x, colliderBounds.max.y);


        var raycastLength = 0.1f;

        RaycastHit2D hitLeft = Physics2D.Raycast(topLeft, Vector2.up, raycastLength, layermask);
        RaycastHit2D hitRight = Physics2D.Raycast(topRight, Vector2.up, raycastLength, layermask);


        if ((hitLeft.collider != null || hitRight.collider != null) && rb.velocity.y > 0) {
            rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y * ceilingBumpSpeedReduction);
        }
    }

    private bool HasInput(float input) {
        return !Mathf.Approximately(input, 0f);
    }

    private float moveTowardZero(float currentValue, float change) {
        var newValue = 0f;
        if (currentValue < 0f) {
            newValue = currentValue + change;
        }
        else if (currentValue > 0f) {
            newValue = currentValue - change;
        }

        if (Mathf.Abs(newValue) < 0.1f) {
            newValue = 0f;
        }

        return newValue;
    }

    private void UpdateContextualGravity() {
        if (isGrounded && rb.gravityScale > 0) {
            rb.gravityScale = 0f;
        }
        else if (!isGrounded) {
            if (rb.velocity.y > 0) {
                rb.gravityScale = Input.GetButton("Jump") ? gravityOnAscentWhildHoldingJump : gravityOnAscent;
            }
            else {
                rb.gravityScale = gravityOnDescent;
            }
        }
    }

    private void UpdateJumpQueueTimer() {
        if (Input.GetButtonDown("Jump")) {
            QueueJump();
        }

        if (jumpQueueTimer > 0f) {
            jumpQueueTimer -= Time.deltaTime;
        }

        if (jumpQueueTimer <= 0f) {
            ResetJumpQueueTimer();
        }
    }

    private void ResetJumpQueueTimer() {
        jumpQueueTimer = 0f;
    }

    private void QueueJump() {
        jumpQueueTimer = preJumpLeniencyTime;
    }

    private bool IsJumpQueued() {
        return jumpQueueTimer > 0f;
    }

    public bool IsGrounded() {
        return isGrounded;
    }
}