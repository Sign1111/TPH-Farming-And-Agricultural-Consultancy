using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System;

namespace Ade_Farming.Models
{
    [CollectionName("CocoaPalmSeedlingPosts")]
    public class CocoaPalmSeedlingPost
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; } // Stored in DB
        public string SellerId { get; set; }
        public string SellerName { get; set; }
        public string SellerEmail { get; set; } // Use registered email
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
