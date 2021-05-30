using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace HalfBlind.Audio {
    [CreateAssetMenu(menuName = "HalfBlind/Audio/" + nameof(AudioAsset), fileName = nameof(AudioAsset))]
    public class AudioAsset : ScriptableObject {
        public enum PlayPolicy {
            PlayAgain,
            DontPlay,
            OverridePrevious,
        }
        
        [SerializeField] private AudioClip[] _variations = null;
        [SerializeField] private float _volume = 1;
        [SerializeField, MinMaxSlider(0,2)] private Vector2 _pitchMinMax = Vector2.one;
#if !ODIN_INSPECTOR
        [Tooltip("Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.")]
#else
        [InfoBox("Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.")]
#endif
        [SerializeField] private float _spatialBlend;
        [SerializeField] private AudioMixerGroup _group = null;
        [SerializeField] private PlayPolicy _whenMultipleSameSound = PlayPolicy.PlayAgain;
        [SerializeField] private float _minDistance = 1.0f;
        [SerializeField] private float _maxDistance = 100.0f;
        [SerializeField, Range(0, 1)] private float _skipProbability = 0.0f;
        
        [SerializeReference, ShowInInspector] private IAudioAssetVariationsProvider _variationsProvider = new SequenceProvider();
        
        public PlayPolicy WhenMultipleSameSound => _whenMultipleSameSound;
        public float GetRandomPitch() => Random.Range(_pitchMinMax.x, _pitchMinMax.y);

        public AudioClip GetNext() => _variationsProvider != null
            ? _variationsProvider.GetNext(_variations)
            : _variations[Random.Range(0, _variations.Length)];

        public void ApplyToAudioSource(AudioSource source) {
            source.clip = GetNext();
            source.pitch = GetRandomPitch();
            source.volume = _volume;
            source.outputAudioMixerGroup = _group;
            source.spatialBlend = _spatialBlend;
            source.minDistance = _minDistance;
            source.maxDistance = _maxDistance;
        }

        public bool ShouldSkip() {
            return Random.Range(0.0f, 1.0f) < _skipProbability;
        }
        
#if UNITY_EDITOR
        private static AudioSource _audioSource;
        
        [Button]
        public void Preview() {
            if (_audioSource == null) {
                var player = new GameObject {hideFlags = HideFlags.HideAndDontSave};
                _audioSource = player.AddComponent<AudioSource>();
            }
            ApplyToAudioSource(_audioSource);
            _audioSource.Play();
        }
#endif
    }
}

#if !ODIN_INSPECTOR
namespace Sirenix.OdinInspector {
    public class MinMaxSlider : System.Attribute { public MinMaxSlider(float minValue, float maxValue) { } }
    public class Button : System.Attribute { }
    public class ShowInInspector : System.Attribute { }
}
#endif
