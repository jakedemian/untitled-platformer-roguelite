using UnityEngine;

public class PlayerAnimation : MonoBehaviour {
    private static readonly int isMovingHorizontally = Animator.StringToHash("isMovingHorizontally");
    private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
    private Animator animator;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    private void Awake() {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void LateUpdate() {
        animator.SetBool(isMovingHorizontally, !Mathf.Approximately(rb.velocity.x, 0));
        animator.SetBool(IsGrounded, playerMovement.IsGrounded());
    }
}