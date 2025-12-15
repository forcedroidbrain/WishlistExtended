using EFT;
using System.Collections.Generic;
using WishlistExtended.Config;
using WishlistExtended.Models;
using ZGFueDkx.ZGCLib.helpers;

namespace WishlistExtended.Features
{
    internal static class WishlistResolver
    {
        public static bool IsInWishlist(MongoID templateId, out EWishlistGroup group)
        {
            if (ResolveWhishlist().TryGetValue(templateId, out WishlistData data))
            {
                group = Settings.BarterPriority!.Value && data.HasBarter ? EWishlistGroup.Trading : EWishlistGroup.Hideout;
                return true;
            }

            group = EWishlistGroup.Hideout;
            return false;
        }

        public static Dictionary<MongoID, EWishlistGroup> GetWishlist()
        {
            Dictionary<MongoID, EWishlistGroup> wishlist = [];

            foreach (var (itemId, data) in ResolveWhishlist())
            {
                wishlist[itemId] = Settings.BarterPriority!.Value && data.HasBarter ? EWishlistGroup.Trading : EWishlistGroup.Hideout;
            }

            return wishlist;
        }

        public static Dictionary<MongoID, WishlistData> ResolveWhishlist()
        {
            if (Settings.RaidOnly!.Value && !RaidUtils.IsInRaid())
            {
                return [];
            }

            Dictionary<MongoID, WishlistData> wishlist = [];

            ResolveCrafts(wishlist);
            ResolveHideout(wishlist);
            ResolveBarters(wishlist);

            return wishlist;
        }

        public static void ResolveCrafts(Dictionary<MongoID, WishlistData> wishlist)
        {
            var craftMode = Settings.CraftMode!.Value;
            if (craftMode == Settings.ECraftMode.Disabled)
            {
                return;
            }

            var craftData = CraftDataService.Crafts;
            if (craftData is null)
            {
                Plugin.LogDebug("GetCrafts returned null");
                return;
            }

            foreach (var (itemId, craft) in craftData)
            {
                if (!wishlist.TryGetValue(itemId, out var wishlistData))
                {
                    wishlistData = new();
                    wishlist.Add(itemId, wishlistData);
                }

                wishlistData.Crafts.AddRange(craft);
            }
        }

        public static void ResolveHideout(Dictionary<MongoID, WishlistData> wishlist)
        {
            var hideoutMode = Settings.HideoutMode!.Value;
            if (hideoutMode == Settings.EHideoutMode.Disabled)
            {
                return;
            }

            var hideoutData = HideoutDataService.HideoutRequirements;
            if (hideoutData is null)
            {
                Plugin.LogDebug("GetHideoutRequirements returned null");
                return;
            }

            foreach (var (itemId, hideout) in hideoutData)
            {
                if (!wishlist.TryGetValue(itemId, out var wishlistData))
                {
                    wishlistData = new();
                    wishlist.Add(itemId, wishlistData);
                }

                wishlistData.Hideout.AddRange(hideout);
            }
        }

        public static void ResolveBarters(Dictionary<MongoID, WishlistData> wishlist)
        {
            if (!Settings.IncludeBarters!.Value)
            {
                return;
            }

            foreach (var (itemId, barters) in BarterDataService.Barters)
            {
                if (!wishlist.TryGetValue(itemId, out var wishlistData))
                {
                    wishlistData = new();
                    wishlist.Add(itemId, wishlistData);
                }

                wishlistData.Barters.AddRange(barters);
            }
        }
    }
}
