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
        // Voces adicionales: PlayOneShot comparte el tono de su AudioSource, asi que
        // sin varias fuentes todos los golpes seguidos suenan exactamente igual.
        private AudioSource[] voces;
        private int siguienteVoz;
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
            voces = new AudioSource[4];
            for (int i = 0; i < voces.Length; i++)
            {
                var voz = gameObject.AddComponent<AudioSource>();
                voz.playOnAwake = false;
                voz.spatialBlend = 0;
                voz.outputAudioMixerGroup = source.outputAudioMixerGroup;
                voces[i] = voz;
            }
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
            if (voces != null)
                foreach (var voz in voces)
                    if (voz != null) voz.Stop();
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

        private void PlayAttack(bool heavy) => Play(heavy ? heavyClip : lightClip, heavy ? 0.8f : 0.6f, 0.09f);
        private void PlaySpecial(SpecialKind kind) => Play(specialClip, 0.7f, 0.03f);
        private void PlayKnockout() => Play(knockoutClip, 1f, 0f);

        private void PlayHit(int damage)
        {
            if (damage <= 0) return;
            bool fuerte = damage >= 10;
            Play(fuerte ? heavyHitClip : hitClip, Mathf.Clamp01(0.65f + damage / 80f), 0.07f);

            // Capa grave por debajo del golpe fuerte. Un impacto que convence casi nunca
            // es un solo sonido: es uno seco arriba y un cuerpo grave debajo.
            if (fuerte) Play(heavyHitClip, 0.45f, 0.04f, 0.62f);
        }

        private void Play(AudioClip clip, float multiplier, float variacionDeTono = 0f, float tonoBase = 1f)
        {
            if (clip == null) return;
            var voz = SiguienteVoz();
            if (voz == null) return;
            voz.pitch = tonoBase + Random.Range(-variacionDeTono, variacionDeTono);
            voz.PlayOneShot(clip, volume * multiplier);
        }

        private AudioSource SiguienteVoz()
        {
            if (voces == null || voces.Length == 0) return source;
            var voz = voces[siguienteVoz];
            siguienteVoz = (siguienteVoz + 1) % voces.Length;
            return voz == null ? source : voz;
        }

        private AudioClip LoadClip(string name)
        {
            var clip = Resources.Load<AudioClip>("CombatAudio/" + name);
            if (clip == null) Debug.LogError("Falta el audio de combate: " + name, this);
            return clip;
        }
    }
}
