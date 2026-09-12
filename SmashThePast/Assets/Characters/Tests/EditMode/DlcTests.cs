using NUnit.Framework;
using SmashThePast.Core;
using UnityEditor;
using UnityEngine;

namespace SmashThePast.Characters.Tests
{
    public class DlcTests
    {
        [Test]
        public void DlcOfferIsPaidAndFailsClosedWithoutStoreEntitlement()
        {
            var character = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Characters/Hitler.asset");
            var offer = AssetDatabase.LoadAssetAtPath<DlcOffer>("Assets/Characters/HitlerDlc.asset");
            Assert.That(character, Is.Not.Null);
            Assert.That(character.isPaidDLC, Is.True);
            Assert.That(character.specialName, Is.EqualTo("Berlin Tantrum"));
            Assert.That(offer, Is.Not.Null);
            Assert.That(offer.character, Is.EqualTo(character));
            Assert.That(offer.minimumAge, Is.EqualTo(15));
            Assert.That(new SteamDlcEntitlementProvider().IsEntitled(""), Is.False);
        }

        [Test]
        public void DlcPrefabUsesNeutralPresentationAndExternalAnimation()
        {
            for (int slot = 1; slot <= 2; slot++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Characters/Prefabs/Hitler_P{slot}.prefab");
                Assert.That(prefab, Is.Not.Null);
                Assert.That(prefab.GetComponent<Combatant>().Kind, Is.EqualTo(SpecialKind.Hitler));
                Assert.That(prefab.GetComponent<FighterAnimation>(), Is.Not.Null);
                Assert.That(prefab.transform.Find("Modelo/Bicornio"), Is.Null);
                Assert.That(prefab.transform.Find("Modelo/Escarapela"), Is.Null);
            }
        }
    }
}
