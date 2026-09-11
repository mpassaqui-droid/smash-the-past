using System.Collections;
using NUnit.Framework;
using SmashThePast.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace SmashThePast.Characters.Tests
{
    public class HitDetectorTests
    {
        [TestCase(1.8f, 0, 0, true)]
        [TestCase(1.81f, 0, 0, false)]
        [TestCase(-1.8f, 0, 0, true)]
        [TestCase(0, 1.2f, 0, true)]
        [TestCase(0, 1.21f, 0, false)]
        [TestCase(0, 0, 1.81f, false)]
        [TestCase(1.5f, 0, 1.5f, false)]
        [TestCase(0, 0, 0, true)]
        public void RangeUsesHorizontalDistanceAndVerticalReach(float x, float y, float z, bool expected)
        {
            Assert.That(HitDetector.IsInRange(Vector3.zero, new Vector3(x, y, z), 1.8f, 1.2f), Is.EqualTo(expected));
        }

        [UnityTest]
        public IEnumerator FighterEventsApplyHitsAndPreserveCoreRules()
        {
            yield return new EnterPlayMode();
            var attackerObject = new GameObject("Atacante", typeof(CharacterController), typeof(Fighter), typeof(HitDetector));
            var defenderObject = new GameObject("Defensor", typeof(CharacterController), typeof(Fighter));
            var attacker = attackerObject.GetComponent<Fighter>();
            var defender = defenderObject.GetComponent<Fighter>();
            var detector = attackerObject.GetComponent<HitDetector>();
            defenderObject.transform.position = Vector3.right;
            detector.SetOpponent(defender);

            attacker.PerformLightAttack();
            int lightHealth = defender.Health.CurrentHealth;
            int attackMeter = attacker.Special.CurrentValue;
            attacker.PerformLightAttack();
            int cooldownHealth = defender.Health.CurrentHealth;
            detector.ResetAttack();
            attacker.SetBlocking(true);
            attacker.PerformHeavyAttack();
            int blockedAttackerHealth = defender.Health.CurrentHealth;
            attacker.SetBlocking(false);
            defender.SetBlocking(true);
            attacker.PerformHeavyAttack();
            int chipHealth = defender.Health.CurrentHealth;
            defender.SetBlocking(false);

            detector.ResetAttack();
            defenderObject.transform.position = Vector3.right * 5;
            attacker.PerformLightAttack();
            int outOfRangeHealth = defender.Health.CurrentHealth;
            detector.ResetAttack();
            defenderObject.transform.position = Vector3.right;
            detector.enabled = false;
            attacker.PerformLightAttack();
            int disabledHealth = defender.Health.CurrentHealth;
            detector.enabled = true;
            attacker.PerformLightAttack();
            int reenabledHealth = defender.Health.CurrentHealth;
            detector.ResetAttack();
            attacker.ReceiveHit(1000);
            attacker.PerformHeavyAttack();
            int deadAttackerHealth = defender.Health.CurrentHealth;
            Object.DestroyImmediate(attackerObject);
            Object.DestroyImmediate(defenderObject);
            yield return new ExitPlayMode();

            Assert.That(lightHealth, Is.EqualTo(95));
            Assert.That(attackMeter, Is.EqualTo(5));
            Assert.That(cooldownHealth, Is.EqualTo(95));
            Assert.That(blockedAttackerHealth, Is.EqualTo(95));
            Assert.That(chipHealth, Is.EqualTo(94));
            Assert.That(outOfRangeHealth, Is.EqualTo(94));
            Assert.That(disabledHealth, Is.EqualTo(94));
            Assert.That(reenabledHealth, Is.EqualTo(89));
            Assert.That(deadAttackerHealth, Is.EqualTo(89));
        }
    }
}
