using NUnit.Framework;
using SmashThePast.Audio;
using SmashThePast.Stages;

namespace SmashThePast.Audio.Tests
{
    // La musica se elige por nombre de archivo para que anadirla no toque codigo.
    // Estas pruebas fijan ese contrato.
    public class MusicaTests
    {
        [Test]
        public void EnSeleccionSuenaElMenu()
        {
            var candidatas = MusicaDelJuego.Candidatas(MatchPhase.Selection, 3);
            Assert.That(candidatas[0], Is.EqualTo(MusicaDelJuego.PistaDeMenu));
        }

        [Test]
        public void CadaArenaPideSuPistaYLuegoLaDeRespaldo()
        {
            var candidatas = MusicaDelJuego.Candidatas(MatchPhase.Fighting, 0);
            Assert.That(candidatas[0], Is.EqualTo("arena1"), "la arena del primer personaje es arena1");
            Assert.That(candidatas[1], Is.EqualTo(MusicaDelJuego.PistaDeRespaldo));
        }

        [Test]
        public void LaNovenaArenaEsArena9()
        {
            Assert.That(MusicaDelJuego.Candidatas(MatchPhase.Fighting, 8)[0], Is.EqualTo("arena9"));
        }

        [Test]
        public void SinArenaConocidaSuenaElRespaldo()
        {
            var candidatas = MusicaDelJuego.Candidatas(MatchPhase.Fighting, -1);
            Assert.That(candidatas, Has.Length.EqualTo(1));
            Assert.That(candidatas[0], Is.EqualTo(MusicaDelJuego.PistaDeRespaldo));
        }

        [Test]
        public void ElRespaldoSiempreEsLaUltimaOpcion()
        {
            foreach (var fase in new[] { MatchPhase.Selection, MatchPhase.Fighting, MatchPhase.RoundIntro, MatchPhase.Finished })
                for (int arena = -1; arena < 9; arena++)
                {
                    var candidatas = MusicaDelJuego.Candidatas(fase, arena);
                    Assert.That(candidatas[candidatas.Length - 1], Is.EqualTo(MusicaDelJuego.PistaDeRespaldo),
                        $"fase {fase}, arena {arena}");
                }
        }
    }
}
