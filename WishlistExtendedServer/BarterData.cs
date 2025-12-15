using SPTarkov.Server.Core.Models.Common;
using System.Text.Json.Serialization;

namespace WishlistExtended
{
    internal class BarterData(MongoId itemId, MongoId resultId, int quantity, int loyaltyLevel = 1)
    {
        [JsonPropertyName("i_id")]
        public MongoId ItemId { get; set; } = itemId;
        [JsonPropertyName("r_id")]
        public MongoId ResultId { get; set; } = resultId;
        [JsonPropertyName("qty")]
        public int Quantity { get; set; } = quantity;
        [JsonPropertyName("ll")]
        public int LoyaltyLevel { get; set; } = loyaltyLevel;
    }
}
