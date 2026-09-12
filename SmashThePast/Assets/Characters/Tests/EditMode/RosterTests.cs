using System.Linq;
using NUnit.Framework;
using SmashThePast.Core;
using UnityEditor;

namespace SmashThePast.Characters.Tests
{
    public class RosterTests
    {
        [TestCase("TupacAmaruII", "T\u00fapac Amaru II", "Grito de Libertad")]
        [TestCase("GenghisKhan", "Genghis Khan", "Carga Mongola")]
        [TestCase("MiyamotoMusashi", "Miyamoto Musashi", "Niten Ichi-ry\u016b")]
        [TestCase("Gandhi", "Gandhi", "Ayuno")]
        [TestCase("Rasputin", "Rasput\u00edn", "Indestructible")]
        [TestCase("NapoleonBonaparte", "Napole\u00f3n Bonaparte", "Artiller\u00eda")]
        [TestCase("Cleopatra", "Cleopatra", "\u00c1spid")]
        [TestCase("AbrahamLincoln", "Abraham Lincoln", "Hat Trick")]
        [TestCase("Socrates", "S\u00f3crates", "May\u00e9utica")]
        public void BaseCharacterHasCompleteContent(string file, string name, string special)
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>($"Assets/Characters/{file}.asset");
            Assert.That(data, Is.Not.Null);
            {
                Assert.That(data.characterName, Is.EqualTo(name));
                Assert.That(data.specialName, Is.EqualTo(special));
                Assert.That(data.bio, Is.Not.Null.And.Not.Empty);
                Assert.That(data.specialDescription, Is.Not.Null.And.Not.Empty);
                Assert.That(data.winQuotes.Length, Is.InRange(2, 3));
                Assert.That(data.loseQuotes.Length, Is.InRange(2, 3));
                Assert.That(data.winQuotes.All(quote => !string.IsNullOrWhiteSpace(quote)), Is.True);
                Assert.That(data.loseQuotes.All(quote => !string.IsNullOrWhiteSpace(quote)), Is.True);
                Assert.That(data.winQuotes.Distinct().Count(), Is.EqualTo(data.winQuotes.Length));
                Assert.That(data.loseQuotes.Distinct().Count(), Is.EqualTo(data.loseQuotes.Length));
                Assert.That(data.isPaidDLC, Is.False);
                Assert.That(data.maxHealth, Is.EqualTo(100));
                Assert.That(data.moveSpeed, Is.EqualTo(5));
                Assert.That(data.jumpForce, Is.EqualTo(8));
                Assert.That(data.lightAttackDamage, Is.EqualTo(5));
                Assert.That(data.heavyAttackDamage, Is.EqualTo(10));
            }
        }

        [Test]
        public void BaseRosterContainsExactlyNineCharacters()
        {
            int baseCount = AssetDatabase.FindAssets("t:CharacterData", new[] { "Assets/Characters" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<CharacterData>(AssetDatabase.GUIDToAssetPath(guid)))
                .Count(data => data != null && !data.isPaidDLC);
            Assert.That(baseCount, Is.EqualTo(9));
        }
    }
}
