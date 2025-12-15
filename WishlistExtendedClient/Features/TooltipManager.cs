using EFT.InventoryLogic;
using EFT.UI;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using System;
using System.Text;
using TMPro;
using WishlistExtended.Config;
using WishlistExtended.Models;
using ZGFueDkx.ZGCLib.Config;

namespace WishlistExtended.Features
{
    internal static class TooltipManager
    {
        private static bool _fromQuest = false;

        private static Item? _item;
        private static WishlistData? _wishlist;

        private static SimpleTooltip? _tooltip;
        private static string? _tooltipText;

        private static int _tooltipCycle = 0;

        private static readonly AccessTools.FieldRef<SimpleTooltip, TextMeshProUGUI> _labelRef =
            AccessTools.FieldRefAccess<SimpleTooltip, TextMeshProUGUI>("_label");

        public static void OnItemShowTooltip(GridItemView gridItem)
        {
            if (WishlistResolver.ResolveWhishlist().TryGetValue(gridItem.Item.TemplateId, out _wishlist))
            {
                _item = gridItem.Item;
            }
        }

        public static void OnQuestShowTooltip()
        {
            _fromQuest = true;
            _tooltip = null;
        }

        public static void OnItemExit() => Cleanup();

        public static void OnTooltipShow(SimpleTooltip tooltip, ref string text)
        {
            if (_item is null)
            {
                return;
            }

            if (_fromQuest)
            {
                _fromQuest = false;
                return;
            }

            _tooltip = tooltip;

            string? tooltipText = MakeTooltip();
            if (tooltipText is null)
            {
                _tooltipText = "";
                return;
            }

            _tooltipText = tooltipText;
            text += tooltipText;
        }

        public static void OnTooltipUpdate(SimpleTooltip tooltip)
        {
            if (_item is null || _tooltip is null || _tooltipText is null)
            {
                return;
            }

            if (tooltip != _tooltip)
            {
                Cleanup();
                return;
            }

            if (Settings.CycleTooltipBind!.Value.IsDown())
            {
                _tooltipCycle = (_tooltipCycle + 1) % 3;
            }

            string? tooltipText = MakeTooltip() ?? "";

            if (_tooltipText.Equals(tooltipText, StringComparison.Ordinal))
            {
                return;
            }

            TextMeshProUGUI? label = _labelRef(tooltip);
            if (label is null)
            {
                return;
            }

            if (_tooltipText.Length == 0)
            {
                label.text += tooltipText;
            }
            else
            {
                _tooltip.SetText(label.text.Replace(_tooltipText, tooltipText, StringComparison.Ordinal));
            }

            _tooltipText = tooltipText;
        }

        private static void Cleanup()
        {
            _fromQuest = false;
            _item = null;
            _wishlist = null;
            _tooltip = null;
            _tooltipText = null;
        }

        private static string? MakeTooltip()
        {
            var tooltipMode = Settings.TooltipMode!.Value;
            if (tooltipMode == Settings.ETooltipMode.Disabled)
            {
                return null;
            }

            bool showAll = Settings.ShowAllTooltipBind!.Value.IsPressed();

            return tooltipMode switch
            {
                Settings.ETooltipMode.Disabled => null,

                Settings.ETooltipMode.Hide when showAll => FormatFull(),
                Settings.ETooltipMode.Hide => null,

                Settings.ETooltipMode.HideFuture when showAll => FormatFull(),
                Settings.ETooltipMode.HideFuture => FormatShort(),

                Settings.ETooltipMode.Cycle when showAll => FormatFull(),
                Settings.ETooltipMode.Cycle => FormatCycle(),

                Settings.ETooltipMode.Always => FormatFull(),

                _ => null
            };
        }

        private static string? FormatFull()
        {
            StringBuilder result = new();

            if (_wishlist!.HasCraft)
            {
                AppendCrafts(result, true);
            }

            if (_wishlist.HasHideout)
            {
                AppendHideout(result, true);
            }

            if (_wishlist.HasBarter)
            {
                AppendBarters(result);
            }

            if (result.Length == 0)
            {
                return null;
            }

            string prefix = "\n";

            if (Settings.ShowStashCount!.Value)
            {
                prefix += GetInStashText() + "\n";
            }

            return prefix + result.ToString();
        }

        private static string? FormatShort()
        {
            StringBuilder result = new();

            if (_wishlist!.HasCraft)
            {
                AppendCrafts(result, false);
            }

            if (_wishlist.HasHideout)
            {
                AppendHideout(result, false);
            }

            if (_wishlist.HasBarter)
            {
                AppendBarters(result);
            }

            if (result.Length == 0)
            {
                return null;
            }

            string prefix = "\n";

            if (Settings.ShowStashCount!.Value)
            {
                prefix += GetInStashText() + "\n";
            }

            return prefix + result.ToString();
        }

        private static string? FormatCycle()
        {
            StringBuilder result = new();

            switch (_tooltipCycle)
            {
                case 0: AppendCrafts(result, true); break;
                case 1: AppendHideout(result, true); break;
                case 2: AppendBarters(result); break;
            }

            if (result.Length == 0)
            {
                return null;
            }

            string prefix = "\n";

            if (Settings.ShowStashCount!.Value)
            {
                prefix += GetInStashText() + "\n";
            }

            return prefix + result.ToString();
        }

