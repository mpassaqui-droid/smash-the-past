using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SmashThePast.Audio.Tests
{
    public class CombatAudioAssetTests
    {
        // El hash fija QUE archivo exacto se publica. Si alguien lo sustituye sin querer,
        // esta prueba lo dice. El autor va en el caso para que el fallo nombre a quien
        // corresponde: dos efectos son de Diego y el resto siguen siendo de Kenney.
        [TestCase("knifeSlice", "f5d564b246b1e81e8d593f6e82cab8f3c97373e29e83665184c12361825c1712", "Diego")]
        [TestCase("knifeSlice2", "6c2064d0ef988d1ec3d56868e823ea8823a5cac00f2742560052633529407def", "Kenney")]
        [TestCase("impactPunch_medium_000", "f1cd9932ac68610b5e3c8beea71807be1b7a58c4808f4d6ba53d615df5ce5de2", "Diego")]
        [TestCase("impactPunch_heavy_000", "b33a8f14068aec24ec69ba85e5e87fdc41228975f6a1a3e44a6e7d6fc3d9f8d8", "Kenney")]
        [TestCase("impactBell_heavy_000", "94b8bb5f2d43ab65e4bcc32b28562416e9bc2c51d9fd4be1e333660ee52f977f", "Kenney")]
        [TestCase("impactPunch_heavy_002", "7993dd4c156b9979ad69f17be5ebe31850b16039a3857f8477141be54dfee1b3", "Kenney")]
        [TestCase("guardia_bloqueo", "89928231ecc2e3ff80af4ae5d228aadea329d4e710dc728b5fe4f727a0a1e4fe", "sintetizado")]
        [TestCase("inicio_asalto", "ce5cb0a464d2e9d1bf5a82bb45d9c95b293d506c26e8a6eb27e8f715558f9e92", "sintetizado")]
        public void ResourceIsTheExpectedClipAndDecodesAudibleSamples(string name, string expectedHash, string autor)
        {
            string path = "Assets/Audio/Resources/CombatAudio/" + name + ".ogg";
            using (var sha = SHA256.Create())
            {
                string actual = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)))
                    .Replace("-", "").ToLowerInvariant();
                Assert.That(actual, Is.EqualTo(expectedHash), $"El clip de {autor} ha cambiado: {name}.ogg");
            }

            var clip = Resources.Load<AudioClip>("CombatAudio/" + name);
            Assert.That(clip, Is.Not.Null, path);
            Assert.That(AssetDatabase.GetAssetPath(clip), Is.EqualTo(path));
            Assert.That(clip.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
            Assert.That(clip.length, Is.InRange(0.05f, 2f));
            Assert.That(clip.channels, Is.InRange(1, 2));
            Assert.That(clip.frequency, Is.InRange(22050, 48000));
            Assert.That(clip.LoadAudioData(), Is.True);
            Assert.That(clip.loadState, Is.EqualTo(AudioDataLoadState.Loaded));
            var samples = new float[clip.samples * clip.channels];
            Assert.That(clip.GetData(samples, 0), Is.True);
            double energy = 0;
            bool finite = true;
            foreach (float sample in samples)
            {
                finite &= !float.IsNaN(sample) && !float.IsInfinity(sample);
                energy += sample * sample;
            }
            Assert.That(finite, Is.True);
            Assert.That(Math.Sqrt(energy / samples.Length), Is.GreaterThan(0.001), "Clip silencioso.");
        }
    }
}
