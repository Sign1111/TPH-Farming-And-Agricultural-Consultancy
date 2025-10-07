using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System;

namespace Ade_Farming.Models
{
    [CollectionName("PlantainSuckerPosts")]
    public class PlantainSuckerPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string SellerId { get; set; }
        public string SellerName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
