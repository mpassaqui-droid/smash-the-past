using NUnit.Framework;
using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Core.Tests
{
    // El aturdimiento es la regla que da consecuencia a un golpe: el que encaja pierde
    // el turno unas decimas y el que para lo pierde mucho menos.
    public class StunTests
    {
        private GameObject objeto;
        private Fighter luchador;

        [SetUp]
        public void Preparar()
        {
            objeto = new GameObject("LuchadorDePrueba", typeof(CharacterController));
            luchador = objeto.AddComponent<Fighter>();
        }

        [TearDown]
        public void Limpiar()
        {
            if (objeto != null) Object.DestroyImmediate(objeto);
        }

        [Test]
        public void UnGolpeSinGuardiaAturdeMasQueUnoParado()
        {
            luchador.SetBlocking(false);
            luchador.ReceiveHit(5);
            float sinGuardia = luchador.StunRemaining;

            Limpiar();
            Preparar();
            luchador.SetBlocking(true);
            luchador.ReceiveHit(5);
            float conGuardia = luchador.StunRemaining;

            Assert.That(sinGuardia, Is.EqualTo(Fighter.LightHitStunSeconds).Within(0.001f));
            Assert.That(conGuardia, Is.EqualTo(Fighter.BlockStunSeconds).Within(0.001f));
            Assert.That(conGuardia, Is.LessThan(sinGuardia), "parar tiene que costar menos que encajar");
        }

        [Test]
        public void UnGolpeFuerteAturdeMasQueUnoLigero()
        {
            luchador.ReceiveHit(5);
            float ligero = luchador.StunRemaining;

            Limpiar();
            Preparar();
            luchador.ReceiveHit(12);
            float fuerte = luchador.StunRemaining;

            Assert.That(fuerte, Is.GreaterThan(ligero));
            Assert.That(fuerte, Is.EqualTo(Fighter.HeavyHitStunSeconds).Within(0.001f));
        }

        [Test]
        public void AturdidoNoAtacaNiSalta()
        {
            int ligeros = 0, fuertes = 0, especiales = 0;
            luchador.OnLightAttackPerformed += () => ligeros++;
            luchador.OnHeavyAttackPerformed += () => fuertes++;
            luchador.OnSpecialUsed += () => especiales++;

            luchador.ApplyStun(0.3f);
            Assert.That(luchador.IsStunned, Is.True);

            luchador.PerformLightAttack();
            luchador.PerformHeavyAttack();
            luchador.Special.Fill(100);
            luchador.TryUseSpecial();

            Assert.That(ligeros, Is.Zero);
            Assert.That(fuertes, Is.Zero);
            Assert.That(especiales, Is.Zero);
        }

        [Test]
        public void AturdidoNoPuedeLevantarLaGuardia()
        {
            luchador.ReceiveHit(12);
            Assert.That(luchador.IsBlocking, Is.False, "encajar sin guardia la baja");

            luchador.SetBlocking(true);
            Assert.That(luchador.IsBlocking, Is.False, "no se puede tapar dentro del aturdimiento");
        }

        [Test]
        public void PararMantieneLaGuardiaDurenteElAturdimiento()
        {
            luchador.SetBlocking(true);
            luchador.ReceiveHit(12);

            Assert.That(luchador.IsStunned, Is.True);
            Assert.That(luchador.IsBlocking, Is.True, "en guardia se sigue tapado mientras dura el block stun");
        }

        [Test]
        public void ElAturdimientoMayorGana()
        {
            luchador.ApplyStun(0.4f);
            luchador.ApplyStun(0.1f);
            Assert.That(luchador.StunRemaining, Is.EqualTo(0.4f).Within(0.001f));
        }

        [Test]
        public void UnKoNoAturde()
        {
            luchador.ReceiveHit(10000);
            Assert.That(luchador.Health.IsDead, Is.True);
            Assert.That(luchador.IsStunned, Is.False, "un KO no necesita aturdimiento, el combate ha terminado");
        }
    }
}
