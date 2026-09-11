using SmashThePast.Characters;
using SmashThePast.Core;
using SmashThePast.Stages;
using UnityEngine;

namespace SmashThePast.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PrototypeAudio : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField, Range(0, 1)] private float volume = 0.45f;

        private AudioSource source;
        private Fighter boundOne;
        private Fighter boundTwo;
        private AudioClip lightClip;
        private AudioClip heavyClip;
        private AudioClip hitClip;
        private AudioClip specialClip;
        private AudioClip knockoutClip;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            if (source == null) return;
            source.playOnAwake = false;
            source.spatialBlend = 0;
            lightClip = Tone("Golpe ligero", 680, 420, 0.08f, 0.28f);
            heavyClip = Tone("Golpe fuerte", 250, 110, 0.14f, 0.36f);
            hitClip = Tone("Impacto", 150, 65, 0.1f, 0.42f);
            specialClip = Tone("Special", 300, 900, 0.34f, 0.3f);
            knockoutClip = Tone("K.O.", 180, 48, 0.48f, 0.38f);
        }

        private void Start() => BindCurrentFighters();

        private void Update()
        {
            if (match == null) return;
            if (match.PlayerOne != boundOne || match.PlayerTwo != boundTwo)
                BindCurrentFighters();
        }

        private void OnDisable() => Unbind();

        private void BindCurrentFighters()
        {
            Unbind();
            boundOne = match == null ? null : match.PlayerOne;
            boundTwo = match == null ? null : match.PlayerTwo;
            Bind(boundOne);
            Bind(boundTwo);
        }

        private void Bind(Fighter fighter)
        {
            if (fighter == null) return;
            fighter.OnLightAttackPerformed += PlayLight;
            fighter.OnHeavyAttackPerformed += PlayHeavy;
            fighter.OnSpecialUsed += PlaySpecial;
            fighter.OnKnockedOut += PlayKnockout;
            var detector = fighter.GetComponent<HitDetector>();
            if (detector != null) detector.HitLanded += PlayHit;
        }

        private void Unbind()
        {
            Unbind(boundOne);
            Unbind(boundTwo);
            boundOne = null;
            boundTwo = null;
        }

        private void Unbind(Fighter fighter)
        {
            if (fighter == null) return;
            fighter.OnLightAttackPerformed -= PlayLight;
            fighter.OnHeavyAttackPerformed -= PlayHeavy;
            fighter.OnSpecialUsed -= PlaySpecial;
            fighter.OnKnockedOut -= PlayKnockout;
            var detector = fighter.GetComponent<HitDetector>();
            if (detector != null) detector.HitLanded -= PlayHit;
        }

        private void PlayLight() => Play(lightClip, 0.7f);
        private void PlayHeavy() => Play(heavyClip, 0.9f);
        private void PlaySpecial() => Play(specialClip, 1f);
        private void PlayKnockout() => Play(knockoutClip, 1f);
        private void PlayHit(int damage)
        {
            if (damage > 0) Play(hitClip, Mathf.Clamp01(0.75f + damage / 80f));
        }

        private void Play(AudioClip clip, float multiplier)
        {
            if (source != null && clip != null) source.PlayOneShot(clip, volume * multiplier);
        }

        private static AudioClip Tone(string name, float startFrequency, float endFrequency, float duration, float amplitude)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(duration * sampleRate);
            var clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            var samples = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                float time = index / (float)sampleRate;
                float progress = time / duration;
                float phase = 2 * Mathf.PI * (startFrequency * time
                    + (endFrequency - startFrequency) * time * time / (2 * duration));
                float envelope = Mathf.Min(1, time * 90) * Mathf.Clamp01((duration - time) * 28);
                samples[index] = Mathf.Sin(phase) * envelope * amplitude;
            }
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
