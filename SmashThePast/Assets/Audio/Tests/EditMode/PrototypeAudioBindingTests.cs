using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SmashThePast.Characters;
using SmashThePast.Core;
using SmashThePast.Stages;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace SmashThePast.Audio.Tests
{
    public class PrototypeAudioBindingTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [UnityTest]
        public IEnumerator AcceptedEventsStayBoundOnceAcrossDisableAndFighterReplacement()
        {
            yield return new EnterPlayMode();
            var first = new GameObject("Primero", typeof(Fighter), typeof(HitDetector), typeof(Combatant));
            var second = new GameObject("Segundo", typeof(Fighter), typeof(HitDetector), typeof(Combatant));
            var replacement = new GameObject("Reemplazo", typeof(Fighter), typeof(HitDetector), typeof(Combatant));
            var matchObject = new GameObject("Partida de prueba");
            var audioObject = new GameObject("Audio de prueba");
            try
            {
                var match = matchObject.AddComponent<MatchController>();
                match.enabled = false;
                SetField(match, "playerOne", first.GetComponent<Fighter>());
                SetField(match, "playerTwo", second.GetComponent<Fighter>());
                audioObject.SetActive(false);
                var audio = audioObject.AddComponent<PrototypeAudio>();
                SetField(audio, "match", match);
                audioObject.SetActive(true);

                AssertBindings(first, audio, 1);
                AssertBindings(second, audio, 1);
                foreach (string field in new[] { "lightClip", "heavyClip", "hitClip", "heavyHitClip", "specialClip", "knockoutClip" })
                    Assert.That(typeof(PrototypeAudio).GetField(field, PrivateInstance).GetValue(audio), Is.Not.Null, field);

                audio.enabled = false;
                AssertBindings(first, audio, 0);
                AssertBindings(second, audio, 0);
                audio.enabled = true;
                AssertBindings(first, audio, 1);
                AssertBindings(second, audio, 1);

                SetField(match, "playerOne", replacement.GetComponent<Fighter>());
                audio.SendMessage("Update");
                audio.SendMessage("Update");
                AssertBindings(first, audio, 0);
                AssertBindings(replacement, audio, 1);
                AssertBindings(second, audio, 1);

                SetField(audio, "match", null);
                audio.SendMessage("Update");
                AssertBindings(replacement, audio, 0);
                AssertBindings(second, audio, 0);
            }
            finally
            {
                Object.DestroyImmediate(audioObject);
                Object.DestroyImmediate(matchObject);
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
                Object.DestroyImmediate(replacement);
            }
            yield return new ExitPlayMode();
        }

        private static void SetField(object target, string name, object value)
            => target.GetType().GetField(name, PrivateInstance).SetValue(target, value);

        private static void AssertBindings(GameObject fighterObject, PrototypeAudio audio, int expected)
        {
            var fighter = fighterObject.GetComponent<Fighter>();
            AssertSubscription(fighter, "OnLightAttackPerformed", audio, 0);
            AssertSubscription(fighter, "OnHeavyAttackPerformed", audio, 0);
            AssertSubscription(fighter, "OnSpecialUsed", audio, 0);
            AssertSubscription(fighter, "OnKnockedOut", audio, expected);
            AssertSubscription(fighterObject.GetComponent<HitDetector>(), "AttackPerformed", audio, expected);
            AssertSubscription(fighterObject.GetComponent<HitDetector>(), "HitLanded", audio, expected);
            // Incluye la supervivencia pasiva de Rasputin, que no emite OnSpecialUsed.
            AssertSubscription(fighterObject.GetComponent<Combatant>(), "SpecialActivated", audio, expected);
        }

        private static void AssertSubscription(object publisher, string name, PrototypeAudio audio, int expected)
        {
            var callbacks = (Delegate)publisher.GetType().GetField(name, PrivateInstance).GetValue(publisher);
            int count = callbacks?.GetInvocationList().Count(callback => ReferenceEquals(callback.Target, audio)) ?? 0;
            Assert.That(count, Is.EqualTo(expected), name);
        }
    }
}
