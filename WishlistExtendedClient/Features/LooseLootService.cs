using EFT;
using EFT.UI;
using WishlistExtended.Config;
using ZGFueDkx.ZGCLib.Config;

namespace WishlistExtended.Features
{
    internal static class LooseLootService
    {
        public static void ApplyWishlistText(MongoID itemId, ref string itemName)
        {
            if (!ItemUiContext.Instance.WishlistManager.IsInWishlist(itemId, true, out _))
            {
                return;
            }

            var lootAffix = Settings.LooseLootAffix!.Value;
            var colorMode = Settings.LooseLootColor!.Value;

            if (lootAffix == Settings.ELooseLootAffix.Disabled && colorMode == Settings.ELooseLootColorMode.Disabled)
            {
                return;
            }

            itemName = itemName.Localized();

            string colorValue = Settings.LooseLootColorValue!.GetHexColor();

            if (colorMode == Settings.ELooseLootColorMode.Both || colorMode == Settings.ELooseLootColorMode.Name)
            {
                itemName = $"<color={colorValue}>{itemName}</color>";
            }

            if (lootAffix == Settings.ELooseLootAffix.Disabled)
            {
                return;
            }

            string affix = Settings.LooseLootAffixValue!.Value;

            if (colorMode == Settings.ELooseLootColorMode.Both || colorMode == Settings.ELooseLootColorMode.Affix)
            {
                affix = $"<color={colorValue}>{affix}</color>";
            }

            itemName = lootAffix == Settings.ELooseLootAffix.Prefix ? $"{affix} {itemName}" : $"{itemName} {affix}";
        }
    }
}
