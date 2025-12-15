using Comfort.Common;
using EFT;
using EFT.Hideout;
using System.Collections.Generic;
using System.Diagnostics;
using WishlistExtended.Config;
using WishlistExtended.Models;

namespace WishlistExtended.Features
{
    internal static class HideoutDataService
    {
        private static bool _dirty = false;
        private static readonly Dictionary<MongoID, List<WishlistHideoutData>> _cachedRequirements = [];

        public static Dictionary<MongoID, List<WishlistHideoutData>> HideoutRequirements
        {
            get
            {
                if (_dirty)
                {
                    _dirty = false;
                    Stopwatch sw = Stopwatch.StartNew();
                    BuildHideoutRequirements();
                    Plugin.LogDebug($"Hideout build took {sw.Elapsed.TotalMilliseconds} ms");
                }

                return _cachedRequirements;
            }
        }

        public static void InvalidateCache() => _dirty = true;

        private static void BuildHideoutRequirements()
        {
            _cachedRequirements.Clear();

            if (!Singleton<HideoutClass>.Instantiated)
            {
                Plugin.LogSource?.LogError("HideoutDataService.BuildHideoutRequirements: Singleton HideoutClass is not instantiated");
            }

            Plugin.LogDebug("Building hideout requirements");

            List<AreaData> hideoutAreas = Singleton<HideoutClass>.Instance.AreaDatas;

            if (hideoutAreas.Count == 0)
            {
                Plugin.LogDebug("Trying to build hideout requirements too early");
                _dirty = true;
                return;
            }

            var hideoutMode = Settings.HideoutMode!.Value;
            bool hideFuture = hideoutMode != Settings.EHideoutMode.All;
            bool excludeLocked = hideoutMode == Settings.EHideoutMode.ExcludeLocked;

            foreach (AreaData area in hideoutAreas)
            {
                Plugin.LogDebug($"Processing area: {area.Template.Type} ({area.Template.Id}), status: {area.Status}, enabled: {area.Enabled}");

                if (!area.Enabled)
                {
                    continue;
                }

                Stage stage = area.CurrentStage;
                while ((stage = area.StageAt(stage.Level + 1)).Level > 0)
                {
                    Plugin.LogDebug($"\tProcessing stage: {stage.Level}");

                    bool isCurrentStage = stage.Level == area.CurrentLevel + 1;

                    if (isCurrentStage &&
                        (area.Status == EAreaStatus.Constructing ||
                            area.Status == EAreaStatus.Upgrading ||
                            area.Status == EAreaStatus.ReadyToInstallConstruct ||
                            area.Status == EAreaStatus.ReadyToInstallUpgrade))
                    {
                        Plugin.LogDebug($"\t\tUpgrade in progress - skip");
                        continue;
                    }

                    if (excludeLocked)
                    {
                        bool skip = false;

                        foreach (Requirement req in stage.Requirements)
                        {
                            if (req is not ItemRequirement && !req.Fulfilled && excludeLocked)
                            {
                                Plugin.LogDebug($"\t\tRequirements not fulfilled {req.Type} - skip");
                                skip = true;
                                break;
                            }
                        }

                        if (skip)
                        {
                            continue;
                        }
                    }

                    foreach (Requirement req in stage.Requirements)
                    {
                        Plugin.LogDebug($"\t\tProcessing requirement: {req.Type}");

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

                        if (!_cachedRequirements.TryGetValue(itemReq.TemplateId, out var itemUpgrades))
                        {
                            itemUpgrades = [];
                            _cachedRequirements.Add(itemReq.TemplateId, itemUpgrades);
                        }

                        itemUpgrades.Add(new(
                            area.Template.Type,
                            stage.Level,
                            itemReq.IntCount,
                            itemReq.IsSpawnedInSession,
                            !isCurrentStage
                        ));

                        Plugin.LogDebug($"\t\t\tRequirement added: " +
                            $"{itemReq.TemplateId} ({itemReq.IntCount}) -> {area.Template.Type} ({stage.Level}, FiR: {itemReq.IsSpawnedInSession}, Future: {!isCurrentStage})");
                    }

                    if (hideFuture)
                    {
                        Plugin.LogDebug($"\t\tNo future - break");
                        break;
                    }
                }
            }

            Plugin.LogSource?.LogInfo($"Loaded hideout requirements - {_cachedRequirements.Count} unique items");
        }
    }
}
