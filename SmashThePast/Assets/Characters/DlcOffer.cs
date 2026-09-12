using SmashThePast.Core;
using UnityEngine;

namespace SmashThePast.Characters
{
    [CreateAssetMenu(fileName = "NewDlcOffer", menuName = "Smash the Past/DLC Offer")]
    public sealed class DlcOffer : ScriptableObject
    {
        public string offerId;
        public string storeSku;
        public string displayName;
        [Min(0)] public float priceUsd = 0.99f;
        [Min(0)] public int minimumAge = 15;
        public CharacterData character;
    }
}
