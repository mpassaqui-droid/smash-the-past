using UnityEngine;

namespace SmashThePast.Characters
{
    public sealed class DlcEntitlementService : MonoBehaviour
    {
        [SerializeField] private DlcOffer[] offers;
        private IDlcEntitlementProvider provider;

        public DlcOffer[] Offers => offers;

        private void Awake() => provider = new SteamDlcEntitlementProvider();

        public bool IsUnlocked(DlcOffer offer)
        {
            return offer != null && offer.character != null && offer.character.isPaidDLC
                && provider != null && provider.IsEntitled(offer.storeSku);
        }
    }
}
