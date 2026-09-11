using System;
using NUnit.Framework;
using SmashThePast.Core;

namespace SmashThePast.Core.Tests
{
    public class HealthSystemTests
    {
        [Test]
        public void StartsAtMaxHealth()
        {
            var health = new HealthSystem(100);
            Assert.AreEqual(100, health.CurrentHealth);
            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void ApplyHit_WithoutBlock_DealsFullDamage()
        {
            var health = new HealthSystem(100);
            int applied = health.ApplyHit(20, defenderIsBlocking: false);
            Assert.AreEqual(20, applied);
            Assert.AreEqual(80, health.CurrentHealth);
        }

        [Test]
        public void ApplyHit_WhileBlocking_DealsChipDamageOnly()
        {
            var health = new HealthSystem(100);
            int applied = health.ApplyHit(20, defenderIsBlocking: true);
            Assert.AreEqual(2, applied); // 10% de 20
            Assert.AreEqual(98, health.CurrentHealth);
        }

        [Test]
        public void ApplyHit_NeverGoesBelowZero()
        {
            var health = new HealthSystem(10);
            health.ApplyHit(100, defenderIsBlocking: false);
            Assert.AreEqual(0, health.CurrentHealth);
            Assert.IsTrue(health.IsDead);
        }

        [Test]
        public void DeadFighter_TakesNoMoreDamage()
        {
            var health = new HealthSystem(10);
            health.ApplyHit(10, false);
            int applied = health.ApplyHit(5, false);
            Assert.AreEqual(0, applied);
        }

        [Test]
        public void NegativeDamage_Throws()
        {
            var health = new HealthSystem(100);
            Assert.Throws<ArgumentOutOfRangeException>(() => health.ApplyHit(-5, false));
        }

        [Test]
        public void Reset_RestoresMaxHealth()
        {
            var health = new HealthSystem(50);
            health.ApplyHit(30, false);
            health.Reset();
            Assert.AreEqual(50, health.CurrentHealth);
        }

        [Test]
        public void ZeroOrNegativeMaxHealth_ThrowsOnConstruction()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthSystem(0));
        }
    }
}
