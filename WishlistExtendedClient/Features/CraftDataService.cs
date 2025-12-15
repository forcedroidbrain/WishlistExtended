using Comfort.Common;
using EFT;
using EFT.Hideout;
using System.Collections.Generic;
using System.Diagnostics;
using WishlistExtended.Config;
using WishlistExtended.Models;

namespace WishlistExtended.Features
{
    internal static class CraftDataService
    {
        private static bool _dirty = false;
        private static readonly Dictionary<MongoID, List<WishlistCraftData>> _cachedCrafts = [];

        public static Dictionary<MongoID, List<WishlistCraftData>> Crafts
        {
            get
            {
                if (_dirty)
                {
                    _dirty = false;
                    Stopwatch sw = Stopwatch.StartNew();
                    BuildCrafts();
                    Plugin.LogDebug($"Crafts build took {sw.Elapsed.TotalMilliseconds} ms");
                }

                return _cachedCrafts;
            }
        }

        public static void InvalidateCache() => _dirty = true;

        private static void BuildCrafts()
        {
            _cachedCrafts.Clear();

            if (!Singleton<HideoutClass>.Instantiated)
            {
                Plugin.LogSource?.LogError("CraftDataService.BuildCrafts: Singleton HideoutClass is not instantiated");
            }

            Plugin.LogDebug("Building hideout crafts");

            HideoutClass hideout = Singleton<HideoutClass>.Instance;

            HashSet<MongoID> favorites = [];
            foreach (var scheme in hideout.FavoriteProductionSchemes)
            {
                favorites.Add(scheme._id);
            }

            var craftMode = Settings.CraftMode!.Value;
            bool showFuture = craftMode == Settings.ECraftMode.All;
            bool favoriteOnly = craftMode == Settings.ECraftMode.Favorite;

            List<AreaData> hideoutAreas = hideout.AreaDatas;

            if (hideoutAreas.Count == 0)
            {
                Plugin.LogDebug("Trying to build crafts too early");
                _dirty = true;
                return;
            }

            foreach (AreaData area in hideoutAreas)
            {
                if (!area.Enabled)
                {
                    Plugin.LogDebug($"Skip area {area.Template.Type} ({area.Template.Id}) - not enabled");
                    continue;
                }

                int stageLevel = area.CurrentStage.Level;

                if (showFuture)
                {
                    while (area.StageAt(stageLevel + 1).Level > 0)
                    {
                        stageLevel++;
                    }
                }

                Stage stage = area.StageAt(stageLevel);

                Plugin.LogDebug($"Processing area: {area.Template.Type} ({area.Template.Id}), status: {area.Status}, stage: {stage.Level}");

                if (stage.Level == 0)
                {
                    Plugin.LogDebug("\tSkipping stage - level 0");
                    continue;
                }

                if (stage.Production.Data is null)
                {
                    Plugin.LogDebug("\tSkipping stage - production data null");
                    continue;
                }

                foreach (ProductionBuildAbstractClass production in stage.Production.Data)
                {
                    Plugin.LogDebug($"\tProcessing production: {production._id}");

                    if (production is not GClass2440 itemProduction)
                    {
                        Plugin.LogDebug($"\t\tProduction is not item production - skip");
                        continue;
                    }

                    bool isFavorite = favorites.Contains(itemProduction._id);
                    if (favoriteOnly && !isFavorite)
                    {
                        Plugin.LogDebug($"\t\tProduction is not in favorites - skip");
                        continue;
                    }

                    foreach (Requirement req in itemProduction.requirements)
                    {
                        if (req is not ItemRequirement itemReq)
                        {
                            Plugin.LogDebug($"\t\t\tNot item requirement: {req.Type} - skip");
                            continue;
                        }

                        if (!GClass2067.IsAvailableForWishlist(itemReq.Item))
                        {
                            Plugin.LogDebug($"\t\t\tItem {itemReq.TemplateId} is not suitable for wishlist");
                            continue;
                        }

                        if (!_cachedCrafts.TryGetValue(itemReq.TemplateId, out var productionItems))
                        {
                            productionItems = [];
                            _cachedCrafts.Add(itemReq.TemplateId, productionItems);
                        }

                        productionItems.Add(new(
                            itemProduction.endProduct,
                            itemReq.IntCount,
                            area.Template.Type,
                            itemProduction.Level,
                            itemProduction.Level > area.CurrentLevel,
                            isFavorite
                        ));

                        Plugin.LogDebug($"\t\t\tCraft added: " +
                            $"{itemReq.TemplateId} ({itemReq.IntCount}) -> {itemProduction.endProduct} ({area.Template.Type} {stage.Level})");
                    }
                }
            }

            Plugin.LogSource?.LogInfo($"Loaded crafts - {_cachedCrafts.Count} unique items");
        }
    }
}
