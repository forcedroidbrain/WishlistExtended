using BepInEx.Configuration;
using System;
using UnityEngine;
using WishlistExtended.Features;
using ZGFueDkx.ZGCLib.Config;

namespace WishlistExtended.Config
{
    internal static class Settings
    {
        public enum ECraftMode
        {
            Disabled,
            Favorite,
            Current,
            All,
        }

        public enum EHideoutMode
        {
            Disabled,
            Current,
            ExcludeLocked,
            All,
        }

        public enum ETooltipMode
        {
            Disabled,
            Always,
            Hide,
            Cycle,
            HideFuture,
        };

        public enum ELooseLootAffix
        {
            Disabled,
            Prefix,
            Suffix,
        };

        public enum ELooseLootColorMode
        {
            Disabled,
            Name,
            Affix,
            Both,
        }

        public static ConfigEntry<ECraftMode>? CraftMode;
        public static ConfigEntry<EHideoutMode>? HideoutMode;
        public static ConfigEntry<bool>? IncludeBarters;
        public static ConfigEntry<bool>? BarterPriority;
        //public static ConfigEntry<bool>? BarterFallback;
        public static ConfigEntry<bool>? ShowStashCount;
        public static ConfigEntry<bool>? RaidOnly;

        public static ConfigEntry<ETooltipMode>? TooltipMode;
        public static ConfigEntry<KeyboardShortcut>? ShowAllTooltipBind;
        public static ConfigEntry<KeyboardShortcut>? CycleTooltipBind;

        public static ConfigEntry<Color>? CurrentCraftsColor;
        public static ConfigEntry<Color>? FavoriteCraftsColor;
        public static ConfigEntry<Color>? FutureCraftsColor;
        public static ConfigEntry<Color>? CurrentHideoutColor;
        public static ConfigEntry<Color>? FutureHideoutColor;
        public static ConfigEntry<Color>? BarterColor;

        public static ConfigEntry<ELooseLootAffix>? LooseLootAffix;
        public static ConfigEntry<ELooseLootColorMode>? LooseLootColor;
        public static ConfigEntry<Color>? LooseLootColorValue;
        public static ConfigEntry<string>? LooseLootAffixValue;


        public static ConfigEntry<bool>? ShowDebug;

