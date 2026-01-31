using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GGJ2026
{
    public class AudioManager : MonoBehaviour {
        public static AudioManager Instance;

        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioClip[] clips; // 存储所有音效

        void Awake() { Instance = this; }

        public void PlaySFX(string name) {
            AudioClip s = System.Array.Find(clips, x => x.name == name);
            if (s != null) sfxSource.PlayOneShot(s);
        }

        public void ToggleMusic() {
            musicSource.mute = !musicSource.mute;
        }
    }
}
