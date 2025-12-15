using EFT;
using Newtonsoft.Json;

namespace WishlistExtended.Models
{
    internal class BarterData
    {
        [JsonProperty("i_id")]
        public MongoID? ItemId { get; set; }
        [JsonProperty("r_id")]
        public MongoID? ResultId { get; set; }
        [JsonProperty("qty")]
        public int? Quantity { get; set; }
        [JsonProperty("ll")]
        public int? LoyaltyLevel { get; set; }
    }
}
