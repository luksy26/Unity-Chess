using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] audioClips;

    public void PlaySound(string action) {
        if (!Game.Instance.soundActive) {
            return; // sound is turned off
        }
        switch(action) {
            case "move" : audioSource.clip = audioClips[0]; break;
            case "capture": audioSource.clip = audioClips[1]; break;
            case "check": audioSource.clip = audioClips[2]; break;
            case "promote": audioSource.clip = audioClips[3]; break;
            case "start": audioSource.clip = audioClips[4]; break;
            case "end": audioSource.clip = audioClips[5]; break;
            case "correct": audioSource.clip = audioClips[6]; break;
            case "castle": audioSource.clip = audioClips[7]; break;
            default: break;
        }
        audioSource.Play();
    }
}