        public static void Init(ConfigFile config)
        {
            ConfigCategory general = config.MakeCategory(1, "General");
            ConfigCategory tooltip = config.MakeCategory(2, "Tooltip");
            ConfigCategory colors = config.MakeCategory(3, "Colors");
            ConfigCategory loose = config.MakeCategory(4, "Loose Loot");
            ConfigCategory debug = config.MakeCategory(9, "Debug");

            /*
             * GENERAL
             */
            CraftMode = general.BindConfig(
                "Hideout crafting wishlist mode",
                ECraftMode.All,
                "Specifies which crafts should be wishlisted:\n" +
                "Disabled - don't include any crafts\n" +
                "Favorite - include only favorite crafts\n" +
                "Current - include all currently available crafts\n" +
                "All - include all crafts (even future ones)"
            );

            CraftMode.SettingChanged += (_, _) => WishlistService.Rebuild();

            HideoutMode = general.BindConfig(
                "Hideout upgrades wishlist mode",
                EHideoutMode.All,
                "Specifies which hideout upgrades should be wishlisted:\n" +
                "Disabled - don't include any upgrades\n" +
                "Current - include all currently available upgrades\n" +
                "ExcludeLocked - same as current but exclude upgrades with missing conditions\n" +
                "All - include all hideout upgrades (even future ones)"
            );

            HideoutMode.SettingChanged += (_, _) => WishlistService.Rebuild();

            IncludeBarters = general.BindConfig(
                "Include barters",
                true,
                "Whether or not to include barters. Mod must be installed on the server for barters wishlist!"
            );

            IncludeBarters.SettingChanged += (_, _) => WishlistService.Rebuild();

            BarterPriority = general.BindConfig(
                "Barter priority",
                false,
                "Whether barters should be prioritized over hideout. Only affects displayed icon"
            );

            /*BarterFallback = general.BindConfig(
                "Barter fallback",
                true,
                "Whether mod should try to fetch barters in other ways if this mod is not installed on the server"
            );*/

            ShowStashCount = general.BindConfig(
                "Show amount of items in stash",
                true,
                "Whether total amount of items in stash should be displayed in the tooltip"
            );

            RaidOnly = general.BindConfig(
                "In-Raid only",
                false,
                "Whether the auto-generated wishlist should work only in raid. Manual wishlist will work regardless"
            );

            RaidOnly.SettingChanged += (_, _) => WishlistService.Rebuild();

            /*
             * TOOLTIP
             */
            TooltipMode = tooltip.BindConfig(
                "Tooltip mode",
                ETooltipMode.Always,
                "Defines the behavior of tooltip:\n" +
                "Disabled - no wishlist details in the tooltip\n" +
                "Always - no binds, everythig is shown\n" +
                "Hide - don't show anything, use 'Show all' bind to show\n" +
                "Cycle - only one category, use 'Cycle tooltip' bind to change, use 'Show all' bind to show all\n" +
                "HideFuture - hide future crafts/modules, use 'Show all' bind to show them"
            );

            ShowAllTooltipBind = tooltip.BindConfig(
                "Show all bind",
                new KeyboardShortcut(KeyCode.LeftShift),
                "Shows info hidden by currently selected 'Tooltip mode'"
            );

            CycleTooltipBind = tooltip.BindConfig(
                "Cycle tooltip bind",
                new KeyboardShortcut(KeyCode.LeftAlt),
                "When pressed, ctagory displayed in tooltip is changed (Cycle tooltip mode only)"
            );

            /*
             * COLORS
             */
            CurrentCraftsColor = colors.BindColor(
                "Current crafts color",
                "#be99ff",
                "Color of current crafts in tooltip"
            );

            FavoriteCraftsColor = colors.BindColor(
                "Favorite crafts color",
                "#d7b442",
                "Color of favorite crafts in tooltip"
            );

            FutureCraftsColor = colors.BindColor(
                "Future crafts color",
                "#7e33ff",
                "Color of future crafts in tooltip"
            );

            CurrentHideoutColor = colors.BindColor(
                "Current hideout upgrades color",
                "#ff8080",
                "Color of current hideout upgrades in tooltip"
            );

            FutureHideoutColor = colors.BindColor(
                "Future hideout upgrades color",
                "#ff1a1a",
                "Color of future hideout upgrades in tooltip"
            );

            BarterColor = colors.BindColor(
                "Barters color",
                "#5cd65c",
                "Color of barters in tooltip"
            );

            /*
             * LOOSE
             */
            LooseLootAffix = loose.BindConfig(
                "Loose loot affix mode",
                ELooseLootAffix.Suffix,
                "Specifies how to add the affix to loose loot names:\n" +
                "Disabled - do nothing\n" +
                "Prefix - show the affix before the item name\n" +
                "Suffix - show the affix after the item name"
            );

            LooseLootColor = loose.BindConfig(
                "Loose loot color mode",
                ELooseLootColorMode.Affix,
                "Specifies how loose loot names will be colored:\n" +
                "Disabled - do nothing\n" +
                "Name - set the color of the item name\n" +
                "Affix - set the color of the affix\n" +
                "Both - set the color of both the item name and the affix"
            );

            LooseLootColorValue = loose.BindColor(
                "Loose loot color",
                "#ffff33",
                "Color of the item name (only in Color mode)"
            );

            LooseLootAffixValue = loose.BindConfig(
                "Loose loot affix",
                "✦",
                "Affix to show next to the item name when enabled"
            );

            /*
             * Debug
             */
            ShowDebug = debug.BindConfig(
                "Debug logs",
                false,
                "Enable debug logs in Player.log"
            );

            debug.BindButton("Rebuild caches", "Rebuild", "Forces rebuild of all caches", () =>
            {
                BarterDataService.LoadBarters();
                WishlistService.Rebuild();
            });

            Plugin.LogSource?.LogInfo("Settings loaded");
        }
    }
}
