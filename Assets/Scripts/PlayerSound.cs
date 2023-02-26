using UnityEngine;

public class PlayerSound : MonoBehaviour {
    public AudioClip jump;
    public AudioClip land;
    public AudioClip step;

    public float jumpPitchVariation;
    public float landPitchVariation;

    private void Awake() {
        PlayerEvents.instance.onPlayerJump += OnPlayerJump;
        PlayerEvents.instance.onPlayerLand += OnPlayerLand;
    }

    private void OnPlayerJump() {
        AudioController.instance.PlaySoundVaryPitch(jump, jumpPitchVariation);
    }

    private void OnPlayerLand() {
        AudioController.instance.PlaySoundVaryPitch(land, landPitchVariation);
    }
}