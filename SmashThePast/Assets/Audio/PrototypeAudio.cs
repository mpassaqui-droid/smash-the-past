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
        private AudioClip heavyHitClip;
        private AudioClip specialClip;
        private AudioClip knockoutClip;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            if (source == null) return;
            source.playOnAwake = false;
            source.spatialBlend = 0;
            lightClip = LoadClip("knifeSlice");
            heavyClip = LoadClip("knifeSlice2");
            hitClip = LoadClip("impactPunch_medium_000");
            heavyHitClip = LoadClip("impactPunch_heavy_000");
            specialClip = LoadClip("impactBell_heavy_000");
            knockoutClip = LoadClip("impactPunch_heavy_002");
        }

        private void OnEnable() => BindCurrentFighters();

        private void Update()
        {
            if ((match == null ? null : match.PlayerOne) != boundOne
                || (match == null ? null : match.PlayerTwo) != boundTwo)
                BindCurrentFighters();
        }

        private void OnDisable()
        {
            Unbind();
            if (source != null) source.Stop();
        }

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
            fighter.OnKnockedOut += PlayKnockout;
            var detector = fighter.GetComponent<HitDetector>();
            if (detector != null)
            {
                detector.AttackPerformed += PlayAttack;
                detector.HitLanded += PlayHit;
            }
            var combatant = fighter.GetComponent<Combatant>();
            if (combatant != null) combatant.SpecialActivated += PlaySpecial;
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
            fighter.OnKnockedOut -= PlayKnockout;
            var detector = fighter.GetComponent<HitDetector>();
            if (detector != null)
            {
                detector.AttackPerformed -= PlayAttack;
                detector.HitLanded -= PlayHit;
            }
            var combatant = fighter.GetComponent<Combatant>();
            if (combatant != null) combatant.SpecialActivated -= PlaySpecial;
        }

        private void PlayAttack(bool heavy) => Play(heavy ? heavyClip : lightClip, heavy ? 0.8f : 0.6f);
        private void PlaySpecial(SpecialKind kind) => Play(specialClip, 0.7f);
        private void PlayKnockout() => Play(knockoutClip, 1f);
        private void PlayHit(int damage)
        {
            if (damage > 0)
                Play(damage >= 10 ? heavyHitClip : hitClip, Mathf.Clamp01(0.65f + damage / 80f));
        }

        private void Play(AudioClip clip, float multiplier)
        {
            if (source != null && clip != null) source.PlayOneShot(clip, volume * multiplier);
        }

        private AudioClip LoadClip(string name)
        {
            var clip = Resources.Load<AudioClip>("CombatAudio/" + name);
            if (clip == null) Debug.LogError("Falta el audio de combate: " + name, this);
            return clip;
        }
    }
}
