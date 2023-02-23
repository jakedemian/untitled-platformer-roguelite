using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public LayerMask layermask;
    public float moveSpeed;
    public float moveSpeedFalloffFactor;
    public float jumpSpeed;
    [Range(0f, 1f)]
    public float ceilingBumpSpeedReduction;
    // TODO preJumpLeniencyTime = 0.2f;
    
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;
    private Vector2 velocity;
    private bool isGrounded;

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        float hInput = Input.GetAxisRaw("Horizontal");
        bool jumpInput = Input.GetButtonDown("Jump");
        UpdateGroundedState();


        if (HasInput(hInput)) {
            Vector2 movement = new Vector2(hInput, 0f);
            RaycastHit2D hit = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(boxCollider.bounds.size.x, boxCollider.bounds.size.y - 0.1f), 0f, movement, 0.1f, layermask);
            if (hit.collider == null) {
                velocity.x = hInput * moveSpeed;
            } else {
                velocity.x = 0f;
            }
        }
        else {
            velocity.x = moveTowardZero(velocity.x, moveSpeedFalloffFactor * Time.deltaTime);
        }

        CheckCeilingBump();
        // TODO max y velocity
        rb.velocity = new Vector2(velocity.x, rb.velocity.y);

        if (jumpInput && isGrounded) {
            rb.AddForce(Vector2.up * jumpSpeed);
            isGrounded = false;
        }
    }

    private void LateUpdate() {
        float newLocalX;
        if (rb.velocity.x < 0) {
            newLocalX = -1f;
        } else if (rb.velocity.x > 0) {
            newLocalX = 1f;
        } else {
            newLocalX = transform.localScale.x;
        }
        transform.localScale = new Vector3(newLocalX, 1, 1);
        
        UpdateGravity();
    }

    private void UpdateGroundedState() {
        var colliderBounds = boxCollider.bounds;
        
        // TODO these raycasts need to come from the player center and stretch out to slightly below the player min y
        //  to avoid phasing through floors at high speeds
        Vector2 bottomLeft = new Vector2(colliderBounds.min.x, colliderBounds.min.y);
        Vector2 bottomRight = new Vector2(colliderBounds.max.x, colliderBounds.min.y);
        

        float raycastLength = 0.1f;

        RaycastHit2D hitLeft = Physics2D.Raycast(bottomLeft, Vector2.down, raycastLength, layermask);
        RaycastHit2D hitRight = Physics2D.Raycast(bottomRight, Vector2.down, raycastLength, layermask);
        

        if (hitLeft.collider != null || hitRight.collider != null ){
            if (!isGrounded) {
                RaycastHit2D hit = hitLeft.collider == null ? hitRight : hitLeft;
                SnapPlayerToGround(hit);
            }
            
            isGrounded = true;
            return;
        }

        isGrounded = false;
    }
    
    private void SnapPlayerToGround(RaycastHit2D hit) {
        if (rb.velocity.y >= 0f) {
            return;
        }
        
        var colliderBounds = boxCollider.bounds;
        var distFromGround = colliderBounds.min.y - hit.point.y;
        
        transform.position = new Vector2(transform.position.x, transform.position.y - distFromGround);
        rb.velocity = new Vector2(rb.velocity.x, 0f);
    }

    private void CheckCeilingBump() {
        var colliderBounds = boxCollider.bounds;
        
        Vector2 topLeft = new Vector2(colliderBounds.min.x, colliderBounds.max.y);
        Vector2 topRight = new Vector2(colliderBounds.max.x, colliderBounds.max.y);
        

        float raycastLength = 0.1f;

        RaycastHit2D hitLeft = Physics2D.Raycast(topLeft, Vector2.up, raycastLength, layermask);
        RaycastHit2D hitRight = Physics2D.Raycast(topRight, Vector2.up, raycastLength, layermask);
        

        if ((hitLeft.collider != null || hitRight.collider != null) && rb.velocity.y > 0 ) {
            rb.velocity = new Vector2(rb.velocity.x, -rb.velocity.y / ceilingBumpSpeedReduction );
        }
    }

    private bool HasInput(float input) {
        return !Mathf.Approximately(input, 0f);
    }

    private float moveTowardZero(float currentValue, float change) {
        float newValue = 0f;
        if (currentValue < 0f) {
            newValue = currentValue + change;
        } else if (currentValue > 0f) {
            newValue = currentValue - change;
        }

        if (Mathf.Abs(newValue) < 0.1f) {
            newValue = 0f;
        }

        return newValue;
    }

    private void UpdateGravity() {
        if (isGrounded && rb.gravityScale > 0) {
            rb.gravityScale = 0f;
        } else if(!isGrounded && Mathf.Approximately(rb.gravityScale, 0f)) {
            rb.gravityScale = 1f;
        }
    }
}
