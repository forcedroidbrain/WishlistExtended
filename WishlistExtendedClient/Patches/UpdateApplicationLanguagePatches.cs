using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using WishlistExtended.Features;

namespace WishlistExtended.Patches
{
    class UpdateApplicationLanguagePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LocaleManagerClass), nameof(LocaleManagerClass.UpdateApplicationLanguage));
        }

        [PatchPostfix]
        private static void Postfix()
        {
            LocalizationService.LoadLocale(LocaleManagerClass.LocaleManagerClass.String_0);
        }
    }
}
