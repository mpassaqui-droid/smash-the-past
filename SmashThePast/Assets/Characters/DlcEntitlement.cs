using UnityEngine;

namespace SmashThePast.Characters
{
    public interface IDlcEntitlementProvider
    {
        bool IsEntitled(string storeSku);
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
