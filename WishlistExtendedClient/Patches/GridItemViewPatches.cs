using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using WishlistExtended.Features;

namespace WishlistExtended.Patches
{
    internal class GridItemViewShowTooltipPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GridItemView), nameof(GridItemView.ShowTooltip));
        }

        [PatchPrefix]
        private static void Prefix(GridItemView __instance)
        {
            TooltipManager.OnItemShowTooltip(__instance);
        }
    }

    internal class GridItemExitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GridItemView), nameof(GridItemView.OnPointerExit));
        }

        [PatchPrefix]
        private static void Prefix()
        {
            TooltipManager.OnItemExit();
        }
    }
}