        private static string GetInStashText()
        {
            if (!StashService.Cache.TryGetValue(_item!.TemplateId, out int count))
            {
                count = 0;
            }

            return $"{"wext_in_stash".Localized()} <color=#20f020>{count}</color>";
        }

        private static void AppendCrafts(StringBuilder sb, bool includeFuture)
        {
            StringBuilder favoriteCrafts = new();
            StringBuilder currentCrafts = new();
            StringBuilder futureCrafts = new();

            string favoriteColor = Settings.FavoriteCraftsColor!.GetHexColor();
            string currentColor = Settings.CurrentCraftsColor!.GetHexColor();
            string futureColor = Settings.FutureCraftsColor!.GetHexColor();

            foreach (var craft in _wishlist!.Crafts)
            {
                if (!includeFuture && craft.IsFuture)
                {
                    continue;
                }

                AppendCraft(
                    craft.IsFavorite ? favoriteCrafts : (craft.IsFuture ? futureCrafts : currentCrafts),
                    craft,
                    craft.IsFavorite ? favoriteColor : (craft.IsFuture ? futureColor : currentColor)
                );
            }

            if (favoriteCrafts.Length > 0)
            {
                sb.AppendLine("wext_favorite_crafts".Localized());
                sb.Append(favoriteCrafts);
            }

            if (currentCrafts.Length > 0)
            {
                sb.AppendLine(favoriteCrafts.Length > 0 ? "wext_other_crafts".Localized() : "wext_current_crafts".Localized());
                sb.Append(currentCrafts);
            }

            if (futureCrafts.Length > 0)
            {
                sb.AppendLine("wext_future_crafts".Localized());
                sb.Append(futureCrafts);
            }
        }

        private static void AppendHideout(StringBuilder sb, bool includeFuture)
        {
            StringBuilder currentAreas = new();
            StringBuilder futureAreas = new();

            string currentColor = Settings.CurrentHideoutColor!.GetHexColor();
            string futureColor = Settings.FutureHideoutColor!.GetHexColor();

            int currentCount = 0;
            int futureCount = 0;

            foreach (var upgrade in _wishlist!.Hideout)
            {
                if (!includeFuture && upgrade.IsFuture)
                {
                    continue;
                }

                if (upgrade.Fir && !_item!.SpawnedInSession)
                {
                    continue;
                }

                if(upgrade.IsFuture)
                {
                    AppendArea(
                        futureAreas,
                        upgrade,
                        futureColor,
                        ref futureCount
                    );
                }
                else
                {
                    AppendArea(
                        currentAreas,
                        upgrade,
                        currentColor,
                        ref currentCount
                    );
                }
            }

            if (currentAreas.Length > 0)
            {
                sb.Append("wext_current_upgrades".Localized())
                    .Append(" (")
                    .Append(currentCount)
                    .AppendLine(")")
                    .Append(currentAreas);
            }

            if (futureAreas.Length > 0)
            {
                sb.Append("wext_future_upgrades".Localized())
                    .Append(" (")
                    .Append(futureCount)
                    .AppendLine(")")
                    .Append(futureAreas);
            }
        }

        private static void AppendBarters(StringBuilder sb)
        {
            StringBuilder barters = new();
            string color = Settings.BarterColor!.GetHexColor();

            foreach (var barter in _wishlist!.Barters)
            {
                AppendBarter(
                    barters,
                    barter,
                    color
                );
            }

            if (barters.Length > 0)
            {
                sb.AppendLine("wext_barters".Localized());
                sb.Append(barters);
            }
        }

        private static void AppendCraft(StringBuilder sb, WishlistCraftData data, string color)
        {
            sb.Append("  ")
                .Append(data.Quantity)
                .Append($" -> <color=")
                .Append(color)
                .Append('>')
                .Append($"{data.ResultId} ShortName".Localized())
                .Append("</color> @ <color=")
                .Append(color)
                .Append('>')
                .Append(data.AreaType.LocalizeAreaName())
                .Append(" lvl ")
                .Append(data.AreaStage.ToString())
                .AppendLine("</color>");
        }

        private static void AppendArea(StringBuilder sb, WishlistHideoutData data, string color, ref int count)
        {
            sb.Append("  ")
                .Append(data.Quantity)
                .Append(" -> <color=")
                .Append(color)
                .Append('>')
                .Append(data.AreaType.LocalizeAreaName())
                .Append(" lvl ")
                .Append(data.Stage.ToString())
                .AppendLine("</color>");

            count += data.Quantity;
        }

        private static void AppendBarter(StringBuilder sb, WishlistBarterData data, string color)
        {
            sb.Append("  ")
                .Append(data.Quantity)
                .Append(" -> <color=")
                .Append(color)
                .Append('>')
                .Append($"{data.ResultId} ShortName".Localized())
                .Append("</color> @ <color=")
                .Append(color)
                .Append('>')
                .Append($"{data.TraderId} Nickname".Localized())
                .Append(" LL")
                .Append(data.LoyaltyLevel.ToString())
                .AppendLine("</color>");
        }
    }
}
