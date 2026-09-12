using System.Collections;
using NUnit.Framework;
using SmashThePast.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace SmashThePast.Characters.Tests
{
    public class SpecialCombatantTests
    {
        private Combatant attacker, defender;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return new EnterPlayMode();
            Time.timeScale = 1;
            attacker = new GameObject("Atacante").AddComponent<Combatant>();
            defender = new GameObject("Defensor").AddComponent<Combatant>();
            defender.transform.position = Vector3.right * 2;
            attacker.Opponent = defender;
            defender.Opponent = attacker;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Time.timeScale = 1;
            Object.Destroy(attacker.gameObject);
            Object.Destroy(defender.gameObject);
            yield return null;
            yield return new ExitPlayMode();
        }

        private void Activate(SpecialKind kind)
        {
            attacker.Kind = kind;
            attacker.Fighter.Special.Fill(SpecialMeter.MaxValue);
            attacker.Fighter.TryUseSpecial();
        }

        private HitDetector NormalAttackDetector()
        {
            defender.transform.position = Vector3.right;
            var detector = attacker.gameObject.AddComponent<HitDetector>();
            detector.SetOpponent(defender.Fighter);
            return detector;
        }

        [UnityTest]
        public IEnumerator NormalAttacksRespectGandhiInvulnerability()
        {
            NormalAttackDetector();
            defender.Kind = SpecialKind.Gandhi;
            defender.Fighter.Special.Fill(100);
            defender.Fighter.TryUseSpecial();
            attacker.Fighter.PerformHeavyAttack();
            Assert.AreEqual(100, defender.Fighter.Health.CurrentHealth);
            Assert.AreEqual(0, attacker.Fighter.Special.CurrentValue);
            yield return null;
        }

        [UnityTest]
        public IEnumerator NormalAttacksTriggerRasputinSurvivalBeforeKnockout()
        {
            var detector = NormalAttackDetector();
            defender.Kind = SpecialKind.Rasputin;
            defender.ReceiveDamage(95);
            int knockouts = 0;
            defender.Fighter.OnKnockedOut += () => knockouts++;
            attacker.Fighter.PerformHeavyAttack();
            Assert.AreEqual(1, defender.Fighter.Health.CurrentHealth);
            Assert.AreEqual(0, knockouts);
            detector.ResetAttack();
            attacker.Fighter.PerformLightAttack();
            Assert.AreEqual(1, knockouts);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SpecialsChargeBeforeEitherFighterIsKnockedOut()
        {
            var detector = NormalAttackDetector();
            for (int hit = 0; hit < 5; hit++)
            {
                detector.ResetAttack();
                attacker.Fighter.PerformHeavyAttack();
            }
            Assert.AreEqual(50, defender.Fighter.Health.CurrentHealth);
            Assert.IsTrue(attacker.Fighter.Special.IsReady);
            Assert.IsTrue(defender.Fighter.Special.IsReady);
            int activations = 0;
            attacker.SpecialActivated += _ => activations++;
            attacker.Fighter.TryUseSpecial();
            Assert.AreEqual(1, activations);
            Assert.AreEqual(38, defender.Fighter.Health.CurrentHealth);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GandhiPreventsDamageThenCounters()
        {
            int events = 0;
            attacker.SpecialActivated += kind => { if (kind == SpecialKind.Gandhi) events++; };
            Activate(SpecialKind.Gandhi);
            Assert.IsTrue(attacker.IsInvulnerable);
            Assert.IsFalse(attacker.CanAct);
            Assert.AreEqual(0, attacker.ReceiveDamage(200));
            Assert.AreEqual(100, attacker.Fighter.Health.CurrentHealth);
            yield return WaitFrames(1.6f);
            Assert.IsFalse(attacker.IsInvulnerable);
            Assert.AreEqual(70, defender.Fighter.Health.CurrentHealth);
            Assert.AreEqual(1, events);
        }

        [UnityTest]
        public IEnumerator RasputinSurvivesOnlyOnceIncludingBlockedOneHp()
        {
            attacker.Kind = SpecialKind.Rasputin;
            Assert.AreEqual(99, attacker.ReceiveDamage(99));
            attacker.Fighter.SetBlocking(true);
            Assert.AreEqual(0, attacker.ReceiveDamage(1));
            Assert.AreEqual(1, attacker.Fighter.Health.CurrentHealth);
            attacker.CombatEnabled = false;
            attacker.CombatEnabled = true;
            attacker.Fighter.SetBlocking(true);
            Assert.AreEqual(1, attacker.ReceiveDamage(1));
            Assert.IsTrue(attacker.Fighter.Health.IsDead);
            Assert.IsFalse(attacker.CanAct);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RasputinClampsOverkillWithAndWithoutBlock()
        {
            attacker.Kind = defender.Kind = SpecialKind.Rasputin;
            defender.Fighter.SetBlocking(true);
            Assert.AreEqual(99, attacker.ReceiveDamage(int.MaxValue));
            Assert.AreEqual(99, defender.ReceiveDamage(int.MaxValue));
            Assert.AreEqual(1, attacker.Fighter.Health.CurrentHealth);
            Assert.AreEqual(1, defender.Fighter.Health.CurrentHealth);
            Assert.AreEqual(1, attacker.ReceiveDamage(20));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TupacStunsAndRestoresExternalInput()
        {
            var input = defender.gameObject.AddComponent<PlayerInputController>();
            Activate(SpecialKind.Tupac);
            Assert.AreEqual(88, defender.Fighter.Health.CurrentHealth);
            Assert.IsTrue(defender.IsStunned);
            Assert.IsFalse(input.enabled);
            yield return WaitFrames(0.9f);
            Assert.IsFalse(defender.IsStunned);
            Assert.IsTrue(defender.CanAct);
            Assert.IsFalse(input.enabled);
            defender.ReceiveDamage(100);
            Assert.IsFalse(input.enabled);
        }

        [UnityTest]
        public IEnumerator HitlerTantrumDealsDamageAndStunsNearbyTarget()
        {
            Activate(SpecialKind.Hitler);
            Assert.AreEqual(90, defender.Fighter.Health.CurrentHealth);
            Assert.IsTrue(defender.IsStunned);
            yield return WaitFrames(0.9f);
            Assert.IsFalse(defender.IsStunned);
        }

        [UnityTest]
        public IEnumerator PoisonTicksButCannotCrossTargetDisable()
        {
            Activate(SpecialKind.Cleopatra);
            Assert.AreEqual(96, defender.Fighter.Health.CurrentHealth);
            yield return WaitFrames(0.6f);
            Assert.AreEqual(91, defender.Fighter.Health.CurrentHealth);
            defender.enabled = false;
            defender.enabled = true;
            yield return WaitFrames(1.7f);
            Assert.AreEqual(91, defender.Fighter.Health.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator ArrowsAreDelayedAndKnockoutCancelsRemainingShots()
        {
            Activate(SpecialKind.Genghis);
            Assert.AreEqual(100, defender.Fighter.Health.CurrentHealth);
            yield return WaitFrames(0.2f);
            Assert.AreEqual(92, defender.Fighter.Health.CurrentHealth);
            attacker.ReceiveDamage(100);
            yield return WaitFrames(0.5f);
            Assert.AreEqual(92, defender.Fighter.Health.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator PauseFreezesPoisonAndSourceDisableCancelsIt()
        {
            Activate(SpecialKind.Cleopatra);
            Time.timeScale = 0;
            Assert.IsFalse(attacker.CanAct);
            Assert.AreEqual(0, defender.ReceiveDamage(10));
            yield return WaitFrames(0.6f);
            Assert.AreEqual(96, defender.Fighter.Health.CurrentHealth);
            Time.timeScale = 1;
            attacker.CombatEnabled = false;
            attacker.CombatEnabled = true;
            yield return WaitFrames(0.6f);
            Assert.AreEqual(96, defender.Fighter.Health.CurrentHealth);
        }

        private static IEnumerator WaitFrames(float seconds)
        {
            float elapsed = 0;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator CannonAndBoomerangResolveTheirDelayedHits()
        {
            Activate(SpecialKind.Napoleon);
            Assert.AreEqual(100, defender.Fighter.Health.CurrentHealth);
            yield return WaitFrames(0.75f);
            Assert.AreEqual(72, defender.Fighter.Health.CurrentHealth);
            Activate(SpecialKind.Lincoln);
            yield return WaitFrames(0.3f);
            Assert.AreEqual(62, defender.Fighter.Health.CurrentHealth);
            yield return WaitFrames(0.45f);
            Assert.AreEqual(52, defender.Fighter.Health.CurrentHealth);
        }

        [UnityTest]
        public IEnumerator MusashiDashesAndSocratesInversionExpires()
        {
            float before = attacker.transform.position.x;
            Activate(SpecialKind.Musashi);
            yield return WaitFrames(0.5f);
            Assert.Greater(attacker.transform.position.x, before);
            Assert.Less(defender.Fighter.Health.CurrentHealth, 100);
            Activate(SpecialKind.Socrates);
            Assert.IsTrue(defender.ControlsInverted);
            yield return WaitFrames(2.1f);
            Assert.IsFalse(defender.ControlsInverted);
        }

        [UnityTest]
        public IEnumerator EveryBaseSpecialHasVisibleGeometryAndDisableCleansItUp()
        {
            for (int index = 0; index < 9; index++)
            {
                var source = new GameObject("VFX " + index).AddComponent<Combatant>();
                source.Opponent = defender;
                source.Kind = (SpecialKind)index;
                var visual = source.gameObject.AddComponent<SpecialVisual>();
                if (source.Kind == SpecialKind.Rasputin) source.ReceiveDamage(100);
                else
                {
                    source.Fighter.Special.Fill(100);
                    Assert.IsTrue(source.TrySpecial());
                }
                yield return null;
                Assert.AreEqual(1, visual.ActiveEffectCount, source.Kind.ToString());
                var line = source.GetComponentInChildren<LineRenderer>();
                Assert.IsNotNull(line);
                Assert.Greater(line.positionCount, 1);
                Assert.IsNotNull(line.sharedMaterial.shader);
                Assert.Greater(Vector3.Distance(line.GetPosition(0), line.GetPosition(1)), 0);
                foreach (var collider in source.GetComponentsInChildren<Collider>())
                    if (!(collider is CharacterController)) Assert.IsFalse(collider.enabled);
                visual.enabled = false;
                Assert.AreEqual(0, visual.ActiveEffectCount);
                yield return null;
                Assert.IsNull(source.GetComponentInChildren<LineRenderer>());
                Object.Destroy(source.gameObject);
            }
        }
    }
}
