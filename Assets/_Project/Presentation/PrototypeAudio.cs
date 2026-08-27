using UnityEngine;

namespace IncrementalGame.Presentation
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PrototypeAudio : MonoBehaviour
    {
        private AudioSource _source;
        private AudioClip _fire;
        private AudioClip _hit;
        private AudioClip _ready;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.playOnAwake = false;
            _fire = CreateTone("Fire", 220f, 0.055f, 0.12f);
            _hit = CreateTone("Hit", 520f, 0.08f, 0.14f);
            _ready = CreateTone("Reload Ready", 760f, 0.04f, 0.08f);
        }

        public void PlayFire() => Play(_fire);
        public void PlayHit() => Play(_hit);
        public void PlayReady() => Play(_ready);

        private void Play(AudioClip clip)
        {
            if (_source != null && clip != null)
            {
                _source.PlayOneShot(clip);
            }
        }

        private static AudioClip CreateTone(string clipName, float frequency, float duration, float volume)
        {
            const int sampleRate = 22050;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var index = 0; index < sampleCount; index += 1)
            {
                var t = index / (float)sampleRate;
                var fade = 1f - index / (float)sampleCount;
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * t) * fade * volume;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
