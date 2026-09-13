using NUnit.Framework;
using SmashThePast.Characters;

namespace SmashThePast.Characters.Tests
{
    // La decision de la IA es una funcion pura: distancia, lo que hace el rival, nivel y
    // un sorteo fijado. Asi se puede comprobar sin abrir el juego y sin depender del azar.
    public class CerebroIATests
    {
        [Test]
        public void LejosSeAcerca()
        {
            var accion = CerebroIA.Decidir(6f, false, false, false, NivelIA.Normal, 0.5f);
            Assert.That(accion, Is.EqualTo(AccionIA.Acercarse));
        }

        [Test]
        public void AlQueSeQuedaTapadoSeLeAgarra()
        {
            var accion = CerebroIA.Decidir(1.2f, true, false, false, NivelIA.Normal, 0.9f);
            Assert.That(accion, Is.EqualTo(AccionIA.Presa), "la presa es la respuesta a la guardia");
        }

        [Test]
        public void TaparseDependeDelNivel()
        {
            // Un sorteo de 0.3 esta por debajo del umbral en dificil (0.7) y por encima
            // en facil (0.15): el mismo azar da guardia o golpe segun la dificultad.
            var dificil = CerebroIA.Decidir(1.5f, false, true, false, NivelIA.Dificil, 0.3f);
            var facil = CerebroIA.Decidir(1.5f, false, true, false, NivelIA.Facil, 0.3f);

            Assert.That(dificil, Is.EqualTo(AccionIA.Guardia));
            Assert.That(facil, Is.Not.EqualTo(AccionIA.Guardia));
        }

        [Test]
        public void ElNivelDificilReaccionaAntesQueElFacil()
        {
            Assert.That(CerebroIA.SegundosDeReaccion(NivelIA.Dificil),
                Is.LessThan(CerebroIA.SegundosDeReaccion(NivelIA.Normal)));
            Assert.That(CerebroIA.SegundosDeReaccion(NivelIA.Normal),
                Is.LessThan(CerebroIA.SegundosDeReaccion(NivelIA.Facil)));
        }

        [Test]
        public void ConSpecialListoLoUsaAVeces()
        {
            var conSorteoBajo = CerebroIA.Decidir(1.5f, false, false, true, NivelIA.Normal, 0.1f);
            var conSorteoAlto = CerebroIA.Decidir(1.5f, false, false, true, NivelIA.Normal, 0.9f);

            Assert.That(conSorteoBajo, Is.EqualTo(AccionIA.Special));
            Assert.That(conSorteoAlto, Is.Not.EqualTo(AccionIA.Special), "no se gasta el special siempre");
        }

        [Test]
        public void EnFacilSePegaMasLigeroQueFuerte()
        {
            // Con el mismo sorteo intermedio, facil elige ligero y dificil fuerte.
            var facil = CerebroIA.Decidir(1.5f, false, false, false, NivelIA.Facil, 0.4f);
            var dificil = CerebroIA.Decidir(1.5f, false, false, false, NivelIA.Dificil, 0.4f);

            Assert.That(facil, Is.EqualTo(AccionIA.GolpeLigero));
            Assert.That(dificil, Is.EqualTo(AccionIA.GolpeFuerte));
        }

        [Test]
        public void NuncaSeQuedaQuietaSinRazon()
        {
            foreach (var nivel in new[] { NivelIA.Facil, NivelIA.Normal, NivelIA.Dificil })
                for (float sorteo = 0f; sorteo < 1f; sorteo += 0.1f)
                    Assert.That(CerebroIA.Decidir(1.5f, false, false, false, nivel, sorteo),
                        Is.Not.EqualTo(AccionIA.Esperar), $"nivel {nivel}, sorteo {sorteo}");
        }
    }
}
