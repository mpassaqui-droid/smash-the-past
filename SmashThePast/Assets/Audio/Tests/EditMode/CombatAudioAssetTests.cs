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
        [TestCase("knifeSlice", "4cd96dc630bed9840c15f1dd2306da2cc56a4da26a5d3f1a03c5a7265ac5e54f")]
        [TestCase("knifeSlice2", "6c2064d0ef988d1ec3d56868e823ea8823a5cac00f2742560052633529407def")]
        [TestCase("impactPunch_medium_000", "486988aa2d6440ffc4c62a0e8ccf3c23673ba84424bd4723378d451b7255eb5c")]
        [TestCase("impactPunch_heavy_000", "b33a8f14068aec24ec69ba85e5e87fdc41228975f6a1a3e44a6e7d6fc3d9f8d8")]
        [TestCase("impactBell_heavy_000", "94b8bb5f2d43ab65e4bcc32b28562416e9bc2c51d9fd4be1e333660ee52f977f")]
        [TestCase("impactPunch_heavy_002", "7993dd4c156b9979ad69f17be5ebe31850b16039a3857f8477141be54dfee1b3")]
        public void ResourceIsOriginalKenneyClipAndDecodesAudibleSamples(string name, string expectedHash)
        {
            string path = "Assets/Audio/Resources/CombatAudio/" + name + ".ogg";
            using (var sha = SHA256.Create())
            {
                string actual = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)))
                    .Replace("-", "").ToLowerInvariant();
                Assert.That(actual, Is.EqualTo(expectedHash), "El original de Kenney ha cambiado.");
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
