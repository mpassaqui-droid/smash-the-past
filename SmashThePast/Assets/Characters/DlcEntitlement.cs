using UnityEngine;

namespace SmashThePast.Characters
{
    public interface IDlcEntitlementProvider
    {
        bool IsEntitled(string storeSku);
    }

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

    public sealed class SteamDlcEntitlementProvider : IDlcEntitlementProvider
    {
        public bool IsEntitled(string storeSku)
        {
#if STEAMWORKS_NET
            if (!uint.TryParse(storeSku, out uint appId)) return false;
            return Steamworks.SteamApps.BIsDlcInstalled(new Steamworks.AppId_t(appId));
#else
            // Fail closed until the Steamworks package and app configuration are installed.
            return false;
#endif
        }
    }
}
