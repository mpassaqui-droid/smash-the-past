using System.Collections;
using NUnit.Framework;
using SmashThePast.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace SmashThePast.Characters.Tests
{
    // La presa es la respuesta al que se queda tapado: atraviesa la guardia, pero si
    // se falla el atacante queda vendido. Sin ese riesgo, agarrar seria la respuesta a todo.
    public class GrabTests
    {
        [UnityTest]
        public IEnumerator LaPresaAtraviesaLaGuardia()
        {
            yield return new EnterPlayMode();
            var atacanteObj = new GameObject("Atacante", typeof(CharacterController), typeof(Fighter), typeof(HitDetector));
            var defensorObj = new GameObject("Defensor", typeof(CharacterController), typeof(Fighter));
            var atacante = atacanteObj.GetComponent<Fighter>();
            var defensor = defensorObj.GetComponent<Fighter>();
            var detector = atacanteObj.GetComponent<HitDetector>();
            defensorObj.transform.position = Vector3.right;
            detector.SetOpponent(defensor);

            defensor.SetBlocking(true);
            int antes = defensor.Health.CurrentHealth;
            atacante.PerformGrab();
            int despues = defensor.Health.CurrentHealth;
            bool aturdido = defensor.IsStunned;
            bool guardia = defensor.IsBlocking;

            Object.DestroyImmediate(atacanteObj);
            Object.DestroyImmediate(defensorObj);
            yield return new ExitPlayMode();

            Assert.That(despues, Is.EqualTo(antes - atacante.GrabDamage), "la presa no debe reducirse por bloquear");
            Assert.That(aturdido, Is.True, "una presa que conecta aturde");
            Assert.That(guardia, Is.False, "la presa abre la guardia");
        }

        [UnityTest]
        public IEnumerator UnaPresaFalladaDejaVendidoAlAtacante()
        {
            yield return new EnterPlayMode();
            var atacanteObj = new GameObject("Atacante", typeof(CharacterController), typeof(Fighter), typeof(HitDetector));
            var defensorObj = new GameObject("Defensor", typeof(CharacterController), typeof(Fighter));
            var atacante = atacanteObj.GetComponent<Fighter>();
            var defensor = defensorObj.GetComponent<Fighter>();
            var detector = atacanteObj.GetComponent<HitDetector>();
            defensorObj.transform.position = Vector3.right * 6;
            detector.SetOpponent(defensor);

            // Presa al aire, con el rival lejos.
            atacante.PerformGrab();

            // El rival se acerca: dentro del castigo no se puede ni agarrar ni pegar.
            defensorObj.transform.position = Vector3.right;
            int vidaAntes = defensor.Health.CurrentHealth;
            atacante.PerformGrab();
            atacante.PerformLightAttack();
            int vidaDespues = defensor.Health.CurrentHealth;

            Object.DestroyImmediate(atacanteObj);
            Object.DestroyImmediate(defensorObj);
            yield return new ExitPlayMode();

            Assert.That(vidaDespues, Is.EqualTo(vidaAntes), "tras fallar la presa hay que quedarse vendido");
        }

        [Test]
        public void AturdidoNoSePuedeAgarrar()
        {
            var objeto = new GameObject("Luchador", typeof(CharacterController));
            var luchador = objeto.AddComponent<Fighter>();
            int presas = 0;
            luchador.OnGrabPerformed += () => presas++;

            luchador.ApplyStun(0.3f);
            luchador.PerformGrab();

            Object.DestroyImmediate(objeto);
            Assert.That(presas, Is.Zero);
        }

        [Test]
        public void BloqueandoNoSeAgarra()
        {
            var objeto = new GameObject("Luchador", typeof(CharacterController));
            var luchador = objeto.AddComponent<Fighter>();
            int presas = 0;
            luchador.OnGrabPerformed += () => presas++;

            luchador.SetBlocking(true);
            luchador.PerformGrab();

            Object.DestroyImmediate(objeto);
            Assert.That(presas, Is.Zero, "no se agarra desde la guardia");
        }
    }
}
