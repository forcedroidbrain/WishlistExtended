using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace WishlistExtended
{
    [Injectable(typePriority: OnLoadOrder.PostDBModLoader + 1)]
    internal class WishlistExtendedMod(
        HttpResponseUtil httpResponseUtil,
        DatabaseService databaseService,
        ISptLogger<WishlistExtendedMod> logger
    ) : IOnLoad
    {
        private static readonly List<MongoId> MoneyIds = [
            "5449016a4bdc2d6f028b456f",
            "5696686a4bdc2da3298b456a",
            "569668774bdc2da2298b4568"
        ];

        private readonly HttpResponseUtil _httpResponseUtil = httpResponseUtil;
        private readonly DatabaseService _databaseService = databaseService;
        private readonly ISptLogger<WishlistExtendedMod> _logger = logger;

        public Task OnLoad()
        {
            return Task.CompletedTask;
        }

        public ValueTask<string> GetBarters()
        {
            Dictionary<MongoId, List<BarterData>> barters = [];
            var allTraders = _databaseService.GetTraders();

            foreach (var (traderId, trader) in allTraders)
            {
                List<BarterData> current = [];
                barters.Add(traderId, current);

                TraderAssort assort = trader.Assort;

                foreach (var (tradeId, barterSchemes) in assort.BarterScheme)
                {
                    bool hasLoyaltyLvel = assort.LoyalLevelItems.TryGetValue(tradeId, out int loyaltyLevel);

                    foreach (var shcemeList in barterSchemes)
                    {
                        foreach(var scheme in shcemeList)
                        {
                            if (MoneyIds.Contains(scheme.Template))
                            {
                                continue;
                            }

                            Item? foundItem = assort.Items.Find(i => i.Id == tradeId);
                            if (foundItem is null)
                            {
                                continue;
                            }

                            current.Add(new BarterData(scheme.Template, foundItem.Template, (int?)scheme.Count ?? 1, hasLoyaltyLvel ? loyaltyLevel : 1));
                        }
                    }
                }
            }

            return new ValueTask<string>(_httpResponseUtil.NoBody(barters));
        }
    }
}
