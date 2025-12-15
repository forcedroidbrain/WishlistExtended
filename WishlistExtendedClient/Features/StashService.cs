using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Utils;
using System.Collections.Generic;
using System.Linq;

namespace WishlistExtended.Features
{
    internal static class StashService
    {
        private static readonly Dictionary<MongoID, int> _stashCache = [];

        public static Dictionary<MongoID, int> Cache => _stashCache;

        public static void BuildCache()
        {
            _stashCache.Clear();

            Profile profile = ClientAppUtils.GetClientApp().GetClientBackEndSession().Profile;
            IEnumerable<Item> itemsToCache = profile.Inventory.GetPlayerItems(EPlayerItems.HideoutStashes);
            IEnumerable<Item>? stashItems = Singleton<HideoutClass>.Instance?.AllStashItems;

            if (stashItems is not null)
            {
                itemsToCache = itemsToCache.Concat(stashItems);
            }

            foreach (Item item in itemsToCache)
            {
                if (_stashCache.TryGetValue(item.TemplateId, out int itemsCount))
                {
                    _stashCache[item.TemplateId] = itemsCount + item.StackObjectsCount;
                }
                else
                {
                    _stashCache[item.TemplateId] = item.StackObjectsCount;
                }
            }

            Plugin.LogSource?.LogInfo($"Stash cache built - total items in cache: {_stashCache.Count}");
        }
    }
}
