using UnityEngine;

public class AudioController : MonoBehaviour {
    private float basePitch;
    private AudioSource source;
    public static AudioController instance { get; private set; }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(this);
            return;
        }

        instance = this;

        source = GetComponent<AudioSource>();
        basePitch = source.pitch;
    }

    public void PlaySoundVaryPitch(AudioClip sound, float variation) {
        source.pitch = basePitch + Random.Range(-variation, variation);
        source.PlayOneShot(sound);
    }
}