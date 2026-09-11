using System;
using NUnit.Framework;
using SmashThePast.Core;

namespace SmashThePast.Core.Tests
{
    public class SpecialMeterTests
    {
        [Test]
        public void StartsEmpty()
        {
            var meter = new SpecialMeter();
            Assert.AreEqual(0, meter.CurrentValue);
            Assert.IsFalse(meter.IsReady);
        }

        [Test]
        public void FillsUpToMax_NeverOverflows()
        {
            var meter = new SpecialMeter();
            meter.Fill(150);
            Assert.AreEqual(SpecialMeter.MaxValue, meter.CurrentValue);
            Assert.IsTrue(meter.IsReady);
        }

        [Test]
        public void TryConsume_FailsWhenNotReady()
        {
            var meter = new SpecialMeter();
            meter.Fill(50);
            Assert.IsFalse(meter.TryConsume());
            Assert.AreEqual(50, meter.CurrentValue);
        }

        [Test]
        public void TryConsume_SucceedsAndResetsWhenReady()
        {
            var meter = new SpecialMeter();
            meter.Fill(100);
            Assert.IsTrue(meter.TryConsume());
            Assert.AreEqual(0, meter.CurrentValue);
        }

        [Test]
        public void NegativeFill_Throws()
        {
            var meter = new SpecialMeter();
            Assert.Throws<ArgumentOutOfRangeException>(() => meter.Fill(-1));
        }
    }
}
