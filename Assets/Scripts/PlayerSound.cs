using System.Collections;
using UnityEngine;

public class PlayerSound : MonoBehaviour {
    public AudioClip jump;
    public AudioClip land;
    public AudioClip step;
    public float stepLoopTime;

    public float jumpPitchVariation;
    public float landPitchVariation;
    public float stepPitchVariation;
    private bool isMovingOnGround;

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    private void Awake() {
        PlayerEvents.instance.onPlayerJump += OnPlayerJump;
        PlayerEvents.instance.onPlayerLand += OnPlayerLand;

        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(LoopStepSounds());
    }

    private void Update() {
        isMovingOnGround = playerMovement.IsGrounded() && !Mathf.Approximately(rb.velocity.x, 0f);
    }

    private void OnDestroy() {
        StopCoroutine(LoopStepSounds());
    }

    private void OnPlayerJump() {
        AudioController.instance.PlaySoundVaryPitch(jump, jumpPitchVariation);
    }

    private void OnPlayerLand() {
        AudioController.instance.PlaySoundVaryPitch(land, landPitchVariation);
    }

    private IEnumerator LoopStepSounds() {
        while (true) {
            if (isMovingOnGround) {
                AudioController.instance.PlaySoundVaryPitch(step, stepPitchVariation);
                yield return new WaitForSeconds(stepLoopTime);
            }
            else {
                yield return new WaitForSeconds(stepLoopTime);
            }
        }
    }
}