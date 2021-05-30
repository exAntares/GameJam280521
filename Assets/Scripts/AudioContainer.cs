using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using HalfBlind.Audio;
using UnityEngine;

public class AudioContainer : MonoBehaviour {
    [SerializeField] private AudioAsset[] _allAudios;
    public static AudioContainer Instance { get; private set; }
    
    private void Awake() {
        Instance = this;
    }

    public bool PlayAudio(string audioName, Vector3 position) {
        foreach (var audioAsset in _allAudios) {
            if (string.CompareOrdinal(audioAsset.name, audioName) == 0) {
                audioAsset.PlayClipAtPoint(position);
                return true;
            }
        }

        return false;
    }
}
