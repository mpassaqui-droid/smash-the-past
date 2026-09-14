using NUnit.Framework;
using SmashThePast.Characters;
using SmashThePast.Stages;

namespace SmashThePast.Stages.Tests
{
    public class ModoArcadeTests
    {
        [Test]
        public void LaEscaleraNoIncluyeAlPropioPersonaje()
        {
            var escalera = ModoArcade.Escalera(3, 9);
            Assert.That(escalera, Has.Length.EqualTo(8));
            Assert.That(escalera, Has.None.EqualTo(3), "nadie se pelea consigo mismo");
        }

        [Test]
        public void CadaPersonajeTieneSuPropiaEscalera()
        {
            var primera = ModoArcade.Escalera(0, 9);
            var segunda = ModoArcade.Escalera(1, 9);
            Assert.That(primera, Is.Not.EqualTo(segunda));
            Assert.That(primera[0], Is.EqualTo(1));
            Assert.That(segunda[0], Is.EqualTo(2));
        }

        [Test]
        public void LaEscaleraRecorreTodoElRosterSinRepetir()
        {
            var escalera = ModoArcade.Escalera(5, 9);
            Assert.That(escalera, Is.Unique);
            for (int i = 0; i < 9; i++)
                if (i != 5) Assert.That(escalera, Contains.Item(i), $"falta el personaje {i}");
        }

        [Test]
        public void UnRosterDeUnoNoTieneEscalera()
        {
            Assert.That(ModoArcade.Escalera(0, 1), Is.Empty);
        }

        [Test]
        public void LaDificultadSubePorTramos()
        {
            Assert.That(ModoArcade.DificultadDelPaso(0, 8), Is.EqualTo(NivelIA.Facil));
            Assert.That(ModoArcade.DificultadDelPaso(4, 8), Is.EqualTo(NivelIA.Normal));
            Assert.That(ModoArcade.DificultadDelPaso(6, 8), Is.EqualTo(NivelIA.Dificil));
        }

        [Test]
        public void ElUltimoRivalSiempreEsDificil()
        {
            for (int total = 2; total <= 9; total++)
                Assert.That(ModoArcade.DificultadDelPaso(total - 1, total), Is.EqualTo(NivelIA.Dificil),
                    $"con {total} rivales el final deberia ser dificil");
        }

        [Test]
        public void LaDificultadNuncaBaja()
        {
            var orden = new[] { NivelIA.Facil, NivelIA.Normal, NivelIA.Dificil };
            int anterior = 0;
            for (int paso = 0; paso < 8; paso++)
            {
                int actual = System.Array.IndexOf(orden, ModoArcade.DificultadDelPaso(paso, 8));
                Assert.That(actual, Is.GreaterThanOrEqualTo(anterior), $"bajo en el paso {paso}");
                anterior = actual;
            }
        }
    }
}
