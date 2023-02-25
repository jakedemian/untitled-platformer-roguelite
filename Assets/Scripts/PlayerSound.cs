using UnityEngine;
using UnityEngine.Serialization;

public class PlayerSound : MonoBehaviour {
    public AudioClip jump;
    public AudioClip land;
    public AudioClip step;

    [FormerlySerializedAs("jumpVariation")]
    public float jumpPitchVariation;

    private void Awake() {
        PlayerEvents.instance.onPlayerJump += OnPlayerJump;
        PlayerEvents.instance.onPlayerLand += OnPlayerLand;
    }

    private void OnPlayerJump() {
        AudioController.instance.PlaySoundVaryPitch(jump, jumpPitchVariation);
    }

    private void OnPlayerLand() {
        AudioController.instance.PlaySoundVaryPitch(land, jumpPitchVariation);
    }
}